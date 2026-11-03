using System.Text;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;

namespace project_PRN232.Services
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<StudentSemesterTranscriptDto>> GetStudentsGpaReportAsync(int? majorId = null, int? administrativeClassId = null);
        Task<byte[]> ExportGpaReportCsvAsync(int? majorId = null, int? administrativeClassId = null);
        Task<byte[]> ExportGpaReportWordAsync(int? majorId = null, int? administrativeClassId = null);
        Task<byte[]> ExportSectionGradesCsvAsync(int courseSectionId);
        Task<byte[]> ExportSectionGradesWordAsync(int courseSectionId);
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

        public async Task<byte[]> ExportGpaReportWordAsync(int? majorId = null, int? administrativeClassId = null)
        {
            var reports = await GetStudentsGpaReportAsync(majorId, administrativeClassId);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head><meta charset='utf-8'>");
            sb.AppendLine("<title>Báo cáo GPA</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 11pt; margin: 20px; }");
            sb.AppendLine(".header-title { text-align: center; font-size: 14pt; font-weight: bold; color: #0d6efd; text-transform: uppercase; margin-bottom: 4px; }");
            sb.AppendLine(".header-sub { text-align: center; font-size: 11pt; font-style: italic; color: #555555; margin-top: 0; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; border: 1.5px solid #000000; }");
            sb.AppendLine("th, td { border: 1px solid #000000; padding: 6px 8px; font-size: 10pt; vertical-align: middle; }");
            sb.AppendLine("th { background-color: #e9ecef; font-weight: bold; text-align: center; }");
            sb.AppendLine(".center { text-align: center; }");
            sb.AppendLine(".right { text-align: right; }");
            sb.AppendLine(".bold { font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header-title'>BÁO CÁO TỔNG HỢP KẾT QUẢ HỌC TẬP & ĐIỂM GPA</div>");
            sb.AppendLine($"<div class='header-sub'>Hệ thống Quản lý Điểm SMMS &bull; Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} &bull; Tổng số: {reports.Count} sinh viên</div>");

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th style='width: 35px;'>STT</th>");
            sb.AppendLine("<th style='width: 80px;'>Mã SV</th>");
            sb.AppendLine("<th>Họ và tên</th>");
            sb.AppendLine("<th>Ngành đào tạo</th>");
            sb.AppendLine("<th>Chuyên ngành hẹp</th>");
            sb.AppendLine("<th style='width: 45px;'>Kỳ</th>");
            sb.AppendLine("<th style='width: 55px;'>TC Đạt</th>");
            sb.AppendLine("<th style='width: 55px;'>TC ĐK</th>");
            sb.AppendLine("<th style='width: 55px;'>GPA</th>");
            sb.AppendLine("<th style='width: 85px;'>Xếp loại</th>");
            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            int stt = 1;
            foreach (var r in reports)
            {
                var spec = string.IsNullOrEmpty(r.SpecializationName) ? "--" : r.SpecializationName;
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='center'>{stt++}</td>");
                sb.AppendLine($"<td class='center bold'>{r.StudentCode}</td>");
                sb.AppendLine($"<td>{r.FullName}</td>");
                sb.AppendLine($"<td>{r.MajorName}</td>");
                sb.AppendLine($"<td>{spec}</td>");
                sb.AppendLine($"<td class='center'>{r.CurrentSemester}</td>");
                sb.AppendLine($"<td class='center bold' style='color: green;'>{r.TotalPassedCredits}</td>");
                sb.AppendLine($"<td class='center'>{r.TotalEnrolledCredits}</td>");
                sb.AppendLine($"<td class='center bold' style='color: #0d6efd;'>{r.CumulativeGpa:0.00}</td>");
                sb.AppendLine($"<td class='center'>{r.AcademicStanding}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</body></html>");

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

        public async Task<byte[]> ExportSectionGradesWordAsync(int courseSectionId)
        {
            var sheet = await _gradeService.GetSectionGradeSheetAsync(courseSectionId, null, "Admin");

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head><meta charset='utf-8'>");
            sb.AppendLine("<title>Bảng điểm lớp học phần</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 11pt; margin: 20px; }");
            sb.AppendLine(".header-title { text-align: center; font-size: 14pt; font-weight: bold; color: #0d6efd; text-transform: uppercase; margin-bottom: 4px; }");
            sb.AppendLine(".header-sub { text-align: center; font-size: 11pt; font-style: italic; color: #555555; margin-top: 0; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; border: 1.5px solid #000000; }");
            sb.AppendLine("th, td { border: 1px solid #000000; padding: 6px 8px; font-size: 10pt; vertical-align: middle; }");
            sb.AppendLine("th { background-color: #e9ecef; font-weight: bold; text-align: center; }");
            sb.AppendLine(".center { text-align: center; }");
            sb.AppendLine(".bold { font-weight: bold; }");
            sb.AppendLine(".passed { color: #198754; font-weight: bold; }");
            sb.AppendLine(".failed { color: #dc3545; font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<div class='header-title'>BẢNG ĐIỂM LỚP HỌC PHẦN: {sheet.CourseSectionCode}</div>");
            sb.AppendLine($"<div class='header-sub'>Môn học: <strong>{sheet.SubjectCode} - {sheet.SubjectName}</strong> ({sheet.Credits} tín chỉ) &bull; Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th style='width: 35px;'>STT</th>");
            sb.AppendLine("<th style='width: 85px;'>MSSV</th>");
            sb.AppendLine("<th>Họ và tên</th>");
            sb.AppendLine("<th style='width: 90px;'>Lớp HC</th>");

            foreach (var comp in sheet.Components)
            {
                sb.AppendLine($"<th style='width: 75px;'>{comp.Name}<br/><span style='font-size: 8pt; font-weight: normal;'>({comp.Weight:0.#}%)</span></th>");
            }

            sb.AppendLine("<th style='width: 70px;'>Tổng kết</th>");
            sb.AppendLine("<th style='width: 80px;'>Kết quả</th>");
            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            int stt = 1;
            foreach (var row in sheet.StudentRows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='center'>{stt++}</td>");
                sb.AppendLine($"<td class='center bold'>{row.StudentCode}</td>");
                sb.AppendLine($"<td>{row.FullName}</td>");
                sb.AppendLine($"<td class='center'>{row.AdministrativeClassCode}</td>");

                foreach (var comp in sheet.Components)
                {
                    row.ComponentScores.TryGetValue(comp.Id, out var score);
                    var scoreText = score.HasValue ? score.Value.ToString("0.0") : "-";
                    sb.AppendLine($"<td class='center'>{scoreText}</td>");
                }

                var finalScoreText = row.FinalScore.HasValue ? row.FinalScore.Value.ToString("0.0") : "--";
                sb.AppendLine($"<td class='center bold' style='color: #0d6efd;'>{finalScoreText}</td>");

                if (row.FinalScore.HasValue)
                {
                    var resClass = row.IsPassed ? "passed" : "failed";
                    var resText = row.IsPassed ? "ĐẠT" : "HỎNG";
                    sb.AppendLine($"<td class='center {resClass}'>{resText}</td>");
                }
                else
                {
                    sb.AppendLine("<td class='center' style='color: #6c757d;'>Đang học</td>");
                }

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}

