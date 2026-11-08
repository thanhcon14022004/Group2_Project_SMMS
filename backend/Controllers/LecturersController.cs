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

        #region Môn đăng ký giảng dạy (Lecturer Subjects CRUD)

        [HttpGet("{id}/subjects")]
        public async Task<IActionResult> GetLecturerSubjects(int id)
        {
            var lecturer = await _db.Lecturers
                .Include(l => l.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id || l.UserId == id);

            if (lecturer == null)
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            var list = await _db.LecturerSubjects
                .Include(ls => ls.Subject)
                .Include(ls => ls.Major)
                .Include(ls => ls.Specialization)
                .Where(ls => ls.LecturerId == lecturer.Id)
                .OrderBy(ls => ls.Subject != null ? ls.Subject.Code : "")
                .Select(ls => new LecturerSubjectDto
                {
                    Id = ls.Id,
                    LecturerId = ls.LecturerId,
                    LecturerCode = lecturer.LecturerCode,
                    LecturerName = lecturer.User != null ? lecturer.User.FullName : "",
                    SubjectId = ls.SubjectId,
                    SubjectCode = ls.Subject != null ? ls.Subject.Code : "",
                    SubjectName = ls.Subject != null ? ls.Subject.Name : "",
                    Credits = ls.Subject != null ? ls.Subject.Credits : 0,
                    MajorId = ls.MajorId,
                    MajorCode = ls.Major != null ? ls.Major.Code : "",
                    MajorName = ls.Major != null ? ls.Major.Name : "",
                    SpecializationId = ls.SpecializationId,
                    SpecializationCode = ls.Specialization != null ? ls.Specialization.Code : "",
                    SpecializationName = ls.Specialization != null ? ls.Specialization.Name : "",
                    RegisteredAt = ls.RegisteredAt,
                    Notes = ls.Notes
                })
                .ToListAsync();

            return Ok(ApiResponse<List<LecturerSubjectDto>>.Ok(list));
        }

        [HttpPost("{id}/subjects")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLecturerSubject(int id, [FromBody] CreateLecturerSubjectRequest request)
        {
            if (request == null || request.MajorId <= 0 || request.SpecializationId <= 0 || request.SubjectId <= 0)
            {
                return BadRequest(ApiResponse.ErrorResult("Vui lòng chọn đầy đủ Ngành, Ngành hẹp và Môn học."));
            }

            var lecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id || l.UserId == id);
            if (lecturer == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId);
            if (subject == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy môn học đã chọn."));

            var major = await _db.Majors.FirstOrDefaultAsync(m => m.Id == request.MajorId);
            var spec = await _db.Specializations.FirstOrDefaultAsync(sp => sp.Id == request.SpecializationId);
            if (major == null || spec == null)
            {
                return BadRequest(ApiResponse.ErrorResult("Ngành hoặc Ngành hẹp không hợp lệ."));
            }

            // Kiểm tra trùng lặp môn học đối với giảng viên này
            var alreadyRegistered = await _db.LecturerSubjects
                .AnyAsync(ls => ls.LecturerId == lecturer.Id && ls.SubjectId == request.SubjectId);

            if (alreadyRegistered)
            {
                return BadRequest(ApiResponse.ErrorResult($"Giảng viên đã đăng ký môn '{subject.Code} - {subject.Name}' rồi."));
            }

            var reg = new LecturerSubject
            {
                LecturerId = lecturer.Id,
                SubjectId = request.SubjectId,
                MajorId = request.MajorId,
                SpecializationId = request.SpecializationId,
                RegisteredAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            _db.LecturerSubjects.Add(reg);
            await _db.SaveChangesAsync();

            var performedBy = User.Identity?.Name ?? "Admin";
            await _auditLog.LogAsync("LECTURER_SUBJECT_ADDED", "LecturerSubject", reg.Id.ToString(), performedBy,
                $"Đăng ký môn học '{subject.Code}' cho giảng viên '{lecturer.LecturerCode}'.");

            var dto = new LecturerSubjectDto
            {
                Id = reg.Id,
                LecturerId = lecturer.Id,
                LecturerCode = lecturer.LecturerCode,
                LecturerName = lecturer.User?.FullName ?? "",
                SubjectId = subject.Id,
                SubjectCode = subject.Code,
                SubjectName = subject.Name,
                Credits = subject.Credits,
                MajorId = major.Id,
                MajorCode = major.Code,
                MajorName = major.Name,
                SpecializationId = spec.Id,
                SpecializationCode = spec.Code,
                SpecializationName = spec.Name,
                RegisteredAt = reg.RegisteredAt,
                Notes = reg.Notes
            };

            return Ok(ApiResponse<LecturerSubjectDto>.Ok(dto, "Thêm môn đăng ký thành công."));
        }

        [HttpPut("{id}/subjects/{subjectRegId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLecturerSubject(int id, int subjectRegId, [FromBody] UpdateLecturerSubjectRequest request)
        {
            if (request == null || request.MajorId <= 0 || request.SpecializationId <= 0 || request.SubjectId <= 0)
            {
                return BadRequest(ApiResponse.ErrorResult("Vui lòng chọn đầy đủ Ngành, Ngành hẹp và Môn học."));
            }

            var lecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id || l.UserId == id);
            if (lecturer == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            var reg = await _db.LecturerSubjects.FirstOrDefaultAsync(ls => ls.Id == subjectRegId && ls.LecturerId == lecturer.Id);
            if (reg == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy bản ghi môn đăng ký."));

            var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId);
            if (subject == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy môn học đã chọn."));

            var major = await _db.Majors.FirstOrDefaultAsync(m => m.Id == request.MajorId);
            var spec = await _db.Specializations.FirstOrDefaultAsync(sp => sp.Id == request.SpecializationId);
            if (major == null || spec == null)
            {
                return BadRequest(ApiResponse.ErrorResult("Ngành hoặc Ngành hẹp không hợp lệ."));
            }

            // Kiểm tra trùng môn khác
            var duplicate = await _db.LecturerSubjects
                .AnyAsync(ls => ls.LecturerId == lecturer.Id && ls.SubjectId == request.SubjectId && ls.Id != subjectRegId);

            if (duplicate)
            {
                return BadRequest(ApiResponse.ErrorResult($"Giảng viên đã đăng ký môn '{subject.Code}' trong danh sách."));
            }

            reg.SubjectId = request.SubjectId;
            reg.MajorId = request.MajorId;
            reg.SpecializationId = request.SpecializationId;
            reg.Notes = request.Notes;

            await _db.SaveChangesAsync();

            var performedBy = User.Identity?.Name ?? "Admin";
            await _auditLog.LogAsync("LECTURER_SUBJECT_UPDATED", "LecturerSubject", reg.Id.ToString(), performedBy,
                $"Cập nhật môn học '{subject.Code}' cho giảng viên '{lecturer.LecturerCode}'.");

            return Ok(ApiResponse.SuccessResult("Cập nhật môn đăng ký thành công."));
        }

        [HttpDelete("{id}/subjects/{subjectRegId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLecturerSubject(int id, int subjectRegId)
        {
            var lecturer = await _db.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id || l.UserId == id);
            if (lecturer == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy giảng viên."));

            var reg = await _db.LecturerSubjects
                .Include(ls => ls.Subject)
                .FirstOrDefaultAsync(ls => ls.Id == subjectRegId && ls.LecturerId == lecturer.Id);

            if (reg == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy bản ghi môn đăng ký."));

            // Ràng buộc nghiệp vụ: Không cho xóa nếu giảng viên đang dạy lớp học phần của môn này trong học kỳ mở/hoạt động
            var isTeachingActiveSection = await _db.CourseSections
                .Include(cs => cs.Semester)
                .AnyAsync(cs => cs.LecturerId == lecturer.Id 
                             && cs.SubjectId == reg.SubjectId 
                             && (cs.Status == "Open" || (cs.Semester != null && cs.Semester.Status == "Active")));

            if (isTeachingActiveSection)
            {
                return BadRequest(ApiResponse.ErrorResult(
                    $"Không thể xóa môn '{reg.Subject?.Code ?? ""}'! Giảng viên đang được phân công giảng dạy lớp học phần môn này trong học kỳ hiện tại."));
            }

            _db.LecturerSubjects.Remove(reg);
            await _db.SaveChangesAsync();

            var performedBy = User.Identity?.Name ?? "Admin";
            await _auditLog.LogAsync("LECTURER_SUBJECT_DELETED", "LecturerSubject", subjectRegId.ToString(), performedBy,
                $"Xóa môn đăng ký '{reg.Subject?.Code ?? ""}' của giảng viên '{lecturer.LecturerCode}'.");

            return Ok(ApiResponse.SuccessResult("Đã xóa môn đăng ký thành công."));
        }

        #endregion
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

