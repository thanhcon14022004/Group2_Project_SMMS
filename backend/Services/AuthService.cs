using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<UserDto> CreateUserAsync(CreateUserRequest request, string performedBy);
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive, string performedBy);
        Task<List<UserDto>> GetUsersAsync(string? role = null, string? search = null);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> UpdateUserAsync(int id, project_PRN232.Controllers.UpdateUserRequest request, string performedBy);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId, string performedBy);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IAuditLogService _auditLog;

        public AuthService(AppDbContext db, IConfiguration config, IAuditLogService auditLog)
        {
            _db = db;
            _config = config;
            _auditLog = auditLog;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users
                .Include(u => u.Student)
                .Include(u => u.Lecturer)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null) return null;

            // Kiểm tra mật khẩu (hỗ trợ hash BCrypt hoặc kiểm tra text cho demo seed nếu có)
            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            catch
            {
                // Fallback nếu chuỗi không phải bcrypt (chỉ dùng trong trường hợp dev thô ban đầu)
                isPasswordValid = (user.PasswordHash == request.Password);
            }

            if (!isPasswordValid) return null;

            if (!user.IsActive)
            {
                throw new InvalidOperationException("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ Quản trị viên.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Sinh JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SMMS_Super_Secret_Key_For_Jwt_Authentication_PRN232_Course_2026!");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email)
            };

            if (user.Student != null)
            {
                claims.Add(new Claim("StudentId", user.Student.Id.ToString()));
                claims.Add(new Claim("StudentCode", user.Student.StudentCode));
            }

            if (user.Lecturer != null)
            {
                claims.Add(new Claim("LecturerId", user.Lecturer.Id.ToString()));
                claims.Add(new Claim("LecturerCode", user.Lecturer.LecturerCode));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _config["Jwt:Issuer"] ?? "SMMS_Server",
                Audience = _config["Jwt:Audience"] ?? "SMMS_Client",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                UserId = user.Id,
                StudentId = user.Student?.Id,
                LecturerId = user.Lecturer?.Id,
                StudentCode = user.Student?.StudentCode,
                LecturerCode = user.Lecturer?.LecturerCode
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request, string performedBy)
        {
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException($"Tên đăng nhập '{request.Username}' đã tồn tại.");

            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException($"Email '{request.Email}' đã được sử dụng.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Nếu vai trò là Student hoặc Lecturer, tạo hồ sơ tương ứng
            if (request.Role == "Student")
            {
                var studentCode = request.Code ?? $"SE{DateTime.UtcNow:yy}{user.Id:D4}";
                var student = new Student
                {
                    UserId = user.Id,
                    StudentCode = studentCode,
                    MajorId = request.MajorId ?? 1,
                    AdministrativeClassId = request.AdministrativeClassId,
                    Cohort = request.Cohort ?? 17,
                    CurrentSemester = 1,
                    Status = "Studying"
                };
                _db.Students.Add(student);
            }
            else if (request.Role == "Lecturer")
            {
                var lecturerCode = request.Code ?? $"GV{user.Id:D3}";
                var lecturer = new Lecturer
                {
                    UserId = user.Id,
                    LecturerCode = lecturerCode,
                    Department = request.Department ?? "Software Engineering",
                    AcademicTitle = request.AcademicTitle ?? "Master"
                };
                _db.Lecturers.Add(lecturer);
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CREATE_USER", "User", user.Id.ToString(), performedBy,
                $"Tạo tài khoản {user.Username} với vai trò {user.Role}");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, string performedBy)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                isActive ? "ACTIVATE_USER" : "DEACTIVATE_USER",
                "User",
                userId.ToString(),
                performedBy,
                $"Cập nhật trạng thái tài khoản {user.Username}: Active={isActive}"
            );

            return true;
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId, string performedBy)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng.");

            var hasStudent = await _db.Students.AnyAsync(s => s.UserId == userId);
            var hasLecturer = await _db.Lecturers.AnyAsync(l => l.UserId == userId);
            if (hasStudent || hasLecturer)
            {
                user.IsActive = false;
                await _db.SaveChangesAsync();
                await _auditLog.LogAsync("DEACTIVATE_USER", "User", userId.ToString(), performedBy, $"Tài khoản {user.Username} đang có dữ liệu sinh viên/giảng viên, đã chuyển sang Inactive.");
                return (true, "Tài khoản đang liên kết hồ sơ Sinh viên/Giảng viên, đã chuyển trạng thái thành Vô hiệu hóa (Inactive).");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync("DELETE_USER", "User", userId.ToString(), performedBy, $"Đã xóa tài khoản {user.Username}.");
            return (true, "Đã xóa người dùng thành công.");
        }

        public async Task<List<UserDto>> GetUsersAsync(string? role = null, string? search = null)
        {
            var query = _db.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.FullName.Contains(search) || u.Email.Contains(search));
            }

            return await query.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                LecturerId = _db.Lecturers.Where(l => l.UserId == u.Id).Select(l => (int?)l.Id).FirstOrDefault(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            }).ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                LecturerId = await _db.Lecturers.Where(l => l.UserId == user.Id).Select(l => (int?)l.Id).FirstOrDefaultAsync(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<UserDto?> UpdateUserAsync(int id, project_PRN232.Controllers.UpdateUserRequest request, string performedBy)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync("USER_UPDATED", "User", user.Id.ToString(), performedBy, $"Cập nhật thông tin tài khoản '{user.Username}'.");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}

