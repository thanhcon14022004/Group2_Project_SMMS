using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly AppDbContext _db;

        public GradesController(IGradeService gradeService, AppDbContext db)
        {
            _gradeService = gradeService;
            _db = db;
        }

        [HttpGet("section/{sectionId}")]
        [HttpGet("course-section/{sectionId}")]
        public async Task<IActionResult> GetSectionGrades(int sectionId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            try
            {
                var sheet = await _gradeService.GetSectionGradeSheetAsync(sectionId, userId, userRole);
                return Ok(sheet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [HttpPost("section")]
        public async Task<IActionResult> SaveGrades([FromBody] UpdateStudentGradeRequest request)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);
            var username = User.Identity?.Name ?? "User";

            bool isAdmin = userRole == "Admin";
            int? lecturerId = null;

            if (userRole == "Lecturer")
            {
                var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.UserId == userId);
                if (lecturer == null) return Forbid();
                lecturerId = lecturer.Id;
            }
            else if (!isAdmin)
            {
                return Forbid();
            }

            try
            {
                var success = await _gradeService.SaveSectionGradesAsync(request, username, lecturerId, isAdmin);
                return Ok(new { message = "Lưu điểm thành công.", success });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Khóa / Mở khóa điểm: CHỈ ADMIN ĐƯỢC PHÉP
        [HttpPut("section/{sectionId}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetGradeLock(int sectionId, [FromQuery] bool isLocked)
        {
            var username = User.Identity?.Name ?? "Admin";
            var success = await _gradeService.SetGradeLockAsync(sectionId, isLocked, username);
            if (!success) return NotFound(new { message = "Không tìm thấy lớp học phần." });

            return Ok(new { message = $"Đã {(isLocked ? "khóa" : "mở khóa")} điểm thành công." });
        }

        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockSectionGrades(int id)
        {
            var username = User.Identity?.Name ?? "Admin";
            var success = await _gradeService.SetGradeLockAsync(id, true, username);
            if (!success) return NotFound(ApiResponse.ErrorResult("Không tìm thấy lớp học phần."));
            return Ok(ApiResponse.SuccessResult("Đã khóa điểm lớp học phần thành công."));
        }

        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockSectionGrades(int id)
        {
            var username = User.Identity?.Name ?? "Admin";
            var success = await _gradeService.SetGradeLockAsync(id, false, username);
            if (!success) return NotFound(ApiResponse.ErrorResult("Không tìm thấy lớp học phần."));
            return Ok(ApiResponse.SuccessResult("Đã mở khóa điểm lớp học phần thành công."));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentGrades(int studentId)
        {
            return await GetTranscript(studentId);
        }

        // Xem bảng điểm và GPA tích lũy
        [HttpGet("transcript")]
        public async Task<IActionResult> GetTranscript([FromQuery] int? studentId)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            int targetStudentId;

            if (userRole == "Student")
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return NotFound(new { message = "Hồ sơ sinh viên không tồn tại." });
                targetStudentId = student.Id;
            }
            else
            {
                if (!studentId.HasValue) return BadRequest(new { message = "Vui lòng truyền mã ID sinh viên cần xem." });
                targetStudentId = studentId.Value;
            }

            try
            {
                var transcript = await _gradeService.GetStudentTranscriptAsync(targetStudentId);
                return Ok(transcript);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("section/{sectionId}/paged")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> GetSectionGradesPaged(
            int sectionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            try
            {
                var sheet = await _gradeService.GetSectionGradeSheetAsync(sectionId, userId, userRole);
                var rows = sheet.StudentRows ?? new List<StudentGradeRowDto>();
                int totalStudents = rows.Count;
                var gradedRows = rows.Where(r => r.FinalScore.HasValue).ToList();
                int gradedStudents = gradedRows.Count;
                int passedStudents = rows.Count(r => r.IsPassed);
                int failedStudents = gradedStudents - passedStudents;

                var report = new SectionGradeReportPagedDto
                {
                    CourseSectionId = sheet.CourseSectionId,
                    CourseSectionCode = sheet.CourseSectionCode,
                    SubjectCode = sheet.SubjectCode,
                    SubjectName = sheet.SubjectName,
                    Credits = sheet.Credits,
                    LecturerName = sheet.LecturerName,
                    SemesterCode = sheet.SemesterCode,
                    IsGradeLocked = sheet.IsGradeLocked,
                    CanEdit = sheet.CanEdit,
                    TotalStudents = totalStudents,
                    GradedStudents = gradedStudents,
                    PassedStudents = passedStudents,
                    FailedStudents = failedStudents,
                    PassRate = gradedStudents > 0 ? Math.Round((double)passedStudents * 100.0 / gradedStudents, 1) : 0,
                    FailRate = gradedStudents > 0 ? Math.Round((double)failedStudents * 100.0 / gradedStudents, 1) : 0,
                    AverageScore = gradedStudents > 0 ? Math.Round(gradedRows.Average(r => r.FinalScore!.Value), 2) : 0m,
                    MaxScore = gradedStudents > 0 ? gradedRows.Max(r => r.FinalScore!.Value) : 0m,
                    MinScore = gradedStudents > 0 ? gradedRows.Min(r => r.FinalScore!.Value) : 0m,
                    CountXuatSac = gradedRows.Count(r => r.FinalScore >= 9.0m),
                    CountGioi = gradedRows.Count(r => r.FinalScore >= 8.0m && r.FinalScore < 9.0m),
                    CountKha = gradedRows.Count(r => r.FinalScore >= 6.5m && r.FinalScore < 8.0m),
                    CountTrungBinh = gradedRows.Count(r => r.FinalScore >= 5.0m && r.FinalScore < 6.5m),
                    CountTruot = gradedRows.Count(r => r.FinalScore < 5.0m),
                    Components = sheet.Components,
                    PagedStudentRows = PaginationHelper.Paginate(rows, page, pageSize)
                };

                return Ok(ApiResponse<SectionGradeReportPagedDto>.Ok(report));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
        }

        [HttpGet("transcript/paged")]
        public async Task<IActionResult> GetTranscriptPaged(
            [FromQuery] int? studentId,
            [FromQuery] string? semesterCode,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            int targetStudentId;
            if (userRole == "Student")
            {
                var student = await _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return NotFound(ApiResponse.ErrorResult("Hồ sơ sinh viên không tồn tại."));
                targetStudentId = student.Id;
            }
            else
            {
                if (!studentId.HasValue) return BadRequest(ApiResponse.ErrorResult("Vui lòng truyền mã ID sinh viên cần xem."));
                targetStudentId = studentId.Value;
            }

            try
            {
                var transcript = await _gradeService.GetStudentTranscriptAsync(targetStudentId);
                var records = transcript.SubjectRecords ?? new List<StudentSubjectTranscriptDto>();
                var availableSemesters = records
                    .Select(r => r.SemesterCode)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToList();

                IEnumerable<StudentSubjectTranscriptDto> filtered = records;
                if (!string.IsNullOrWhiteSpace(semesterCode))
                {
                    var semTrim = semesterCode.Trim();
                    filtered = filtered.Where(r => string.Equals(r.SemesterCode, semTrim, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.Trim().ToLower();
                    filtered = filtered.Where(r =>
                        r.SubjectCode.ToLower().Contains(q) ||
                        r.SubjectName.ToLower().Contains(q));
                }

                var dto = new StudentTranscriptPagedDto
                {
                    StudentId = transcript.StudentId,
                    StudentCode = transcript.StudentCode,
                    FullName = transcript.FullName,
                    MajorName = transcript.MajorName,
                    SpecializationName = transcript.SpecializationName,
                    CurrentSemester = transcript.CurrentSemester,
                    CumulativeGpa = transcript.CumulativeGpa,
                    TotalPassedCredits = transcript.TotalPassedCredits,
                    TotalEnrolledCredits = transcript.TotalEnrolledCredits,
                    AcademicStanding = transcript.AcademicStanding,
                    AvailableSemesters = availableSemesters,
                    PagedSubjects = PaginationHelper.Paginate(filtered, page, pageSize)
                };

                return Ok(ApiResponse<StudentTranscriptPagedDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
        }
    }
}

