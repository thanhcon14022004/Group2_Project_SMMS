using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/classes")]
    [Authorize]
    public class AdministrativeClassesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;

        public AdministrativeClassesController(AppDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        [HttpGet]
        [EnableQuery(PageSize = 100)]
        public async Task<IActionResult> GetClasses([FromQuery] int? majorId, [FromQuery] int? cohort)
        {
            var query = _db.AdministrativeClasses
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .Include(c => c.Students)
                .AsNoTracking()
                .AsQueryable();

            if (majorId.HasValue) query = query.Where(c => c.MajorId == majorId.Value);
            if (cohort.HasValue) query = query.Where(c => c.Cohort == cohort.Value);

            var classes = await query.ToListAsync();
            var courseSections = await _db.CourseSections
                .AsNoTracking()
                .Select(cs => new { cs.AdministrativeClassId, cs.Code })
                .ToListAsync();

            var list = classes.Select(c => new AdministrativeClassDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                MajorId = c.MajorId,
                MajorCode = c.Major != null ? c.Major.Code : "",
                Cohort = c.Cohort,
                AcademicYear = c.AcademicYear,
                SpecializationId = c.SpecializationId,
                SpecializationCode = c.Specialization != null ? c.Specialization.Code : null,
                CurrentSemester = c.CurrentSemester,
                StudentCount = c.Students.Count(s => s.User != null && s.User.IsActive),
                CourseSectionsCount = courseSections.Count(cs => cs.AdministrativeClassId == c.Id || cs.Code.EndsWith("_" + c.Code)),
                IsActive = c.IsActive
            }).ToList();

            return Ok(list.AsQueryable());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            var c = await _db.AdministrativeClasses
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .Include(c => c.Students)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound();

            var students = c.Students.Where(s => s.User != null && s.User.IsActive).Select(s => new
            {
                s.Id,
                s.StudentCode,
                FullName = s.User?.FullName ?? "",
                Email = s.User?.Email ?? "",
                s.CurrentSemester,
                s.Status
            }).ToList();

            return Ok(new
            {
                c.Id,
                c.Code,
                c.Name,
                c.MajorId,
                MajorCode = c.Major?.Code,
                c.SpecializationId,
                SpecializationCode = c.Specialization?.Code,
                c.Cohort,
                c.AcademicYear,
                c.CurrentSemester,
                c.IsActive,
                Students = students
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] AdministrativeClass model, [FromServices] ICurriculumService curriculumService)
        {
            if (await _db.AdministrativeClasses.AnyAsync(c => c.Code == model.Code))
                return BadRequest(new { message = $"Mã lớp '{model.Code}' đã tồn tại." });

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                var major = await _db.Majors.FindAsync(model.MajorId);
                model.Name = major != null ? $"{major.Name} - {model.Code}" : $"Lớp {model.Code}";
            }

            if (model.Cohort <= 0)
            {
                var match = System.Text.RegularExpressions.Regex.Match(model.Code ?? "", @"\d{2}");
                model.Cohort = (match.Success && int.TryParse(match.Value, out int c)) ? c : 18;
            }

            if (string.IsNullOrWhiteSpace(model.AcademicYear))
            {
                model.AcademicYear = "Summer 26";
            }

            if (model.CurrentSemester <= 0)
            {
                model.CurrentSemester = 1;
            }

            model.SpecializationId = (model.SpecializationId.HasValue && model.SpecializationId.Value > 0) ? model.SpecializationId : null;

            _db.AdministrativeClasses.Add(model);
            await _db.SaveChangesAsync();

            // Tự động lấy Cấu hình Chương trình Đào tạo theo (Ngành, Ngành hẹp nếu có, Kì học) để tạo các môn học trong lớp
            var createdSections = await AutoCreateCourseSectionsForClassAsync(model, curriculumService);

            var subjectsListStr = createdSections.Any() 
                ? string.Join(", ", createdSections.Select(s => s.SubjectCode)) 
                : "Chưa cấu hình môn học";

            var successMessage = createdSections.Any()
                ? $"Tạo thành công lớp {model.Code}! Đã tự động tạo {createdSections.Count} môn học theo CTĐT (Kỳ {model.CurrentSemester}): {subjectsListStr}."
                : $"Tạo thành công lớp {model.Code}.";

            await _auditLog.LogAsync("ADMIN_CLASS_CREATED", "AdministrativeClass", model.Id.ToString(),
                User.Identity?.Name ?? "Admin",
                $"Tạo lớp hành chính {model.Code} và tự động khởi tạo {createdSections.Count} môn học theo CTĐT");

            return Ok(new
            {
                model.Id,
                model.Code,
                model.Name,
                model.MajorId,
                model.Cohort,
                model.AcademicYear,
                model.CurrentSemester,
                model.SpecializationId,
                model.IsActive,
                CreatedSectionsCount = createdSections.Count,
                Sections = createdSections,
                Message = successMessage
            });
        }

        [HttpGet("{id}/sections")]
        public async Task<IActionResult> GetClassSections(int id)
        {
            var c = await _db.AdministrativeClasses
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound(new { message = "Không tìm thấy lớp hành chính." });

            var sections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Semester)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.Schedules)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.AdministrativeClassId == id || cs.Code.EndsWith("_" + c.Code) || cs.Name.EndsWith("- " + c.Code))
                .OrderBy(cs => cs.Subject != null ? cs.Subject.Code : cs.Code)
                .Select(cs => new
                {
                    cs.Id,
                    cs.Code,
                    cs.Name,
                    SubjectId = cs.SubjectId,
                    SubjectCode = cs.Subject != null ? cs.Subject.Code : "",
                    SubjectName = cs.Subject != null ? cs.Subject.Name : "",
                    Credits = cs.Subject != null ? cs.Subject.Credits : 3,
                    SemesterId = cs.SemesterId,
                    SemesterCode = cs.Semester != null ? cs.Semester.Code : "",
                    SemesterName = cs.Semester != null ? cs.Semester.Name : "",
                    LecturerName = cs.Lecturer != null && cs.Lecturer.User != null ? cs.Lecturer.User.FullName : "Chưa phân công",
                    cs.MaxCapacity,
                    EnrolledCount = cs.Enrollments.Count,
                    cs.Status,
                    Schedules = cs.Schedules.Select(s => new
                    {
                        s.DayOfWeek,
                        DayOfWeekStr = s.DayOfWeek.ToString(),
                        s.Slot,
                        s.Room
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                ClassId = c.Id,
                ClassCode = c.Code,
                ClassName = c.Name,
                MajorCode = c.Major?.Code ?? "",
                SpecializationCode = c.Specialization?.Code,
                CurrentSemester = c.CurrentSemester,
                AcademicYear = c.AcademicYear,
                SectionsCount = sections.Count,
                Sections = sections
            });
        }

        [HttpPost("{id}/sync-curriculum")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncClassCurriculum(int id, [FromServices] ICurriculumService curriculumService)
        {
            var c = await _db.AdministrativeClasses.FindAsync(id);
            if (c == null) return NotFound(new { message = "Không tìm thấy lớp hành chính." });

            var createdSections = await AutoCreateCourseSectionsForClassAsync(c, curriculumService);

            var subjectsListStr = createdSections.Any() 
                ? string.Join(", ", createdSections.Select(s => s.SubjectCode)) 
                : "Không tìm thấy môn học phù hợp";

            return Ok(new
            {
                message = $"Đã đồng bộ thành công {createdSections.Count} môn học theo CTĐT cho lớp {c.Code} (Kỳ {c.CurrentSemester}): {subjectsListStr}.",
                sectionsCount = createdSections.Count,
                sections = createdSections
            });
        }

        private async Task<List<CourseSectionDto>> AutoCreateCourseSectionsForClassAsync(AdministrativeClass adminClass, ICurriculumService curriculumService)
        {
            var createdSections = new List<CourseSectionDto>();

            // 1. Tìm học kỳ tương ứng từ AcademicYear (vd: "Summer 26", "SU26", ...)
            var academicYearStr = (adminClass.AcademicYear ?? "").Trim();
            var semester = await _db.Semesters.FirstOrDefaultAsync(s => 
                s.Code == academicYearStr || 
                s.Name == academicYearStr ||
                academicYearStr.Contains(s.Code) ||
                s.Name.Contains(academicYearStr));

            if (semester == null)
            {
                semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Status == "Active")
                           ?? await _db.Semesters.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();
            }

            if (semester == null)
            {
                semester = new Semester
                {
                    Code = "SU26",
                    Name = "Summer 2026",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(4),
                    Status = "Active"
                };
                _db.Semesters.Add(semester);
                await _db.SaveChangesAsync();
            }

            // 2. Tra cứu cấu hình chương trình đào tạo theo Ngành, Ngành hẹp (nếu có), Kì học
            var lookup = await curriculumService.LookupCurriculumConfigAsync(new CurriculumLookupRequest
            {
                MajorId = adminClass.MajorId,
                SemesterNumber = adminClass.CurrentSemester,
                SpecializationId = adminClass.SpecializationId
            });

            var subjectsToCreate = new List<CurriculumSubjectDetailDto>();

            if (lookup.IsFound && lookup.Subjects.Any())
            {
                subjectsToCreate = lookup.Subjects;
            }
            else if (adminClass.SpecializationId.HasValue)
            {
                // Thử tìm cấu hình cơ sở ngành nếu ngành hẹp chưa có cấu hình riêng
                var fallbackLookup = await curriculumService.LookupCurriculumConfigAsync(new CurriculumLookupRequest
                {
                    MajorId = adminClass.MajorId,
                    SemesterNumber = adminClass.CurrentSemester,
                    SpecializationId = null
                });
                if (fallbackLookup.IsFound && fallbackLookup.Subjects.Any())
                {
                    subjectsToCreate = fallbackLookup.Subjects;
                }
            }

            // 3. Khởi tạo các lớp học phần tương ứng cho lớp này
            int slotOffset = 1;
            foreach (var sub in subjectsToCreate)
            {
                var sectionCode = $"{sub.SubjectCode}_{semester.Code}_{adminClass.Code}";

                var existing = await _db.CourseSections
                    .Include(cs => cs.Subject)
                    .Include(cs => cs.Semester)
                    .FirstOrDefaultAsync(cs => cs.Code == sectionCode || 
                        (cs.AdministrativeClassId == adminClass.Id && cs.SubjectId == sub.SubjectId && cs.SemesterId == semester.Id));

                if (existing != null)
                {
                    if (!existing.AdministrativeClassId.HasValue)
                    {
                        existing.AdministrativeClassId = adminClass.Id;
                        await _db.SaveChangesAsync();
                    }
                    createdSections.Add(new CourseSectionDto
                    {
                        Id = existing.Id,
                        Code = existing.Code,
                        Name = existing.Name,
                        SubjectId = existing.SubjectId,
                        SubjectCode = sub.SubjectCode,
                        SubjectName = sub.SubjectName,
                        SemesterId = existing.SemesterId,
                        SemesterCode = semester.Code
                    });
                    continue;
                }

                var section = new CourseSection
                {
                    Code = sectionCode,
                    Name = $"{sub.SubjectName} - {adminClass.Code}",
                    SubjectId = sub.SubjectId,
                    SemesterId = semester.Id,
                    AdministrativeClassId = adminClass.Id,
                    MaxCapacity = 30,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                // Gán lịch học giả lập không trùng để sẵn sàng xếp lớp và quản lý (2 buổi/tuần)
                var day1 = (DayOfWeek)(1 + ((slotOffset * 2) % 5)); // Mon, Wed, Fri
                var day2 = (DayOfWeek)(2 + ((slotOffset * 2) % 4)); // Tue, Thu
                var slot = (slotOffset % 4) + 1; // Slot 1 - 4
                slotOffset++;

                section.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day1, Slot = slot, Room = $"BE-{300 + slot}" });
                section.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day2, Slot = slot, Room = $"BE-{300 + slot}" });

                _db.CourseSections.Add(section);
                await _db.SaveChangesAsync();

                createdSections.Add(new CourseSectionDto
                {
                    Id = section.Id,
                    Code = section.Code,
                    Name = section.Name,
                    SubjectId = section.SubjectId,
                    SubjectCode = sub.SubjectCode,
                    SubjectName = sub.SubjectName,
                    SemesterId = section.SemesterId,
                    SemesterCode = semester.Code
                });
            }

            // 4. Nếu lớp hành chính này đã có sinh viên, tự động đăng ký các sinh viên vào các lớp học phần này
            var studentIds = await _db.Students.Where(s => s.AdministrativeClassId == adminClass.Id).Select(s => s.Id).ToListAsync();
            if (studentIds.Any())
            {
                foreach (var secDto in createdSections)
                {
                    foreach (var stId in studentIds)
                    {
                        if (!await _db.CourseSectionStudents.AnyAsync(css => css.CourseSectionId == secDto.Id && css.StudentId == stId))
                        {
                            _db.CourseSectionStudents.Add(new CourseSectionStudent
                            {
                                CourseSectionId = secDto.Id,
                                StudentId = stId,
                                EnrolledAt = DateTime.UtcNow,
                                Status = "Enrolled"
                            });
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }

            return createdSections;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] AdministrativeClass model)
        {
            var c = await _db.AdministrativeClasses.FindAsync(id);
            if (c == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.Name)) c.Name = model.Name;
            if (model.MajorId > 0) c.MajorId = model.MajorId;
            if (model.Cohort > 0) c.Cohort = model.Cohort;
            if (!string.IsNullOrWhiteSpace(model.AcademicYear)) c.AcademicYear = model.AcademicYear;
            if (model.CurrentSemester > 0) c.CurrentSemester = model.CurrentSemester;
            c.SpecializationId = model.SpecializationId;
            c.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return Ok(c);
        }

        [HttpGet("{id}/curriculum")]
        public async Task<IActionResult> GetClassCurriculum(int id, [FromServices] ICurriculumService curriculumService)
        {
            var c = await _db.AdministrativeClasses.FindAsync(id);
            if (c == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy lớp hành chính."));

            var lookup = await curriculumService.LookupCurriculumConfigAsync(new CurriculumLookupRequest
            {
                MajorId = c.MajorId,
                SemesterNumber = c.CurrentSemester,
                SpecializationId = c.SpecializationId
            });

            return Ok(ApiResponse<CurriculumLookupResponse>.Ok(lookup));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var c = await _db.AdministrativeClasses.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return NotFound(new { message = "Không tìm thấy lớp hành chính." });

            if (c.Students.Any())
            {
                c.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Lớp đang có sinh viên theo học, đã chuyển sang trạng thái Inactive." });
            }

            _db.AdministrativeClasses.Remove(c);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa lớp hành chính thành công." });
        }

        [HttpGet("{id}/lecturers-assignment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLecturersAssignment(int id)
        {
            var c = await _db.AdministrativeClasses
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound(new { message = "Không tìm thấy lớp hành chính." });

            var majorCode = c.Major?.Code ?? "";

            var sections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.Schedules)
                .Where(cs => cs.AdministrativeClassId == id || cs.Code.EndsWith("_" + c.Code) || cs.Name.EndsWith("- " + c.Code))
                .OrderBy(cs => cs.Subject != null ? cs.Subject.Code : cs.Code)
                .ToListAsync();

            var lecturers = await _db.Lecturers
                .Include(l => l.User)
                .Where(l => l.User != null && l.User.IsActive)
                .OrderBy(l => l.LecturerCode)
                .ToListAsync();

            var lecturerOptions = lecturers.Select(l => new LecturerOptionDto
            {
                Id = l.Id,
                LecturerCode = l.LecturerCode,
                FullName = l.User?.FullName ?? "",
                Department = l.Department,
                AcademicTitle = l.AcademicTitle,
                IsPreferred = IsMatchingDepartment(majorCode, l.Department)
            })
            .OrderByDescending(l => l.IsPreferred)
            .ThenBy(l => l.FullName)
            .ToList();

            var sectionItems = sections.Select(cs => new SectionLecturerItemDto
            {
                Id = cs.Id,
                Code = cs.Code,
                Name = cs.Name,
                SubjectId = cs.SubjectId,
                SubjectCode = cs.Subject?.Code ?? "",
                SubjectName = cs.Subject?.Name ?? "",
                Credits = cs.Subject?.Credits ?? 3,
                LecturerId = cs.LecturerId,
                LecturerCode = cs.Lecturer?.LecturerCode ?? "",
                LecturerName = cs.Lecturer?.User?.FullName ?? "Chưa phân công",
                LecturerDepartment = cs.Lecturer?.Department ?? "",
                Schedules = cs.Schedules.Select(s => new CourseSectionScheduleDto
                {
                    Id = s.Id,
                    CourseSectionId = s.CourseSectionId,
                    DayOfWeek = s.DayOfWeek,
                    Slot = s.Slot,
                    Room = s.Room
                }).ToList()
            }).ToList();

            var result = new ClassLecturersAssignmentDto
            {
                ClassId = c.Id,
                ClassCode = c.Code,
                ClassName = c.Name,
                MajorId = c.MajorId,
                MajorCode = majorCode,
                SpecializationCode = c.Specialization?.Code,
                CurrentSemester = c.CurrentSemester,
                AcademicYear = c.AcademicYear,
                Sections = sectionItems,
                Lecturers = lecturerOptions
            };

            return Ok(result);
        }

        [HttpPost("{id}/assign-lecturers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignClassLecturers(int id, [FromBody] AssignClassLecturersRequest request)
        {
            var c = await _db.AdministrativeClasses.FindAsync(id);
            if (c == null) return NotFound(new { message = "Không tìm thấy lớp hành chính." });

            var sectionIds = request.Assignments.Select(a => a.CourseSectionId).ToList();
            var sections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Where(cs => sectionIds.Contains(cs.Id))
                .ToListAsync();

            var updatedCount = 0;

            foreach (var a in request.Assignments)
            {
                var sec = sections.FirstOrDefault(s => s.Id == a.CourseSectionId);
                if (sec != null)
                {
                    sec.LecturerId = a.LecturerId;
                    updatedCount++;
                }
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CLASS_LECTURERS_ASSIGNED", "AdministrativeClass", id.ToString(),
                User.Identity?.Name ?? "Admin",
                $"Phân bổ giáo viên cho {updatedCount} môn học của lớp hành chính {c.Code}");

            return Ok(new
            {
                message = $"Đã cập nhật phân bổ giáo viên cho {updatedCount} môn học của lớp {c.Code} thành công.",
                updatedCount
            });
        }

        [HttpPost("auto-assign-lecturers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AutoAssignLecturers([FromBody] AutoAssignLecturersRequest request)
        {
            var classesQuery = _db.AdministrativeClasses
                .Include(ac => ac.Major)
                .Include(ac => ac.Specialization)
                .Where(ac => ac.IsActive);

            if (request.ClassId.HasValue && request.ClassId.Value > 0)
            {
                classesQuery = classesQuery.Where(ac => ac.Id == request.ClassId.Value);
            }

            var adminClasses = await classesQuery.OrderBy(ac => ac.Code).ToListAsync();
            if (!adminClasses.Any())
            {
                return BadRequest(new { message = "Không tìm thấy lớp hành chính nào phù hợp." });
            }

            // Lấy danh sách giảng viên hiện có
            var allLecturers = await _db.Lecturers
                .Include(l => l.User)
                .Where(l => l.User != null && l.User.IsActive)
                .ToListAsync();

            if (!allLecturers.Any())
            {
                return BadRequest(new { message = "Chưa có giảng viên nào trong hệ thống." });
            }

            var adminClassIds = adminClasses.Select(c => c.Id).ToList();
            var allSections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Where(cs => cs.AdministrativeClassId.HasValue && adminClassIds.Contains(cs.AdministrativeClassId.Value))
                .ToListAsync();

            // Đếm số lớp hiện tại của từng giảng viên để phân bổ đều
            var lecturerLoad = allLecturers.ToDictionary(l => l.Id, l => 0);
            foreach (var s in allSections.Where(s => s.LecturerId.HasValue))
            {
                if (lecturerLoad.ContainsKey(s.LecturerId!.Value))
                {
                    lecturerLoad[s.LecturerId!.Value]++;
                }
            }

            int assignedCount = 0;
            var assignmentDetails = new List<object>();

            foreach (var adminClass in adminClasses)
            {
                var classSections = allSections
                    .Where(cs => cs.AdministrativeClassId == adminClass.Id)
                    .ToList();

                var majorCode = adminClass.Major?.Code ?? "";

                // Giảng viên phù hợp với chuyên môn của ngành
                var preferredLecturers = allLecturers
                    .Where(l => IsMatchingDepartment(majorCode, l.Department))
                    .ToList();

                var candidatePool = preferredLecturers.Any() ? preferredLecturers : allLecturers;

                foreach (var sec in classSections)
                {
                    // Chọn giảng viên có tải ít nhất trong nhóm phù hợp
                    var bestLecturer = candidatePool
                        .OrderBy(l => lecturerLoad.TryGetValue(l.Id, out var load) ? load : 0)
                        .ThenBy(l => l.Id)
                        .First();

                    sec.LecturerId = bestLecturer.Id;
                    lecturerLoad[bestLecturer.Id] = (lecturerLoad.TryGetValue(bestLecturer.Id, out var currentLoad) ? currentLoad : 0) + 1;
                    assignedCount++;

                    assignmentDetails.Add(new
                    {
                        classCode = adminClass.Code,
                        sectionCode = sec.Code,
                        subjectCode = sec.Subject?.Code ?? "",
                        lecturerCode = bestLecturer.LecturerCode,
                        lecturerName = bestLecturer.User?.FullName ?? "",
                        department = bestLecturer.Department
                    });
                }
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("AUTO_ASSIGN_LECTURERS", "AdministrativeClasses", "Batch",
                User.Identity?.Name ?? "Admin",
                $"Tự động phân bổ giảng viên cho {assignedCount} môn học phần thuộc {adminClasses.Count} lớp hành chính.");

            return Ok(new
            {
                message = $"Tự động phân bổ thành công {assignedCount} môn học phần cho {adminClasses.Count} lớp hành chính.",
                assignedCount,
                classesCount = adminClasses.Count,
                assignments = assignmentDetails
            });
        }

        private static bool IsMatchingDepartment(string majorCode, string department)
        {
            var m = (majorCode ?? "").ToUpper();
            var d = (department ?? "").ToLower();

            return m switch
            {
                "SE" => d.Contains("software") || d.Contains("phần mềm") || d.Contains("lập trình") || d.Contains("se"),
                "IA" => d.Contains("an toàn") || d.Contains("an ninh") || d.Contains("security") || d.Contains("cyber") || d.Contains("mạng"),
                "AI" => d.Contains("trí tuệ nhân tạo") || d.Contains("ai") || d.Contains("dữ liệu") || d.Contains("data"),
                "DS" => d.Contains("dữ liệu") || d.Contains("data") || d.Contains("trí tuệ nhân tạo") || d.Contains("thống kê"),
                "IT" => d.Contains("hệ thống") || d.Contains("mạng") || d.Contains("thông tin") || d.Contains("it") || d.Contains("network"),
                "GD" => d.Contains("mỹ thuật") || d.Contains("thiết kế") || d.Contains("đồ họa") || d.Contains("graphic") || d.Contains("multimedia"),
                "BA" => d.Contains("quản trị") || d.Contains("kinh doanh") || d.Contains("marketing") || d.Contains("business") || d.Contains("tài chính"),
                "MC" => d.Contains("truyền thông") || d.Contains("báo chí") || d.Contains("media") || d.Contains("communication") || d.Contains("pr"),
                "EN" => d.Contains("tiếng anh") || d.Contains("anh") || d.Contains("english"),
                "JP" => d.Contains("tiếng nhật") || d.Contains("nhật") || d.Contains("japanese"),
                "KR" => d.Contains("tiếng hàn") || d.Contains("hàn") || d.Contains("korean"),
                _ => true
            };
        }
    }
}

