namespace project_PRN232.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? StudentId { get; set; }
        public int? LecturerId { get; set; }
        public string? StudentCode { get; set; }
        public string? LecturerCode { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? LecturerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "Student"; // Admin, Lecturer, Student
        public string? Code { get; set; } // StudentCode or LecturerCode
        public int? MajorId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public int? Cohort { get; set; } = 17;
        public string? Department { get; set; }
        public string? AcademicTitle { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

