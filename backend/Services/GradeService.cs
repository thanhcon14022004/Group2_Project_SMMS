using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IGradeService
    {
        Task<SectionGradeSheetDto> GetSectionGradeSheetAsync(int courseSectionId, int? currentUserId = null, string? userRole = null);
        Task<bool> SaveSectionGradesAsync(UpdateStudentGradeRequest request, string performedBy, int? lecturerId = null, bool isAdmin = false);
        Task<bool> SetGradeLockAsync(int courseSectionId, bool isLocked, string performedBy);
        Task<decimal?> CalculateFinalScoreAsync(int courseSectionId, int studentId);
        Task<StudentSemesterTranscriptDto> GetStudentTranscriptAsync(int studentId);
    }

    public class GradeService : IGradeService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;

        public GradeService(AppDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<SectionGradeSheetDto> GetSectionGradeSheetAsync(int courseSectionId, int? currentUserId = null, string? userRole = null)
        {
            // 1. Tải thông tin lớp học phần (truy vấn 1:1, không join bảng quan hệ nhiều-nhiều)
            var section = await _db.CourseSections
                .AsNoTracking()
                .Include(cs => cs.Subject)
                .Include(cs => cs.Semester)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.AdministrativeClass)
                .FirstOrDefaultAsync(cs => cs.Id == courseSectionId);

            if (section == null) throw new InvalidOperationException("Lớp học phần không tồn tại.");

            // Kiểm tra quyền của giảng viên: Giảng viên CHỈ được xem lớp được phân công
            if (userRole == "Lecturer")
            {
                var lecturer = await _db.Lecturers.AsNoTracking().FirstOrDefaultAsync(l => l.UserId == currentUserId);
                if (lecturer == null || section.LecturerId != lecturer.Id)
                {
                    throw new UnauthorizedAccessException("Bạn không được phân công phụ trách lớp học phần này.");
                }
            }

            bool withinDeadline = section.Semester != null && DateTime.UtcNow <= section.Semester.GradeEntryDeadline;
            bool canEdit = (userRole == "Admin") || (!section.IsGradeLocked && withinDeadline && userRole == "Lecturer");

            // 2. Tải danh sách đầu điểm thành phần của môn học (truy vấn riêng theo SubjectId)
            var components = await _db.SubjectGradeComponents
                .AsNoTracking()
                .Where(c => c.SubjectId == section.SubjectId)
                .OrderBy(c => c.OrderIndex)
                .Select(c => new SubjectGradeComponentDto
                {
                    Id = c.Id,
                    SubjectId = c.SubjectId,
                    Name = c.Name,
                    Weight = c.Weight,
                    Scale = c.Scale,
                    IsAdminOnly = c.IsAdminOnly,
                    OrderIndex = c.OrderIndex
                })
                .ToListAsync();

            // 3. Tải danh sách sinh viên ghi danh trong lớp học phần (truy vấn riêng dùng index)
            var enrollments = await _db.CourseSectionStudents
                .AsNoTracking()
                .Where(e => e.CourseSectionId == courseSectionId && e.Status == "Enrolled")
                .Select(e => new
                {
                    e.StudentId,
                    StudentCode = e.Student != null ? e.Student.StudentCode : "",
                    FullName = e.Student != null && e.Student.User != null ? e.Student.User.FullName : "",
                    AdminClassCode = e.Student != null && e.Student.AdministrativeClass != null ? e.Student.AdministrativeClass.Code : ""
                })
                .OrderBy(e => e.StudentCode)
                .ToListAsync();

            // 4. Tải tất cả điểm của lớp học phần này (truy vấn riêng theo CourseSectionId)
            var grades = await _db.StudentGrades
                .AsNoTracking()
                .Where(g => g.CourseSectionId == courseSectionId)
                .Select(g => new { g.StudentId, g.SubjectGradeComponentId, g.Score })
                .ToListAsync();

            var gradeLookup = grades
                .GroupBy(g => (g.StudentId, g.SubjectGradeComponentId))
                .ToDictionary(g => g.Key, g => (decimal?)g.First().Score);

            var studentRows = new List<StudentGradeRowDto>();

            foreach (var en in enrollments)
            {
                var compScores = new Dictionary<int, decimal?>();

                foreach (var c in components)
                {
                    if (gradeLookup.TryGetValue((en.StudentId, c.Id), out var score))
                    {
                        compScores[c.Id] = score;
                    }
                    else
                    {
                        compScores[c.Id] = null;
                    }
                }

                // Kiểm tra xem sinh viên đã có đủ tất cả các đầu điểm thành phần chưa
                bool isCompleted = components.Count > 0 && components.All(c => compScores.TryGetValue(c.Id, out var sc) && sc.HasValue);

                // Tính điểm tổng kết
                decimal? finalScore = ComputeFinalScore(compScores, components);
                bool isPassed = isCompleted && finalScore.HasValue && finalScore.Value >= 5.0m;

                studentRows.Add(new StudentGradeRowDto
                {
                    StudentId = en.StudentId,
                    StudentCode = en.StudentCode,
                    FullName = en.FullName,
                    AdministrativeClassCode = !string.IsNullOrEmpty(en.AdminClassCode) ? en.AdminClassCode : (section.AdministrativeClass?.Code ?? "Chưa có lớp"),
                    ComponentScores = compScores,
                    FinalScore = finalScore,
                    IsPassed = isPassed,
                    IsCompleted = isCompleted
                });
            }

            return new SectionGradeSheetDto
            {
                CourseSectionId = section.Id,
                CourseSectionCode = section.Code,
                SubjectCode = section.Subject?.Code ?? "",
                SubjectName = section.Subject?.Name ?? "",
                Credits = section.Subject?.Credits ?? 0,
                AdministrativeClassId = section.AdministrativeClassId,
                AdministrativeClassCode = section.AdministrativeClass?.Code,
                LecturerName = section.Lecturer?.User?.FullName ?? "Chưa phân công",
                SemesterCode = section.Semester?.Code ?? "",
                IsGradeLocked = section.IsGradeLocked,
                CanEdit = canEdit,
                Components = components,
                StudentRows = studentRows
            };
        }

        public async Task<bool> SaveSectionGradesAsync(UpdateStudentGradeRequest request, string performedBy, int? lecturerId = null, bool isAdmin = false)
        {
            var section = await _db.CourseSections
                .Include(cs => cs.Semester)
                .Include(cs => cs.Subject)
                    .ThenInclude(s => s!.GradeComponents)
                .FirstOrDefaultAsync(cs => cs.Id == request.CourseSectionId);

            if (section == null) throw new InvalidOperationException("Lớp học phần không tồn tại.");

            // Kiểm tra phân quyền:
            if (!isAdmin)
            {
                if (section.LecturerId != lecturerId)
                    throw new UnauthorizedAccessException("Bạn không có quyền nhập điểm cho lớp học phần này.");

                if (section.IsGradeLocked)
                    throw new InvalidOperationException("Điểm của lớp học phần này đã bị khóa. Vui lòng liên hệ Admin để mở khóa.");

                if (section.Semester != null && DateTime.UtcNow > section.Semester.GradeEntryDeadline)
                    throw new InvalidOperationException("Đã hết hạn nhập điểm cho học kỳ này theo quy định.");
            }

            var componentsDict = section.Subject?.GradeComponents.ToDictionary(c => c.Id) ?? new();

            int updatedCount = 0;

            foreach (var item in request.Grades)
            {
                if (!componentsDict.TryGetValue(item.SubjectGradeComponentId, out var comp))
                {
                    continue; // Đầu điểm không thuộc môn
                }

                // Kiểm tra phân quyền sửa đầu điểm:
                if (isAdmin)
                {
                    // Admin chỉ được nhập các đầu điểm có quyền nhập (IsAdminOnly = true)
                    // Các đầu điểm của giảng viên (Quiz, Lab, Assignment...) thì admin không can thiệp/nhập
                    if (!comp.IsAdminOnly)
                    {
                        continue;
                    }
                }
                else
                {
                    // Giảng viên chỉ được nhập các đầu điểm thường, không được sửa đầu điểm Chỉ Admin
                    if (comp.IsAdminOnly)
                    {
                        continue;
                    }
                }

                // Validate khoảng điểm: từ 0.0 đến 10.0 (hoặc comp.Scale)
                if (item.Score.HasValue)
                {
                    if (item.Score.Value < 0m || item.Score.Value > comp.Scale)
                    {
                        throw new InvalidOperationException($"Điểm cho đầu điểm '{comp.Name}' phải nằm trong thang điểm 0 - {comp.Scale}.");
                    }
                }

                var gradeRecord = await _db.StudentGrades
                    .FirstOrDefaultAsync(g => g.CourseSectionId == request.CourseSectionId &&
                                              g.StudentId == item.StudentId &&
                                              g.SubjectGradeComponentId == item.SubjectGradeComponentId);

                if (gradeRecord == null)
                {
                    gradeRecord = new StudentGrade
                    {
                        CourseSectionId = request.CourseSectionId,
                        StudentId = item.StudentId,
                        SubjectGradeComponentId = item.SubjectGradeComponentId,
                        Score = item.Score,
                        Notes = item.Notes,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = performedBy
                    };
                    _db.StudentGrades.Add(gradeRecord);
                }
                else
                {
                    gradeRecord.Score = item.Score;
                    gradeRecord.Notes = item.Notes;
                    gradeRecord.UpdatedAt = DateTime.UtcNow;
                    gradeRecord.UpdatedBy = performedBy;
                }

                updatedCount++;
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("GRADE_UPDATE", "CourseSection", request.CourseSectionId.ToString(), performedBy,
                $"Cập nhật {updatedCount} đầu điểm cho lớp học phần {section.Code}");

            return true;
        }

        public async Task<bool> SetGradeLockAsync(int courseSectionId, bool isLocked, string performedBy)
        {
            var section = await _db.CourseSections.FindAsync(courseSectionId);
            if (section == null) return false;

            section.IsGradeLocked = isLocked;
            section.GradeLockedAt = isLocked ? DateTime.UtcNow : null;
            section.GradeLockedBy = isLocked ? performedBy : null;

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                isLocked ? "GRADE_LOCKED" : "GRADE_UNLOCKED",
                "CourseSection",
                courseSectionId.ToString(),
                performedBy,
                $"Admin {performedBy} đã {(isLocked ? "khóa" : "mở khóa")} bảng điểm lớp {section.Code}"
            );

            return true;
        }

        public async Task<decimal?> CalculateFinalScoreAsync(int courseSectionId, int studentId)
        {
            var section = await _db.CourseSections
                .Include(cs => cs.Subject)
                    .ThenInclude(s => s!.GradeComponents)
                .Include(cs => cs.StudentGrades)
                .FirstOrDefaultAsync(cs => cs.Id == courseSectionId);

            if (section?.Subject == null) return null;

            var components = section.Subject.GradeComponents.ToList();
            if (components.Count == 0) return null;

            var compScores = new Dictionary<int, decimal?>();
            foreach (var c in components)
            {
                var g = section.StudentGrades.FirstOrDefault(x => x.StudentId == studentId && x.SubjectGradeComponentId == c.Id);
                compScores[c.Id] = g?.Score;
            }

            var compDtos = components.Select(c => new SubjectGradeComponentDto
            {
                Id = c.Id,
                Weight = c.Weight,
                Scale = c.Scale
            }).ToList();

            return ComputeFinalScore(compScores, compDtos);
        }

        public async Task<StudentSemesterTranscriptDto> GetStudentTranscriptAsync(int studentId)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                .Include(s => s.CourseSectionEnrollments)
                    .ThenInclude(e => e.CourseSection)
                        .ThenInclude(cs => cs!.Subject)
                            .ThenInclude(s => s!.GradeComponents)
                .Include(s => s.CourseSectionEnrollments)
                    .ThenInclude(e => e.CourseSection)
                        .ThenInclude(cs => cs!.Semester)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) throw new InvalidOperationException("Sinh viên không tồn tại.");

            var transcript = new StudentSemesterTranscriptDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.User?.FullName ?? "",
                MajorName = student.Major?.Name ?? "",
                SpecializationName = student.Specialization?.Name,
                CurrentSemester = student.CurrentSemester
            };

            decimal totalWeightedPoints = 0m;
            int totalEnrolledCredits = 0;
            int totalPassedCredits = 0;

            foreach (var en in student.CourseSectionEnrollments.OrderBy(e => e.CourseSection?.Semester?.StartDate))
            {
                var cs = en.CourseSection;
                if (cs?.Subject == null) continue;

                var compDtos = new List<ComponentScoreDto>();
                var compScoreMap = new Dictionary<int, decimal?>();

                var components = cs.Subject.GradeComponents.OrderBy(c => c.OrderIndex).ToList();
                foreach (var c in components)
                {
                    var gradeRecord = student.Grades.FirstOrDefault(g => g.CourseSectionId == cs.Id && g.SubjectGradeComponentId == c.Id);
                    compScoreMap[c.Id] = gradeRecord?.Score;
                    compDtos.Add(new ComponentScoreDto
                    {
                        ComponentName = c.Name,
                        Weight = c.Weight,
                        Score = gradeRecord?.Score
                    });
                }

                var compList = components.Select(c => new SubjectGradeComponentDto
                {
                    Id = c.Id,
                    Weight = c.Weight,
                    Scale = c.Scale
                }).ToList();

                decimal? finalScore = ComputeFinalScore(compScoreMap, compList);
                bool isPassed = finalScore.HasValue && finalScore.Value >= 5.0m;

                transcript.SubjectRecords.Add(new StudentSubjectTranscriptDto
                {
                    CourseSectionId = cs.Id,
                    SemesterCode = cs.Semester?.Code ?? "",
                    SubjectCode = cs.Subject.Code,
                    SubjectName = cs.Subject.Name,
                    Credits = cs.Subject.Credits,
                    Components = compDtos,
                    FinalScore = finalScore,
                    IsPassed = isPassed,
                    IsLocked = cs.IsGradeLocked
                });

                // Quy tắc tính GPA:
                // Thang 10, tính theo tín chỉ: tổng (điểm môn x tín chỉ) / tổng tín chỉ.
                // MÔN TRƯỢT VẪN ĐƯỢC TÍNH VÀO GPA TÍCH LŨY theo điểm thực tế và tín chỉ!
                if (finalScore.HasValue)
                {
                    totalWeightedPoints += finalScore.Value * cs.Subject.Credits;
                    totalEnrolledCredits += cs.Subject.Credits;
                    if (isPassed)
                    {
                        totalPassedCredits += cs.Subject.Credits;
                    }
                }
            }

            transcript.TotalEnrolledCredits = totalEnrolledCredits;
            transcript.TotalPassedCredits = totalPassedCredits;
            transcript.CumulativeGpa = totalEnrolledCredits > 0
                ? Math.Round(totalWeightedPoints / totalEnrolledCredits, 2)
                : 0.0m;

            // Xếp loại học lực
            transcript.AcademicStanding = transcript.CumulativeGpa switch
            {
                >= 9.0m => "Xuất sắc",
                >= 8.0m => "Giỏi",
                >= 6.5m => "Khá",
                >= 5.0m => "Trung bình",
                _ => "Yếu"
            };

            return transcript;
        }

        private static decimal? ComputeFinalScore(Dictionary<int, decimal?> compScores, List<SubjectGradeComponentDto> components)
        {
            if (components.Count == 0) return null;

            // Nếu chưa nhập bất kỳ đầu điểm nào -> null
            if (compScores.Values.All(v => !v.HasValue)) return null;

            decimal total = 0m;
            decimal totalWeightCalculated = 0m;

            foreach (var c in components)
            {
                if (compScores.TryGetValue(c.Id, out var score) && score.HasValue)
                {
                    // Điểm thành phần x (tỷ trọng / 100)
                    total += score.Value * (c.Weight / 100.0m);
                    totalWeightCalculated += c.Weight;
                }
            }

            // Quy tắc làm tròn: làm tròn đến 1 chữ số thập phân (e.g. 7.26 -> 7.3)
            return Math.Round(total, 1, MidpointRounding.AwayFromZero);
        }
    }
}

