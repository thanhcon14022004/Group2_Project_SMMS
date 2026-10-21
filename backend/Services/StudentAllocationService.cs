using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IStudentAllocationService
    {
        Task<AllocationResultDto> RunAllocationAsync(RunAllocationRequest request, string performedBy);
        Task<AllocationResultDto> GetAllocationStatusAsync(int semesterId);
        Task<List<UnallocatedExceptionDto>> GetUnallocatedExceptionsAsync(int semesterId);
        Task<bool> EnrollSingleStudentAsync(int courseSectionId, int studentId, string performedBy);
        Task<bool> RemoveStudentFromSectionAsync(int courseSectionId, int studentId, string performedBy);
    }

    public class StudentAllocationService : IStudentAllocationService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;
        private readonly IGradeService _gradeService;
        private readonly ICurriculumService _curriculumService;

        public StudentAllocationService(
            AppDbContext db, 
            IAuditLogService auditLog, 
            IGradeService gradeService,
            ICurriculumService curriculumService)
        {
            _db = db;
            _auditLog = auditLog;
            _gradeService = gradeService;
            _curriculumService = curriculumService;
        }

        public async Task<AllocationResultDto> RunAllocationAsync(RunAllocationRequest request, string performedBy)
        {
            // 1. Xác định học kỳ cần phân bổ (ưu tiên học kỳ Active)
            Semester? semester = null;
            if (request.SemesterId > 0)
            {
                semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Id == request.SemesterId);
            }
            if (semester == null)
            {
                semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Status == "Active");
            }
            if (semester == null)
            {
                throw new InvalidOperationException("Không tìm thấy học kỳ hợp lệ hoặc học kỳ đang ở trạng thái 'Active' để phân bổ sinh viên.");
            }

            // 2. Lấy danh sách các lớp hành chính cần phân bổ thuộc mùa học này
            var semCode = (semester.Code ?? "").Trim().ToUpper();
            var semName = (semester.Name ?? "").Trim().ToUpper();
            string seasonKeyword = semCode.StartsWith("FA") ? "FALL" : 
                                   semCode.StartsWith("SP") ? "SPRING" : 
                                   semCode.StartsWith("SU") ? "SUMMER" : semCode;

            var classesQuery = _db.AdministrativeClasses
                .Include(ac => ac.Major)
                .Include(ac => ac.Specialization)
                .Where(ac => ac.IsActive)
                .Where(ac => ac.AcademicYear.ToUpper() == semCode || 
                             ac.AcademicYear.ToUpper().Contains(semCode) ||
                             ac.AcademicYear.ToUpper().Contains(seasonKeyword) ||
                             semName.Contains(ac.AcademicYear.ToUpper()));

            if (request.MajorId.HasValue && request.MajorId.Value > 0)
                classesQuery = classesQuery.Where(ac => ac.MajorId == request.MajorId.Value);

            if (request.AdministrativeClassId.HasValue && request.AdministrativeClassId.Value > 0)
                classesQuery = classesQuery.Where(ac => ac.Id == request.AdministrativeClassId.Value);

            var adminClasses = await classesQuery.OrderBy(ac => ac.Code).ToListAsync();
            if (!adminClasses.Any())
            {
                throw new InvalidOperationException($"Không tìm thấy lớp hành chính nào thuộc mùa học '{semester.Code}' để thực thi phân bổ sinh viên.");
            }

            var adminClassIds = adminClasses.Select(c => c.Id).ToHashSet();

            // 3. Đảm bảo tất cả các lớp có đủ CourseSections theo Chương trình Đào tạo CTTĐT (tải và tạo theo batch cực nhanh)
            var existingSections = await _db.CourseSections
                .Include(cs => cs.Schedules)
                .Where(cs => cs.SemesterId == semester.Id)
                .ToListAsync();

            var sectionsByClass = existingSections
                .Where(cs => cs.AdministrativeClassId.HasValue)
                .GroupBy(cs => cs.AdministrativeClassId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var newSectionsToInsert = new List<CourseSection>();

            foreach (var adminClass in adminClasses)
            {
                if (!sectionsByClass.TryGetValue(adminClass.Id, out var classSecs))
                {
                    classSecs = new List<CourseSection>();
                    sectionsByClass[adminClass.Id] = classSecs;
                }

                var lookupReq = new CurriculumLookupRequest
                {
                    MajorId = adminClass.MajorId,
                    SemesterNumber = adminClass.CurrentSemester,
                    SpecializationId = adminClass.SpecializationId,
                    Version = "2024.1"
                };

                var lookupRes = await _curriculumService.LookupCurriculumConfigAsync(lookupReq);
                if (lookupRes.IsFound && lookupRes.Subjects.Any())
                {
                    int slotOffset = 1;
                    foreach (var sub in lookupRes.Subjects)
                    {
                        var existingSec = classSecs.FirstOrDefault(cs => cs.SubjectId == sub.SubjectId);
                        if (existingSec == null)
                        {
                            var sectionCode = $"{sub.SubjectCode}_{semester.Code}_{adminClass.Code}";
                            var sec = new CourseSection
                            {
                                Code = sectionCode,
                                Name = $"{sub.SubjectName} - {adminClass.Code}",
                                SubjectId = sub.SubjectId,
                                SemesterId = semester.Id,
                                AdministrativeClassId = adminClass.Id,
                                MaxCapacity = 31,
                                Status = "Open",
                                CreatedAt = DateTime.UtcNow
                            };

                            var day1 = (DayOfWeek)(1 + ((slotOffset * 2) % 5));
                            var day2 = (DayOfWeek)(2 + ((slotOffset * 2) % 4));
                            var slot = (slotOffset % 4) + 1;
                            slotOffset++;

                            sec.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day1, Slot = slot, Room = $"BE-{300 + slot}" });
                            sec.Schedules.Add(new CourseSectionSchedule { DayOfWeek = day2, Slot = slot, Room = $"BE-{300 + slot}" });

                            newSectionsToInsert.Add(sec);
                            classSecs.Add(sec);
                        }
                        else
                        {
                            existingSec.MaxCapacity = 31;
                        }
                    }
                }
            }

            if (newSectionsToInsert.Any())
            {
                _db.CourseSections.AddRange(newSectionsToInsert);
                await _db.SaveChangesAsync();
            }

            // 4. Nạp danh sách tất cả sinh viên đang học
            var allStudents = await _db.Students
                .Include(s => s.User)
                .Where(s => s.Status == "Studying" && s.User != null && s.User.IsActive)
                .ToListAsync();

            var usedCodes = allStudents.Select(s => s.StudentCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var usedUsernames = (await _db.Users.Select(u => u.Username).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var usedEmails = (await _db.Users.Select(u => u.Email).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Hash mật khẩu chuẩn 'Admin@123'
            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            // Danh sách họ tên tiếng Việt
            var familyNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Đinh", "Đoàn", "Lâm", "Trịnh", "Mai", "Đào" };
            var middleNames = new[] { "Văn", "Thị", "Hoàng", "Minh", "Hữu", "Đức", "Quang", "Tuấn", "Anh", "Ngọc", "Phương", "Thanh", "Quốc", "Đình", "Hải", "Gia", "Khánh", "Bảo", "Xuân" };
            var givenNames = new[] { "Dũng", "Hùng", "Long", "Nam", "Huy", "Tuấn", "Đạt", "Thành", "Thắng", "Linh", "Mai", "Lan", "Trang", "Hằng", "Thảo", "Nga", "Ngọc", "Vân", "Sơn", "Cường", "Khoa", "Phúc", "An", "Bình", "Tùng", "Phong", "Việt", "Quân", "Hiếu", "Nghĩa", "Tâm", "Kiên", "Tú", "Hà", "Yến", "Châu", "Hương" };
            var rng = new Random(12345);

            int totalEnrollmentsCreated = 0;
            int totalAutoCreated = 0;
            var classSummaries = new List<ClassAllocationSummaryDto>();
            var allocatedStudentIds = new HashSet<int>();

            var newUsersToInsert = new List<User>();
            var newStudentsToInsert = new List<Student>();
            var classStudentMap = new Dictionary<int, List<Student>>();

            // Ưu tiên xử lý các lớp có chỉ định Chuyên ngành hẹp trước (đặc biệt kỳ 5, 7, 8)
            var sortedClasses = adminClasses
                .OrderByDescending(ac => ac.SpecializationId.HasValue)
                .ThenBy(ac => ac.Code)
                .ToList();

            // 5. Duyệt phân bổ sinh viên vào từng lớp
            foreach (var adminClass in sortedClasses)
            {
                // A. Sinh viên đã thuộc lớp này từ trước
                var classStudents = allStudents
                    .Where(s => !allocatedStudentIds.Contains(s.Id))
                    .Where(s => s.AdministrativeClassId == adminClass.Id)
                    .ToList();

                // Kiểm tra xem sinh viên này có khớp kỳ và ngành không
                var validExistingStudents = new List<Student>();
                foreach (var st in classStudents)
                {
                    if (st.CurrentSemester == adminClass.CurrentSemester && st.MajorId == adminClass.MajorId)
                    {
                        // Nếu lớp có ngành hẹp, đồng bộ ngành hẹp cho sinh viên
                        if (adminClass.SpecializationId.HasValue && adminClass.SpecializationId.Value > 0)
                        {
                            st.SpecializationId = adminClass.SpecializationId;
                        }
                        validExistingStudents.Add(st);
                        allocatedStudentIds.Add(st.Id);
                    }
                    else
                    {
                        st.AdministrativeClassId = null; // Không khớp, gỡ ra để xếp lại
                    }
                }
                classStudents = validExistingStudents;

                // B. Nếu chưa đủ 30 SV, lấy thêm sinh viên tự do chưa có lớp
                if (classStudents.Count < 30)
                {
                    var freeStudents = allStudents
                        .Where(s => !allocatedStudentIds.Contains(s.Id))
                        .Where(s => s.AdministrativeClassId == null || !adminClassIds.Contains(s.AdministrativeClassId.Value))
                        .Where(s => MatchesCriteria(s, adminClass))
                        .ToList();

                    foreach (var st in freeStudents)
                    {
                        if (classStudents.Count >= 30) break;
                        st.AdministrativeClassId = adminClass.Id;
                        if (adminClass.SpecializationId.HasValue && adminClass.SpecializationId.Value > 0)
                        {
                            st.SpecializationId = adminClass.SpecializationId;
                        }
                        classStudents.Add(st);
                        allocatedStudentIds.Add(st.Id);
                    }
                }

                // C. Kiểm tra sĩ số: Mỗi lớp tối đa 31, tối thiểu 28 sinh viên.
                // Nếu thiếu sinh viên (< 28) -> Tự động insert thêm sinh viên vào database cho đủ chuẩn 30 SV
                int classAutoCreated = 0;
                string prefix = GetMajorStudentPrefix(adminClass.Major?.Code ?? "");

                if (classStudents.Count < 28)
                {
                    int needed = 30 - classStudents.Count;
                    for (int i = 0; i < needed; i++)
                    {
                        var (code, username, email) = GenerateUniqueCredentials(prefix, adminClass.Cohort, usedCodes, usedUsernames, usedEmails);
                        var fullName = $"{familyNames[rng.Next(familyNames.Length)]} {middleNames[rng.Next(middleNames.Length)]} {givenNames[rng.Next(givenNames.Length)]}";
                        var phone = "09" + rng.Next(10000000, 99999999).ToString();

                        var newUser = new User
                        {
                            Username = username,
                            PasswordHash = defaultPasswordHash,
                            FullName = fullName,
                            Email = email,
                            PhoneNumber = phone,
                            Role = "Student",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var newStudent = new Student
                        {
                            User = newUser, // EF Core tự động liên kết User và Student
                            StudentCode = code,
                            MajorId = adminClass.MajorId,
                            SpecializationId = adminClass.SpecializationId,
                            CurrentSemester = adminClass.CurrentSemester,
                            Cohort = adminClass.Cohort,
                            AdministrativeClassId = adminClass.Id,
                            Status = "Studying"
                        };

                        newUsersToInsert.Add(newUser);
                        newStudentsToInsert.Add(newStudent);
                        classStudents.Add(newStudent);
                        classAutoCreated++;
                        totalAutoCreated++;
                    }
                }
                else if (classStudents.Count > 31)
                {
                    // Giới hạn tối đa 31 sinh viên
                    var excess = classStudents.Skip(31).ToList();
                    foreach (var ex in excess)
                    {
                        ex.AdministrativeClassId = null;
                        allocatedStudentIds.Remove(ex.Id);
                    }
                    classStudents = classStudents.Take(31).ToList();
                }

                classStudentMap[adminClass.Id] = classStudents;

                // Ghi nhận tổng kết lớp
                classSummaries.Add(new ClassAllocationSummaryDto
                {
                    ClassId = adminClass.Id,
                    ClassCode = adminClass.Code,
                    MajorCode = adminClass.Major?.Code ?? "",
                    SpecializationCode = adminClass.Specialization?.Code,
                    SemesterNumber = adminClass.CurrentSemester,
                    StudentCount = classStudents.Count,
                    AutoCreatedCount = classAutoCreated,
                    SectionsCount = sectionsByClass.TryGetValue(adminClass.Id, out var secs) ? secs.Count : 0
                });
            }

            // Lưu các User và Student tạo mới một lần theo Batch
            if (newUsersToInsert.Any())
            {
                _db.Users.AddRange(newUsersToInsert);
                _db.Students.AddRange(newStudentsToInsert);
                await _db.SaveChangesAsync();

                foreach (var st in newStudentsToInsert)
                {
                    allocatedStudentIds.Add(st.Id);
                }
            }

            // 6. Thực hiện Đăng ký (Enroll) sinh viên vào tất cả các CourseSections của lớp
            var allSectionIds = sectionsByClass.Values.SelectMany(x => x).Select(s => s.Id).Distinct().ToList();

            var existingEnrollmentPairs = (await _db.CourseSectionStudents
                .Where(css => allSectionIds.Contains(css.CourseSectionId))
                .Select(css => new { css.CourseSectionId, css.StudentId })
                .ToListAsync())
                .Select(x => (x.CourseSectionId, x.StudentId))
                .ToHashSet();

            var allSubjectIds = sectionsByClass.Values.SelectMany(x => x).Select(s => s.SubjectId).Distinct().ToList();
            var allGradeComponents = await _db.SubjectGradeComponents
                .Where(gc => allSubjectIds.Contains(gc.SubjectId))
                .ToListAsync();
            var gradeComponentsBySubject = allGradeComponents
                .GroupBy(gc => gc.SubjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var existingGradeKeys = (await _db.StudentGrades
                .Where(g => allSectionIds.Contains(g.CourseSectionId))
                .Select(g => new { g.CourseSectionId, g.StudentId, g.SubjectGradeComponentId })
                .ToListAsync())
                .Select(x => (x.CourseSectionId, x.StudentId, x.SubjectGradeComponentId))
                .ToHashSet();

            var newEnrollments = new List<CourseSectionStudent>();
            var newGrades = new List<StudentGrade>();
            var allocationsList = new List<AllocationItemDto>();

            foreach (var adminClass in adminClasses)
            {
                if (!classStudentMap.TryGetValue(adminClass.Id, out var classStudents) || !classStudents.Any())
                    continue;

                if (!sectionsByClass.TryGetValue(adminClass.Id, out var classSections) || !classSections.Any())
                    continue;

                foreach (var sec in classSections)
                {
                    foreach (var st in classStudents)
                    {
                        bool isNewlyAdded = false;
                        if (!existingEnrollmentPairs.Contains((sec.Id, st.Id)))
                        {
                            newEnrollments.Add(new CourseSectionStudent
                            {
                                CourseSectionId = sec.Id,
                                StudentId = st.Id,
                                EnrolledAt = DateTime.UtcNow,
                                Status = "Enrolled"
                            });
                            existingEnrollmentPairs.Add((sec.Id, st.Id));
                            totalEnrollmentsCreated++;
                            isNewlyAdded = true;
                        }

                        // Tạo bảng điểm khởi tạo
                        if (gradeComponentsBySubject.TryGetValue(sec.SubjectId, out var gcs))
                        {
                            foreach (var gc in gcs)
                            {
                                if (!existingGradeKeys.Contains((sec.Id, st.Id, gc.Id)))
                                {
                                    newGrades.Add(new StudentGrade
                                    {
                                        CourseSectionId = sec.Id,
                                        StudentId = st.Id,
                                        SubjectGradeComponentId = gc.Id,
                                        Score = null
                                    });
                                    existingGradeKeys.Add((sec.Id, st.Id, gc.Id));
                                }
                            }
                        }

                        allocationsList.Add(new AllocationItemDto
                        {
                            StudentId = st.Id,
                            StudentCode = st.StudentCode,
                            StudentName = st.User?.FullName ?? "",
                            SectionCode = sec.Code,
                            SubjectCode = sec.Code.Split('_').FirstOrDefault() ?? "",
                            AdministrativeClassCode = adminClass.Code,
                            IsAutoCreated = isNewlyAdded
                        });
                    }
                }
            }

            // Lưu tất cả enrollments và grades một lần
            if (newEnrollments.Any())
            {
                _db.CourseSectionStudents.AddRange(newEnrollments);
            }
            if (newGrades.Any())
            {
                _db.StudentGrades.AddRange(newGrades);
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("ALLOCATION_RUN", "Semester", semester.Id.ToString(), performedBy,
                $"Phân bổ sinh viên học kỳ {semester.Code}: Xử lý {adminClasses.Count} lớp hành chính, {allocatedStudentIds.Count} sinh viên ({totalAutoCreated} sinh viên tạo mới tự động), tạo thành công {totalEnrollmentsCreated} lượt xếp lớp học phần.");

            return new AllocationResultDto
            {
                SemesterId = semester.Id,
                SemesterCode = semester.Code ?? string.Empty,
                TotalStudentsProcessed = allocatedStudentIds.Count,
                TotalEligibleStudents = allocatedStudentIds.Count,
                TotalEnrolledSections = totalEnrollmentsCreated,
                AutoCreatedStudentsCount = totalAutoCreated,
                BlockedPrerequisiteCount = 0,
                CapacityOverflowCount = 0,
                UnallocatedCount = 0,
                Allocations = allocationsList,
                UnallocatedExceptions = new List<UnallocatedExceptionDto>(),
                ClassSummaries = classSummaries.OrderBy(c => c.ClassCode).ToList()
            };
        }

        public async Task<AllocationResultDto> GetAllocationStatusAsync(int semesterId)
        {
            // 1. Xác định học kỳ
            Semester? semester = null;
            if (semesterId > 0)
            {
                semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);
            }
            if (semester == null)
            {
                semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Status == "Active");
            }
            if (semester == null)
            {
                return new AllocationResultDto();
            }

            // 2. Nhận diện mùa học từ semester (VD: FA26 / Fall 26, SU26 / Summer 26, SP26 / Spring 26)
            var semCode = (semester.Code ?? "").Trim().ToUpper();
            var semName = (semester.Name ?? "").Trim().ToUpper();
            string seasonKeyword = semCode.StartsWith("FA") ? "FALL" : 
                                   semCode.StartsWith("SP") ? "SPRING" : 
                                   semCode.StartsWith("SU") ? "SUMMER" : semCode;

            // 3. Lấy các lớp hành chính thuộc mùa học này
            var seasonClasses = await _db.AdministrativeClasses
                .Include(ac => ac.Major)
                .Include(ac => ac.Specialization)
                .Where(ac => ac.IsActive)
                .Where(ac => ac.AcademicYear.ToUpper() == semCode || 
                             ac.AcademicYear.ToUpper().Contains(semCode) ||
                             ac.AcademicYear.ToUpper().Contains(seasonKeyword) ||
                             semName.Contains(ac.AcademicYear.ToUpper()))
                .OrderBy(ac => ac.Code)
                .ToListAsync();

            if (!seasonClasses.Any())
            {
                return new AllocationResultDto
                {
                    SemesterId = semester.Id,
                    SemesterCode = semester.Code ?? string.Empty
                };
            }

            var seasonClassIds = seasonClasses.Select(c => c.Id).ToHashSet();

            // 4. Lấy sinh viên đã được phân bổ vào các lớp hành chính của mùa này
            var studentsInSeason = await _db.Students
                .Include(s => s.User)
                .Where(s => s.AdministrativeClassId.HasValue && seasonClassIds.Contains(s.AdministrativeClassId.Value))
                .ToListAsync();

            var studentsByClass = studentsInSeason
                .GroupBy(s => s.AdministrativeClassId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Chỉ lấy các lớp ĐÃ ĐƯỢC PHÂN BỔ TRƯỚC ĐÓ trong mùa đó (StudentCount > 0)
            var allocatedClasses = seasonClasses
                .Where(ac => studentsByClass.ContainsKey(ac.Id) && studentsByClass[ac.Id].Count > 0)
                .ToList();

            if (!allocatedClasses.Any())
            {
                return new AllocationResultDto
                {
                    SemesterId = semester.Id,
                    SemesterCode = semester.Code ?? string.Empty
                };
            }

            // 5. Lấy các môn học phần tương ứng trong học kỳ này
            var sectionsInSemester = await _db.CourseSections
                .Where(cs => cs.SemesterId == semester.Id && cs.AdministrativeClassId.HasValue && seasonClassIds.Contains(cs.AdministrativeClassId.Value))
                .ToListAsync();

            var sectionsByClass = sectionsInSemester
                .GroupBy(cs => cs.AdministrativeClassId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 6. Lấy thông tin enrollments
            var sectionIds = sectionsInSemester.Select(s => s.Id).ToHashSet();
            var enrollments = await _db.CourseSectionStudents
                .Include(css => css.CourseSection)
                .Include(css => css.Student)
                    .ThenInclude(st => st!.User)
                .Where(css => sectionIds.Contains(css.CourseSectionId))
                .ToListAsync();

            var classSummaries = new List<ClassAllocationSummaryDto>();
            int totalStudents = 0;
            int totalAutoCreated = 0;

            foreach (var ac in allocatedClasses)
            {
                var classStudents = studentsByClass.TryGetValue(ac.Id, out var studs) ? studs : new List<Student>();
                var classSecs = sectionsByClass.TryGetValue(ac.Id, out var secs) ? secs : new List<CourseSection>();

                totalStudents += classStudents.Count;

                int autoCreatedInClass = classStudents.Count(s => s.User != null && s.User.CreatedAt >= DateTime.UtcNow.AddDays(-7));
                totalAutoCreated += autoCreatedInClass;

                classSummaries.Add(new ClassAllocationSummaryDto
                {
                    ClassId = ac.Id,
                    ClassCode = ac.Code,
                    MajorCode = ac.Major?.Code ?? "",
                    SpecializationCode = ac.Specialization?.Code,
                    SemesterNumber = ac.CurrentSemester,
                    StudentCount = classStudents.Count,
                    SectionsCount = classSecs.Count,
                    AutoCreatedCount = autoCreatedInClass
                });
            }

            var allocationsList = new List<AllocationItemDto>();
            foreach (var en in enrollments)
            {
                var sec = en.CourseSection;
                var st = en.Student;
                if (sec == null || st == null) continue;

                var adminClass = seasonClasses.FirstOrDefault(ac => ac.Id == sec.AdministrativeClassId);
                allocationsList.Add(new AllocationItemDto
                {
                    StudentId = st.Id,
                    StudentCode = st.StudentCode,
                    StudentName = st.User?.FullName ?? st.StudentCode,
                    SectionCode = sec.Code,
                    SubjectCode = sec.Code.Split('_').FirstOrDefault() ?? "",
                    AdministrativeClassCode = adminClass?.Code ?? "",
                    IsAutoCreated = st.User != null && st.User.CreatedAt >= DateTime.UtcNow.AddDays(-7)
                });
            }

            return new AllocationResultDto
            {
                SemesterId = semester.Id,
                SemesterCode = semester.Code ?? string.Empty,
                TotalStudentsProcessed = totalStudents,
                TotalEligibleStudents = totalStudents,
                TotalEnrolledSections = enrollments.Count,
                AutoCreatedStudentsCount = totalAutoCreated,
                ClassSummaries = classSummaries.OrderBy(c => c.ClassCode).ToList(),
                Allocations = allocationsList
            };
        }

        public async Task<List<UnallocatedExceptionDto>> GetUnallocatedExceptionsAsync(int semesterId)
        {
            var request = new RunAllocationRequest { SemesterId = semesterId };
            var res = await RunAllocationAsync(request, "AuditCheck");
            return res.UnallocatedExceptions;
        }

        public async Task<bool> EnrollSingleStudentAsync(int courseSectionId, int studentId, string performedBy)
        {
            var section = await _db.CourseSections.Include(cs => cs.Schedules).FirstOrDefaultAsync(cs => cs.Id == courseSectionId);
            if (section == null) throw new InvalidOperationException("Lớp học phần không tồn tại.");

            var student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) throw new InvalidOperationException("Sinh viên không tồn tại.");

            var alreadyEnrolled = await _db.CourseSectionStudents.AnyAsync(css => css.CourseSectionId == courseSectionId && css.StudentId == studentId);
            if (alreadyEnrolled) return true;

            var enrolledCount = await _db.CourseSectionStudents.CountAsync(css => css.CourseSectionId == courseSectionId);
            if (enrolledCount >= section.MaxCapacity)
            {
                throw new InvalidOperationException("Lớp học phần đã đầy sức chứa (tối đa 31 sinh viên).");
            }

            _db.CourseSectionStudents.Add(new CourseSectionStudent
            {
                CourseSectionId = courseSectionId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                Status = "Enrolled"
            });

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("STUDENT_ENROLLED", "CourseSection", courseSectionId.ToString(), performedBy,
                $"Phân bổ sinh viên {student.StudentCode} vào lớp học phần {section.Code}");

            return true;
        }

        public async Task<bool> RemoveStudentFromSectionAsync(int courseSectionId, int studentId, string performedBy)
        {
            var enrollment = await _db.CourseSectionStudents
                .FirstOrDefaultAsync(css => css.CourseSectionId == courseSectionId && css.StudentId == studentId);

            if (enrollment == null) return false;

            _db.CourseSectionStudents.Remove(enrollment);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("STUDENT_UNENROLLED", "CourseSection", courseSectionId.ToString(), performedBy,
                $"Hủy sinh viên ID {studentId} khỏi lớp học phần {courseSectionId}");

            return true;
        }

        /// <summary>
        /// Khớp sinh viên tự do với lớp hành chính theo tiêu chuẩn:
        /// - Kì sinh viên == Kì lớp
        /// - Ngành sinh viên == Ngành lớp
        /// - Đối với kì 5, 7, 8: dựa thêm vào ngành hẹp (nếu lớp có ngành hẹp thì SV phải khớp ngành hẹp)
        /// </summary>
        private static bool MatchesCriteria(Student st, AdministrativeClass ac)
        {
            if (st.CurrentSemester != ac.CurrentSemester) return false;
            if (st.MajorId != ac.MajorId) return false;

            bool isSpecializedSem = ac.CurrentSemester == 5 || ac.CurrentSemester == 7 || ac.CurrentSemester == 8;
            if (isSpecializedSem)
            {
                if (ac.SpecializationId.HasValue && ac.SpecializationId.Value > 0)
                {
                    // Lớp có ngành hẹp: Sinh viên phải có cùng ngành hẹp hoặc chưa chọn ngành hẹp
                    return !st.SpecializationId.HasValue || st.SpecializationId.Value == ac.SpecializationId.Value;
                }
                else
                {
                    // Lớp chung / cơ sở ngành
                    return true;
                }
            }
            else
            {
                if (ac.SpecializationId.HasValue && ac.SpecializationId.Value > 0)
                {
                    return !st.SpecializationId.HasValue || st.SpecializationId.Value == ac.SpecializationId.Value;
                }
                return true;
            }
        }

        private static string GetMajorStudentPrefix(string majorCode)
        {
            return majorCode.ToUpper() switch
            {
                "SE" or "IA" or "AI" or "DS" or "IT" => "HE",
                "BA" or "MC" => "HS",
                "GD" => "HA",
                _ => "HE"
            };
        }

        private static (string code, string username, string email) GenerateUniqueCredentials(
            string prefix, 
            int cohort, 
            HashSet<string> usedCodes, 
            HashSet<string> usedUsernames, 
            HashSet<string> usedEmails)
        {
            int seq = 1;
            while (true)
            {
                var code = $"{prefix}{cohort}{seq:D4}";
                var username = code.ToLower();
                var email = $"{username}@fpt.edu.vn";

                if (!usedCodes.Contains(code) && !usedUsernames.Contains(username) && !usedEmails.Contains(email))
                {
                    usedCodes.Add(code);
                    usedUsernames.Add(username);
                    usedEmails.Add(email);
                    return (code, username, email);
                }
                seq++;
            }
        }
    }
}