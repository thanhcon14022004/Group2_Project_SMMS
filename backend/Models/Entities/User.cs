using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "Student"; // Admin, Lecturer, Student
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        [JsonIgnore]
        public virtual Student? Student { get; set; }
        [JsonIgnore]
        public virtual Lecturer? Lecturer { get; set; }
    }
}

