using System.Security.Claims;
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
    [Route("api/course-sections")]
    [Authorize]
    public class CourseSectionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ICurriculumService _curriculumService;
        private readonly IAuditLogService _auditLog;

        public CourseSectionsController(AppDbContext db, ICurriculumService curriculumService, IAuditLogService auditLog)
        {
            _db = db;
            _curriculumService = curriculumService;
            _auditLog = auditLog;
        }

        [HttpGet]
        [EnableQuery(PageSize = 100)]
        public async Task<IActionResult> GetSections([FromQuery] int? semesterId, [FromQuery] int? subjectId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var query = _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Semester)
                .Include(cs => cs.Lecturer)
                    .ThenInclude(l => l!.User)
                .AsNoTracking()
                .AsQueryable();

            if (semesterId.HasValue) query = query.Where(cs => cs.SemesterId == semesterId.Value);
            if (subjectId.HasValue) query = query.Where(cs => cs.SubjectId == subjectId.Value);

            if (userRole == "Lecturer")
            {
                var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
                if (lecturer == null) return Ok(new List<CourseSectionDto>().AsQueryable());
                query = query.Where(cs => cs.LecturerId == lecturer.Id);
            }
            else if (userRole == "Student")
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return Ok(new List<CourseSectionDto>().AsQueryable());
                var enrolledIds = await _db.CourseSectionStudents
                    .Where(e => e.StudentId == student.Id)
                    .Select(e => e.CourseSectionId)
                    .ToListAsync();
                query = query.Where(cs => enrolledIds.Contains(cs.Id));
            }

            var sections = await query.ToListAsync();
            var sectionIds = sections.Select(cs => cs.Id).ToList();

            // Đếm số lượng sinh viên bằng GroupBy cực nhanh
            var enrollCounts = await _db.CourseSectionStudents
                .Where(e => sectionIds.Contains(e.CourseSectionId) && e.Status == "Enrolled")
                .GroupBy(e => e.CourseSectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SectionId, x => x.Count);

            // Lấy lịch học theo lookup
            var schedules = await _db.CourseSectionSchedules
                .Where(s => sectionIds.Contains(s.CourseSectionId))
                .AsNoTracking()
                .ToListAsync();
            var scheduleLookup = schedules.ToLookup(s => s.CourseSectionId);

            var list = sections.Select(cs => new CourseSectionDto
            {
                Id = cs.Id,
                Code = cs.Code,
                Name = cs.Name,
                SubjectId = cs.SubjectId,
                SubjectCode = cs.Subject?.Code ?? "",
                SubjectName = cs.Subject?.Name ?? "",
                Credits = cs.Subject?.Credits ?? 0,
                SemesterId = cs.SemesterId,
                SemesterCode = cs.Semester?.Code ?? "",
                LecturerId = cs.LecturerId,
                LecturerCode = cs.Lecturer?.LecturerCode,
                LecturerName = cs.Lecturer?.User?.FullName ?? "Chưa phân công",
                MaxCapacity = cs.MaxCapacity,
                EnrolledCount = enrollCounts.TryGetValue(cs.Id, out var count) ? count : 0,
                IsGradeLocked = cs.IsGradeLocked,
                GradeLockedAt = cs.GradeLockedAt,
                GradeLockedBy = cs.GradeLockedBy,
                Status = cs.Status,
                Schedules = scheduleLookup[cs.Id].Select(sc => new CourseSectionScheduleDto
                {
                    Id = sc.Id,
                    CourseSectionId = sc.CourseSectionId,
                    DayOfWeek = sc.DayOfWeek,
                    Slot = sc.Slot,
                    Room = sc.Room
                }).ToList()
            }).ToList();

            return Ok(list.AsQueryable());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSectionById(int id)
        {
            var cs = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Semester)
                .Include(cs => cs.Lecturer)
                    .ThenInclude(l => l!.User)
                .Include(cs => cs.Schedules)
                .Include(cs => cs.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s!.User)
                .Include(cs => cs.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s!.AdministrativeClass)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (cs == null) return NotFound();

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            if (userRole == "Lecturer")
            {
                var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
                if (lecturer == null || cs.LecturerId != lecturer.Id)
                {
                    return Forbid();
                }
            }

            var enrolledStudents = cs.Enrollments.Select(e => new
            {
                StudentId = e.StudentId,
                StudentCode = e.Student?.StudentCode ?? "",
                FullName = e.Student?.User?.FullName ?? "",
                AdministrativeClass = e.Student?.AdministrativeClass?.Code ?? "Chưa có",
                EnrolledAt = e.EnrolledAt
            }).ToList();

            return Ok(new
            {
                cs.Id,
                cs.Code,
                cs.Name,
                cs.SubjectId,
                SubjectCode = cs.Subject?.Code,
                SubjectName = cs.Subject?.Name,
                cs.SemesterId,
                SemesterCode = cs.Semester?.Code,
                cs.LecturerId,
                LecturerCode = cs.Lecturer?.LecturerCode,
                LecturerName = cs.Lecturer?.User?.FullName ?? "Chưa phân công",
                cs.MaxCapacity,
                EnrolledCount = cs.Enrollments.Count,
                cs.IsGradeLocked,
                cs.Status,
                Schedules = cs.Schedules.Select(sc => new { sc.Id, sc.DayOfWeek, sc.Slot, sc.Room }),
                Students = enrolledStudents
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSection([FromBody] CreateCourseSectionRequest request)
        {
            if (await _db.CourseSections.AnyAsync(c => c.Code == request.Code))
                return BadRequest(new { message = $"Mã lớp học phần '{request.Code}' đã tồn tại." });

            var section = new CourseSection
            {
                Code = request.Code,
                Name = request.Name,
                SubjectId = request.SubjectId,
                SemesterId = request.SemesterId,
                LecturerId = request.LecturerId,
                MaxCapacity = request.MaxCapacity > 0 ? request.MaxCapacity : 30,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            foreach (var sc in request.Schedules)
            {
                section.Schedules.Add(new CourseSectionSchedule
                {
                    DayOfWeek = sc.DayOfWeek,
                    Slot = sc.Slot,
                    Room = string.IsNullOrWhiteSpace(sc.Room) ? "BE-301" : sc.Room
                });
            }

            _db.CourseSections.Add(section);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CREATE_COURSE_SECTION", "CourseSection", section.Id.ToString(),
                User.Identity?.Name ?? "Admin", $"Tạo lớp học phần {section.Code} (Sức chứa: {section.MaxCapacity})");

            return Ok(section);
        }

        // Tạo hàng loạt lớp học phần tự động theo cấu hình chương trình đào tạo
        [HttpPost("batch-create-from-curriculum")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BatchCreateFromCurriculum([FromBody] BatchCreateSectionsFromCurriculumRequest request)
        {
            var adminClass = await _db.AdministrativeClasses
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .FirstOrDefaultAsync(c => c.Id == request.AdministrativeClassId);

            if (adminClass == null) return NotFound(new { message = "Lớp hành chính không tồn tại." });

            var semester = await _db.Semesters.FindAsync(request.SemesterId);
            if (semester == null) return NotFound(new { message = "Học kỳ không tồn tại." });

            // Tra cứu cấu hình chương trình
            var lookup = await _curriculumService.LookupCurriculumConfigAsync(new CurriculumLookupRequest
            {
                MajorId = adminClass.MajorId,
                SemesterNumber = adminClass.CurrentSemester,
                SpecializationId = adminClass.SpecializationId,
                Version = request.Version
            });

            if (!lookup.IsFound)
            {
                return BadRequest(new { message = lookup.Message });
            }

            var createdSections = new List<string>();

            int slotOffset = 1;
            foreach (var sub in lookup.Subjects)
            {
                var code = $"{sub.SubjectCode}_{semester.Code}_{adminClass.Code}";
                if (await _db.CourseSections.AnyAsync(cs => cs.Code == code))
                {
                    continue; // Đã tạo rồi
                }

                var section = new CourseSection
                {
                    Code = code,
                    Name = $"{sub.SubjectName} - {adminClass.Code}",
                    SubjectId = sub.SubjectId,
                    SemesterId = semester.Id,
                    MaxCapacity = request.DefaultMaxCapacity > 0 ? request.DefaultMaxCapacity : 30,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                // Gán lịch học giả lập không trùng để sẵn sàng xếp lớp
                var day1 = (DayOfWeek)(1 + ((slotOffset * 2) % 5)); // Mon, Wed, Fri
                var day2 = (DayOfWeek)(2 + ((slotOffset * 2) % 4)); // Tue, Thu
                var slot = (slotOffset % 4) + 1; // Slot 1 - 4
                slotOffset++;

                section.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day1, Slot = slot, Room = $"BE-{300 + slot}" });
                section.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day2, Slot = slot, Room = $"BE-{300 + slot}" });

                _db.CourseSections.Add(section);
                createdSections.Add(code);
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("BATCH_SECTIONS_CREATED", "AdministrativeClass", adminClass.Id.ToString(),
                User.Identity?.Name ?? "Admin",
                $"Tạo tự động {createdSections.Count} lớp học phần theo chương trình đào tạo cho lớp {adminClass.Code} trong kỳ {semester.Code}");

            return Ok(new
            {
                message = $"Đã tạo thành công {createdSections.Count} lớp học phần.",
                createdSections
            });
        }

        // Phân công / Thay đổi giảng viên: Giảng viên mới được cấp quyền ngay, giảng viên cũ mất quyền ngay, điểm giữ nguyên
        [HttpPut("{id}/lecturer")]
        [HttpPatch("{id}/lecturer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignLecturer(int id, [FromBody] AssignLecturerRequest request)
        {
            var section = await _db.CourseSections
                .Include(cs => cs.Lecturer)
                    .ThenInclude(l => l!.User)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (section == null) return NotFound(new { message = "Lớp học phần không tồn tại." });

            var oldLecturerName = section.Lecturer?.User?.FullName ?? "Chưa phân công";

            string newLecturerName = "Chưa phân công";
            if (request.LecturerId.HasValue)
            {
                var newLecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == request.LecturerId.Value);
                if (newLecturer == null) return BadRequest(new { message = "Giảng viên mới không tồn tại." });
                newLecturerName = newLecturer.User?.FullName ?? newLecturer.LecturerCode;
            }

            section.LecturerId = request.LecturerId;
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("LECTURER_CHANGED", "CourseSection", section.Id.ToString(),
                User.Identity?.Name ?? "Admin",
                $"Đổi giảng viên phụ trách lớp {section.Code} từ '{oldLecturerName}' sang '{newLecturerName}'. Điểm số đã nhập được giữ nguyên.");

            return Ok(new
            {
                message = $"Đã phân công giảng viên '{newLecturerName}' cho lớp {section.Code} thành công.",
                sectionId = section.Id,
                lecturerId = section.LecturerId,
                lecturerName = newLecturerName
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourseSection(int id, [FromBody] UpdateCourseSectionRequest request)
        {
            var sec = await _db.CourseSections.FindAsync(id);
            if (sec == null) return NotFound(new { message = "Không tìm thấy lớp học phần." });

            if (sec.IsGradeLocked)
                return BadRequest(new { message = "Lớp học phần đã khóa sổ điểm, không thể chỉnh sửa." });

            if (!string.IsNullOrWhiteSpace(request.Name))
                sec.Name = request.Name;

            sec.MaxCapacity = request.MaxCapacity > 0 ? request.MaxCapacity : 30;
            if (request.LecturerId.HasValue)
            {
                sec.LecturerId = request.LecturerId.Value;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật lớp học phần thành công." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourseSection(int id)
        {
            var sec = await _db.CourseSections
                .Include(cs => cs.StudentGrades)
                .Include(cs => cs.Schedules)
                .FirstOrDefaultAsync(cs => cs.Id == id);
            if (sec == null) return NotFound(new { message = "Không tìm thấy lớp học phần." });

            if (sec.IsGradeLocked)
                return BadRequest(new { message = "Lớp học phần đã khóa điểm, không thể xóa." });

            var hasScores = sec.StudentGrades.Any(g => g.Score.HasValue);
            if (hasScores)
            {
                sec.Status = "Cancelled";
                await _db.SaveChangesAsync();
                return Ok(new { message = "Lớp học phần đã có điểm sinh viên, đã chuyển trạng thái thành Đã hủy (Cancelled)." });
            }

            _db.StudentGrades.RemoveRange(sec.StudentGrades);
            _db.CourseSectionSchedules.RemoveRange(sec.Schedules);
            _db.CourseSections.Remove(sec);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa lớp học phần thành công." });
        }

        [HttpPost("allocation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunAllocation([FromBody] RunAllocationRequest request, [FromServices] IStudentAllocationService allocationService)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var result = await allocationService.RunAllocationAsync(request, performedBy);
                return Ok(ApiResponse<AllocationResultDto>.Ok(result, "Phân bổ sinh viên hoàn tất."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
        }
    }

    public class UpdateCourseSectionRequest
    {
        public string? Name { get; set; }
        public int? LecturerId { get; set; }
        public int MaxCapacity { get; set; } = 30;
    }
}

