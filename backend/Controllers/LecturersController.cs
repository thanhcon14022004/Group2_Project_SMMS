using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LecturersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;

        public LecturersController(AppDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLecturers([FromQuery] string? department, [FromQuery] string? search)
        {
            var query = _db.Lecturers
                .Include(l => l.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(l => l.Department == department);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l => l.LecturerCode.ToLower().Contains(term) ||
                                         (l.User != null && l.User.FullName.ToLower().Contains(term)));
            }

            var list = await query.Select(l => new LecturerDetailDto
            {
                Id = l.Id,
                LecturerCode = l.LecturerCode,
                UserId = l.UserId,
                FullName = l.User != null ? l.User.FullName : "",
                Email = l.User != null ? l.User.Email : "",
                PhoneNumber = l.User != null ? l.User.PhoneNumber : "",
                Department = l.Department,
                AcademicTitle = l.AcademicTitle,
                IsActive = l.User != null && l.User.IsActive
            }).ToListAsync();

            return Ok(ApiResponse<List<LecturerDetailDto>>.Ok(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLecturerById(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var l = await _db.Lecturers
                .Include(l => l.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (l == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            if (userRole == "Lecturer" && l.UserId != userId)
            {
                return Forbid();
            }

            var dto = new LecturerDetailDto
            {
                Id = l.Id,
                LecturerCode = l.LecturerCode,
                UserId = l.UserId,
                FullName = l.User != null ? l.User.FullName : "",
                Email = l.User != null ? l.User.Email : "",
                PhoneNumber = l.User != null ? l.User.PhoneNumber : "",
                Department = l.Department,
                AcademicTitle = l.AcademicTitle,
                IsActive = l.User != null && l.User.IsActive
            };

            return Ok(ApiResponse<LecturerDetailDto>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLecturer([FromBody] CreateLecturerRequest request)
        {
            if (await _db.Lecturers.AnyAsync(l => l.LecturerCode == request.LecturerCode))
            {
                return BadRequest(ApiResponse.ErrorResult($"Mã giảng viên '{request.LecturerCode}' đã tồn tại."));
            }

            var performedBy = User.Identity?.Name ?? "Admin";

            var user = new User
            {
                Username = request.LecturerCode.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Lecturer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var lecturer = new Lecturer
            {
                LecturerCode = request.LecturerCode,
                UserId = user.Id,
                Department = request.Department ?? "Software Engineering",
                AcademicTitle = request.AcademicTitle ?? "Master"
            };
            _db.Lecturers.Add(lecturer);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("LECTURER_CREATED", "Lecturer", lecturer.Id.ToString(), performedBy,
                $"Tạo mới giảng viên '{lecturer.LecturerCode}' - {user.FullName}.");

            return CreatedAtAction(nameof(GetLecturerById), new { id = lecturer.Id }, ApiResponse<Lecturer>.Ok(lecturer, "Tạo giảng viên thành công."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLecturer(int id, [FromBody] UpdateLecturerRequest request)
        {
            var lecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
            if (lecturer == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            var performedBy = User.Identity?.Name ?? "Admin";

            if (lecturer.User != null)
            {
                if (!string.IsNullOrWhiteSpace(request.FullName)) lecturer.User.FullName = request.FullName;
                if (!string.IsNullOrWhiteSpace(request.Email)) lecturer.User.Email = request.Email;
                if (request.PhoneNumber != null) lecturer.User.PhoneNumber = request.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(request.Department)) lecturer.Department = request.Department;
            if (!string.IsNullOrWhiteSpace(request.AcademicTitle)) lecturer.AcademicTitle = request.AcademicTitle;

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync("LECTURER_UPDATED", "Lecturer", lecturer.Id.ToString(), performedBy,
                $"Cập nhật thông tin giảng viên '{lecturer.LecturerCode}'.");

            return Ok(ApiResponse.SuccessResult("Cập nhật thông tin giảng viên thành công."));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleLecturerStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var lecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
            if (lecturer == null || lecturer.User == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            lecturer.User.IsActive = request.IsActive;
            await _db.SaveChangesAsync();

            var performedBy = User.Identity?.Name ?? "Admin";
            await _auditLog.LogAsync("LECTURER_STATUS_CHANGED", "Lecturer", lecturer.Id.ToString(), performedBy,
                $"{(request.IsActive ? "Kích hoạt" : "Vô hiệu hóa")} giảng viên '{lecturer.LecturerCode}'.");

            return Ok(ApiResponse.SuccessResult($"Đã {(request.IsActive ? "kích hoạt" : "vô hiệu hóa")} giảng viên thành công."));
        }
    }

    public class CreateLecturerRequest
    {
        public string LecturerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Department { get; set; } = "Software Engineering";
        public string AcademicTitle { get; set; } = "Master";
    }

    public class UpdateLecturerRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? AcademicTitle { get; set; }
    }

    public class LecturerDetailDto
    {
        public int Id { get; set; }
        public string LecturerCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Department { get; set; } = string.Empty;
        public string AcademicTitle { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

