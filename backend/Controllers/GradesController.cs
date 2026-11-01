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
    }
}

