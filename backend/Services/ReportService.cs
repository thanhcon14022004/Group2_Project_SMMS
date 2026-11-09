using System.Text;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;

namespace project_PRN232.Services
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<LecturerDashboardDto?> GetLecturerDashboardAsync(int userId);
        Task<List<StudentSemesterTranscriptDto>> GetStudentsGpaReportAsync(int? majorId = null, int? administrativeClassId = null);
        Task<byte[]> ExportGpaReportCsvAsync(int? majorId = null, int? administrativeClassId = null);
        Task<byte[]> ExportSectionGradesCsvAsync(int courseSectionId);
    }

    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        private readonly IGradeService _gradeService;

        public ReportService(AppDbContext db, IGradeService gradeService)
        {
            _db = db;
            _gradeService = gradeService;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var totalStudents = await _db.Students.CountAsync(s => s.User!.IsActive);
            var totalLecturers = await _db.Lecturers.CountAsync(l => l.User!.IsActive);
            var totalCourses = await _db.Subjects.CountAsync(s => s.IsActive);
            var totalSections = await _db.CourseSections.CountAsync();

            var activeSemester = await _db.Semesters
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync() ?? await _db.Semesters.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();

            int sectionsLackingLecturer = 0;
            int sectionsLackingGrades = 0;
            int unallocatedExceptions = 0;

            if (activeSemester != null)
            {
                sectionsLackingLecturer = await _db.CourseSections
                    .CountAsync(cs => cs.SemesterId == activeSemester.Id && !cs.LecturerId.HasValue);

                // Đếm lớp chưa hoàn thành nhập điểm (chưa khóa điểm)
                sectionsLackingGrades = await _db.CourseSections
                    .CountAsync(cs => cs.SemesterId == activeSemester.Id && !cs.IsGradeLocked);
            }

            // Tính phân phối GPA siêu tốc bằng 1 câu truy vấn GroupBy duy nhất thay vì N+1 queries
            var studentScores = await _db.StudentGrades
                .Where(g => g.Score.HasValue)
                .GroupBy(g => g.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AvgScore = g.Average(x => (double)x.Score!.Value)
                })
                .ToListAsync();

            int countXuatSac = 0, countGioi = 0, countKha = 0, countTB = 0, countYeu = 0;
            decimal totalGpa = 0m;
            int gpaCount = studentScores.Count;

            foreach (var sc in studentScores)
            {
                var gpa = Math.Round((decimal)sc.AvgScore, 2);
                totalGpa += gpa;

                if (gpa >= 9.0m) countXuatSac++;
                else if (gpa >= 8.0m) countGioi++;
                else if (gpa >= 6.5m) countKha++;
                else if (gpa >= 5.0m) countTB++;
                else countYeu++;
            }

            decimal avgGpa = gpaCount > 0 ? Math.Round(totalGpa / gpaCount, 2) : 0m;

            var distribution = new List<GpaDistributionDto>
            {
                new() { Rank = "Xuất sắc (>= 9.0)", Count = countXuatSac, Percentage = gpaCount > 0 ? Math.Round((decimal)countXuatSac / gpaCount * 100, 1) : 0 },
                new() { Rank = "Giỏi (8.0 - 8.9)", Count = countGioi, Percentage = gpaCount > 0 ? Math.Round((decimal)countGioi / gpaCount * 100, 1) : 0 },
                new() { Rank = "Khá (6.5 - 7.9)", Count = countKha, Percentage = gpaCount > 0 ? Math.Round((decimal)countKha / gpaCount * 100, 1) : 0 },
                new() { Rank = "Trung bình (5.0 - 6.4)", Count = countTB, Percentage = gpaCount > 0 ? Math.Round((decimal)countTB / gpaCount * 100, 1) : 0 },
                new() { Rank = "Yếu (< 5.0)", Count = countYeu, Percentage = gpaCount > 0 ? Math.Round((decimal)countYeu / gpaCount * 100, 1) : 0 }
            };

            return new DashboardSummaryDto
            {
                TotalStudents = totalStudents,
                TotalLecturers = totalLecturers,
                TotalCourses = totalCourses,
                TotalSections = totalSections,
                ActiveSemesterId = activeSemester?.Id ?? 0,
                ActiveSemesterName = activeSemester?.Name ?? "Chưa có",
                SectionsLackingLecturerCount = sectionsLackingLecturer,
                SectionsLackingGradesCount = sectionsLackingGrades,
                UnallocatedExceptionsCount = unallocatedExceptions,
                AverageSystemGpa = avgGpa,
                GpaDistribution = distribution
            };
        }

        public async Task<LecturerDashboardDto?> GetLecturerDashboardAsync(int userId)
        {
            var lecturer = await _db.Lecturers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (lecturer == null) return null;

            // Lấy các lớp học phần giảng viên HIỆN ĐANG GIẢNG DẠY
            // Điều kiện: Thuộc học kỳ chưa kết thúc (Status != "Ended" && Status != "Closed") và Lớp chưa đóng (Status != "Completed")
            var activeSections = await _db.CourseSections
                .Include(cs => cs.Semester)
                .Include(cs => cs.Subject)
                .Include(cs => cs.Schedules)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.LecturerId == lecturer.Id 
                             && cs.Semester != null 
                             && cs.Semester.Status != "Ended" 
                             && cs.Semester.Status != "Closed" 
                             && cs.Status != "Completed")
                .OrderByDescending(cs => cs.Semester!.StartDate)
                .ThenBy(cs => cs.Code)
                .ToListAsync();

            var activeSectionIds = activeSections.Select(cs => cs.Id).ToList();

            // Sĩ số sinh viên thực tế mà GV đang trực tiếp giảng dạy
            var totalStudents = activeSections.SelectMany(cs => cs.Enrollments).Select(e => e.StudentId).Distinct().Count();

            // Tiến độ nhập điểm của các lớp đang dạy
            var lockedCount = activeSections.Count(cs => cs.IsGradeLocked);
            var pendingCount = activeSections.Count(cs => !cs.IsGradeLocked);

            // Tổng số môn GV đã đăng ký giảng dạy
            var registeredSubjectsCount = await _db.LecturerSubjects.CountAsync(ls => ls.LecturerId == lecturer.Id);

            // Tên kỳ học hiện tại (lấy từ các lớp đang dạy hoặc kỳ gần nhất)
            var activeSemesterNames = activeSections
                .Select(cs => cs.Semester?.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            string semesterDisplayName;
            if (activeSemesterNames.Any())
            {
                semesterDisplayName = string.Join(", ", activeSemesterNames);
            }
            else
            {
                var latestSemester = await _db.Semesters
                    .Where(s => s.Status != "Ended" && s.Status != "Closed")
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync() ?? await _db.Semesters.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();
                semesterDisplayName = latestSemester?.Name ?? "Chưa có";
            }

            // Hạn nhập điểm của kỳ gần nhất
            var latestActiveSem = activeSections.Select(cs => cs.Semester).FirstOrDefault()
                ?? await _db.Semesters.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();

            // Format thông tin các lớp học phần đang dạy
            var assignedSections = activeSections.Select(cs => new LecturerSectionSummaryDto
            {
                Id = cs.Id,
                Code = cs.Code,
                SemesterName = cs.Semester?.Name ?? "",
                SubjectCode = cs.Subject?.Code ?? "",
                SubjectName = cs.Subject?.Name ?? "",
                Credits = cs.Subject?.Credits ?? 0,
                ScheduleSummary = FormatSchedule(cs.Schedules),
                StudentCount = cs.Enrollments.Count,
                MaxCapacity = cs.MaxCapacity,
                IsGradeLocked = cs.IsGradeLocked,
                GradeLockedAt = cs.GradeLockedAt
            }).ToList();

            // Phổ điểm của sinh viên các lớp do giảng viên này phụ trách
            var teachingScores = await _db.StudentGrades
                .Where(g => activeSectionIds.Contains(g.CourseSectionId) && g.Score.HasValue)
                .GroupBy(g => g.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AvgScore = g.Average(x => (double)x.Score!.Value)
                })
                .ToListAsync();

            if (!teachingScores.Any())
            {
                // Fallback: nếu các lớp hiện tại chưa nhập điểm thì lấy điểm tất cả các lớp GV từng dạy để biểu đồ trực quan
                teachingScores = await _db.StudentGrades
                    .Where(g => _db.CourseSections.Where(cs => cs.LecturerId == lecturer.Id).Select(cs => cs.Id).Contains(g.CourseSectionId) && g.Score.HasValue)
                    .GroupBy(g => g.StudentId)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        AvgScore = g.Average(x => (double)x.Score!.Value)
                    })
                    .ToListAsync();
            }

            int countXuatSac = 0, countGioi = 0, countKha = 0, countTB = 0, countYeu = 0;
            decimal totalScore = 0m;
            int gpaCount = teachingScores.Count;

            foreach (var sc in teachingScores)
            {
                var score = Math.Round((decimal)sc.AvgScore, 2);
                totalScore += score;

                if (score >= 9.0m) countXuatSac++;
                else if (score >= 8.0m) countGioi++;
                else if (score >= 6.5m) countKha++;
                else if (score >= 5.0m) countTB++;
                else countYeu++;
            }

            decimal avgTeachingScore = gpaCount > 0 ? Math.Round(totalScore / gpaCount, 2) : 0m;

            var distribution = new List<GpaDistributionDto>
            {
                new() { Rank = "Xuất sắc (>= 9.0)", Count = countXuatSac, Percentage = gpaCount > 0 ? Math.Round((decimal)countXuatSac / gpaCount * 100, 1) : 0 },
                new() { Rank = "Giỏi (8.0 - 8.9)", Count = countGioi, Percentage = gpaCount > 0 ? Math.Round((decimal)countGioi / gpaCount * 100, 1) : 0 },
                new() { Rank = "Khá (6.5 - 7.9)", Count = countKha, Percentage = gpaCount > 0 ? Math.Round((decimal)countKha / gpaCount * 100, 1) : 0 },
                new() { Rank = "Trung bình (5.0 - 6.4)", Count = countTB, Percentage = gpaCount > 0 ? Math.Round((decimal)countTB / gpaCount * 100, 1) : 0 },
                new() { Rank = "Yếu (< 5.0)", Count = countYeu, Percentage = gpaCount > 0 ? Math.Round((decimal)countYeu / gpaCount * 100, 1) : 0 }
            };

            return new LecturerDashboardDto
            {
                LecturerId = lecturer.Id,
                LecturerCode = lecturer.LecturerCode,
                FullName = lecturer.User?.FullName ?? "",
                AcademicTitle = lecturer.AcademicTitle,
                Department = lecturer.Department,
                ActiveSemesterId = latestActiveSem?.Id ?? 0,
                ActiveSemesterName = semesterDisplayName,
                GradeEntryDeadline = latestActiveSem?.GradeEntryDeadline,
                TotalAssignedSections = activeSections.Count,
                TotalStudentsTeaching = totalStudents,
                SectionsLockedCount = lockedCount,
                SectionsPendingGradeCount = pendingCount,
                TotalRegisteredSubjects = registeredSubjectsCount,
                AverageTeachingScore = avgTeachingScore,
                GradeDistribution = distribution,
                AssignedSections = assignedSections
            };
        }

        private static string FormatSchedule(IEnumerable<project_PRN232.Models.Entities.CourseSectionSchedule> schedules)
        {
            if (schedules == null || !schedules.Any()) return "Chưa xếp lịch";
            var items = schedules.Select(s => {
                var dayStr = s.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Thứ 2",
                    DayOfWeek.Tuesday => "Thứ 3",
                    DayOfWeek.Wednesday => "Thứ 4",
                    DayOfWeek.Thursday => "Thứ 5",
                    DayOfWeek.Friday => "Thứ 6",
                    DayOfWeek.Saturday => "Thứ 7",
                    DayOfWeek.Sunday => "Chủ nhật",
                    _ => s.DayOfWeek.ToString()
                };
                return $"{dayStr} (Slot {s.Slot}, {s.Room})";
            });
            return string.Join("; ", items);
        }

        public async Task<List<StudentSemesterTranscriptDto>> GetStudentsGpaReportAsync(int? majorId = null, int? administrativeClassId = null)
        {
            var query = _db.Students.Include(s => s.User).Where(s => s.User!.IsActive).AsQueryable();

            if (majorId.HasValue) query = query.Where(s => s.MajorId == majorId.Value);
            if (administrativeClassId.HasValue) query = query.Where(s => s.AdministrativeClassId == administrativeClassId.Value);

            var students = await query.ToListAsync();
            var list = new List<StudentSemesterTranscriptDto>();

            foreach (var st in students)
            {
                var transcript = await _gradeService.GetStudentTranscriptAsync(st.Id);
                list.Add(transcript);
            }

            return list.OrderByDescending(t => t.CumulativeGpa).ToList();
        }

        public async Task<byte[]> ExportGpaReportCsvAsync(int? majorId = null, int? administrativeClassId = null)
        {
            var reports = await GetStudentsGpaReportAsync(majorId, administrativeClassId);

            var sb = new StringBuilder();
            // UTF-8 BOM để Excel hiển thị đúng tiếng Việt
            sb.Append('\uFEFF');
            sb.AppendLine("STT,Mã sinh viên,Họ và tên,Ngành học,Ngành hẹp,Kỳ hiện tại,Tín chỉ tích lũy,Tín chỉ đăng ký,GPA tích lũy,Xếp loại");

            int stt = 1;
            foreach (var r in reports)
            {
                sb.AppendLine($"{stt++},\"{r.StudentCode}\",\"{r.FullName}\",\"{r.MajorName}\",\"{r.SpecializationName ?? "Chưa chọn"}\",{r.CurrentSemester},{r.TotalPassedCredits},{r.TotalEnrolledCredits},{r.CumulativeGpa},\"{r.AcademicStanding}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportSectionGradesCsvAsync(int courseSectionId)
        {
            var sheet = await _gradeService.GetSectionGradeSheetAsync(courseSectionId, null, "Admin");

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine($"BẢNG ĐIỂM LỚP HỌC PHẦN: {sheet.CourseSectionCode} - MÔN: {sheet.SubjectCode} ({sheet.SubjectName})");
            sb.AppendLine();

            // Header columns
            var headers = new List<string> { "STT", "Mã sinh viên", "Họ và tên", "Lớp hành chính" };
            foreach (var comp in sheet.Components)
            {
                headers.Add($"\"{comp.Name} ({comp.Weight}%)\"");
            }
            headers.Add("Điểm tổng kết");
            headers.Add("Kết quả");

            sb.AppendLine(string.Join(",", headers));

            int stt = 1;
            foreach (var row in sheet.StudentRows)
            {
                var cells = new List<string>
                {
                    (stt++).ToString(),
                    $"\"{row.StudentCode}\"",
                    $"\"{row.FullName}\"",
                    $"\"{row.AdministrativeClassCode}\""
                };

                foreach (var comp in sheet.Components)
                {
                    row.ComponentScores.TryGetValue(comp.Id, out var score);
                    cells.Add(score.HasValue ? score.Value.ToString("F1") : "");
                }

                cells.Add(row.FinalScore.HasValue ? row.FinalScore.Value.ToString("F1") : "");
                cells.Add(row.FinalScore.HasValue ? (row.IsPassed ? "Đạt" : "Không đạt") : "Chưa đủ điểm");

                sb.AppendLine(string.Join(",", cells));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}

