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
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IGradeService _gradeService;
        private readonly IAuditLogService _auditLog;

        public StudentsController(AppDbContext db, IGradeService gradeService, IAuditLogService auditLog)
        {
            _db = db;
            _gradeService = gradeService;
            _auditLog = auditLog;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudents([FromQuery] int? majorId, [FromQuery] int? classId, [FromQuery] string? search)
        {
            var list = await BuildStudentDetailQuery(majorId, classId, search).ToListAsync();
            return Ok(ApiResponse<List<StudentDetailDto>>.Ok(list));
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentsPaged(
            [FromQuery] int? majorId,
            [FromQuery] int? classId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = BuildStudentDetailQuery(majorId, classId, search);
            var paged = await PaginationHelper.PaginateAsync(query, page, pageSize);
            return Ok(ApiResponse<PagedResult<StudentDetailDto>>.Ok(paged));
        }

        private IQueryable<StudentDetailDto> BuildStudentDetailQuery(int? majorId, int? classId, string? search)
        {
            var query = _db.Students
                .Include(s => s.User)
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                .Include(s => s.AdministrativeClass)
                .AsNoTracking()
                .AsQueryable();

            if (majorId.HasValue && majorId.Value > 0) query = query.Where(s => s.MajorId == majorId.Value);
            if (classId.HasValue && classId.Value > 0) query = query.Where(s => s.AdministrativeClassId == classId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var sTerm = search.Trim().ToLower();
                query = query.Where(s => s.StudentCode.ToLower().Contains(sTerm) ||
                                         (s.User != null && s.User.FullName.ToLower().Contains(sTerm)));
            }

            return query
                .OrderBy(s => s.StudentCode)
                .Select(s => new StudentDetailDto
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    UserId = s.UserId,
                    FullName = s.User != null ? s.User.FullName : "",
                    Email = s.User != null ? s.User.Email : "",
                    PhoneNumber = s.User != null ? s.User.PhoneNumber : "",
                    MajorId = s.MajorId,
                    MajorCode = s.Major != null ? s.Major.Code : "",
                    MajorName = s.Major != null ? s.Major.Name : "",
                    SpecializationId = s.SpecializationId,
                    SpecializationCode = s.Specialization != null ? s.Specialization.Code : null,
                    SpecializationName = s.Specialization != null ? s.Specialization.Name : null,
                    AdministrativeClassId = s.AdministrativeClassId,
                    AdministrativeClassName = s.AdministrativeClass != null ? s.AdministrativeClass.Name : null,
                    Cohort = s.Cohort,
                    CurrentSemester = s.CurrentSemester,
                    Status = s.Status,
                    IsActive = s.User != null && s.User.IsActive
                });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var s = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                .Include(s => s.AdministrativeClass)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (s == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            // Sinh viên chỉ được xem hồ sơ của chính mình
            if (userRole == "Student" && s.UserId != userId)
            {
                return Forbid();
            }

            var dto = new StudentDetailDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                UserId = s.UserId,
                FullName = s.User != null ? s.User.FullName : "",
                Email = s.User != null ? s.User.Email : "",
                PhoneNumber = s.User != null ? s.User.PhoneNumber : "",
                MajorId = s.MajorId,
                MajorCode = s.Major != null ? s.Major.Code : "",
                MajorName = s.Major != null ? s.Major.Name : "",
                SpecializationId = s.SpecializationId,
                SpecializationCode = s.Specialization != null ? s.Specialization.Code : null,
                SpecializationName = s.Specialization != null ? s.Specialization.Name : null,
                AdministrativeClassId = s.AdministrativeClassId,
                AdministrativeClassName = s.AdministrativeClass != null ? s.AdministrativeClass.Name : null,
                Cohort = s.Cohort,
                CurrentSemester = s.CurrentSemester,
                Status = s.Status,
                IsActive = s.User != null && s.User.IsActive
            };

            return Ok(ApiResponse<StudentDetailDto>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (await _db.Students.AnyAsync(s => s.StudentCode == request.StudentCode))
            {
                return BadRequest(ApiResponse.ErrorResult($"Mã sinh viên '{request.StudentCode}' đã tồn tại."));
            }

            var performedBy = User.Identity?.Name ?? "Admin";

            // Tạo User trước nếu cần
            var user = new User
            {
                Username = request.StudentCode.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var student = new Student
            {
                StudentCode = request.StudentCode,
                UserId = user.Id,
                MajorId = request.MajorId,
                SpecializationId = request.SpecializationId,
                AdministrativeClassId = request.AdministrativeClassId,
                Cohort = request.Cohort > 0 ? request.Cohort : 17,
                CurrentSemester = request.CurrentSemester > 0 ? request.CurrentSemester : 1,
                Status = "Studying"
            };
            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("STUDENT_CREATED", "Student", student.Id.ToString(), performedBy,
                $"Tạo sinh viên mới '{student.StudentCode}' - {user.FullName}.");

            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, ApiResponse<Student>.Ok(student, "Tạo sinh viên thành công."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            var student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            var performedBy = User.Identity?.Name ?? "Admin";

            if (student.User != null)
            {
                if (!string.IsNullOrWhiteSpace(request.FullName)) student.User.FullName = request.FullName;
                if (!string.IsNullOrWhiteSpace(request.Email)) student.User.Email = request.Email;
                if (request.PhoneNumber != null) student.User.PhoneNumber = request.PhoneNumber;
            }

            if (request.MajorId.HasValue) student.MajorId = request.MajorId.Value;
            student.SpecializationId = request.SpecializationId;
            student.AdministrativeClassId = request.AdministrativeClassId;
            if (request.CurrentSemester.HasValue) student.CurrentSemester = request.CurrentSemester.Value;
            if (!string.IsNullOrWhiteSpace(request.Status)) student.Status = request.Status;

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync("STUDENT_UPDATED", "Student", student.Id.ToString(), performedBy,
                $"Cập nhật hồ sơ sinh viên '{student.StudentCode}'.");

            return Ok(ApiResponse.SuccessResult("Cập nhật thông tin sinh viên thành công."));
        }

        [HttpGet("{id}/gpa")]
        public async Task<IActionResult> GetStudentGpa(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            if (userRole == "Student" && student.UserId != userId)
            {
                return Forbid();
            }

            var transcript = await _gradeService.GetStudentTranscriptAsync(id);

            return Ok(ApiResponse<object>.Ok(new
            {
                studentId = id,
                studentCode = transcript.StudentCode,
                fullName = transcript.FullName,
                gpa = transcript.CumulativeGpa,
                totalCredits = transcript.TotalEnrolledCredits,
                passedCredits = transcript.TotalPassedCredits,
                academicStanding = transcript.AcademicStanding
            }));
        }

        [HttpGet("{id}/transcript")]
        public async Task<IActionResult> GetStudentTranscript(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            if (userRole == "Student" && student.UserId != userId)
            {
                return Forbid();
            }

            var transcript = await _gradeService.GetStudentTranscriptAsync(id);
            return Ok(ApiResponse<StudentSemesterTranscriptDto>.Ok(transcript));
        }

        [HttpGet("{id}/specialization-options")]
        public async Task<IActionResult> GetSpecializationOptions(int id)
        {
            var student = await _db.Students.Include(s => s.Major).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            var specs = await _db.Specializations
                .Where(sp => sp.MajorId == student.MajorId && sp.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return Ok(ApiResponse<List<Specialization>>.Ok(specs));
        }

        [HttpPost("{id}/specialization")]
        public async Task<IActionResult> SelectSpecialization(int id, [FromBody] SelectSpecializationRequest request)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy sinh viên."));

            if (userRole == "Student" && student.UserId != userId)
            {
                return Forbid();
            }

            var spec = await _db.Specializations.FirstOrDefaultAsync(sp => sp.Id == request.SpecializationId && sp.MajorId == student.MajorId);
            if (spec == null)
            {
                return BadRequest(ApiResponse.ErrorResult("Chuyên ngành hẹp không thuộc ngành đào tạo của sinh viên."));
            }

            student.SpecializationId = request.SpecializationId;
            await _db.SaveChangesAsync();

            var performedBy = User.Identity?.Name ?? student.StudentCode;
            await _auditLog.LogAsync("SPECIALIZATION_SELECTED", "Student", student.Id.ToString(), performedBy,
                $"Sinh viên '{student.StudentCode}' chọn chuyên ngành '{spec.Name}' ({spec.Code}).");

            return Ok(ApiResponse.SuccessResult($"Đã lưu lựa chọn chuyên ngành '{spec.Name}' thành công."));
        }
    }

    public class CreateStudentRequest
    {
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int MajorId { get; set; }
        public int? SpecializationId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public int Cohort { get; set; } = 17;
        public int CurrentSemester { get; set; } = 1;
    }

    public class UpdateStudentRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? MajorId { get; set; }
        public int? SpecializationId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public int? CurrentSemester { get; set; }
        public string? Status { get; set; }
    }

    public class SelectSpecializationRequest
    {
        public int SpecializationId { get; set; }
    }

    public class StudentDetailDto
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public int? AdministrativeClassId { get; set; }
        public string? AdministrativeClassName { get; set; }
        public int Cohort { get; set; }
        public int CurrentSemester { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
