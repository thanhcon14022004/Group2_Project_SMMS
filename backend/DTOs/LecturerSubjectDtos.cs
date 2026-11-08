using System.ComponentModel.DataAnnotations;

namespace project_PRN232.DTOs
{
    public class LecturerSubjectDto
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public string LecturerCode { get; set; } = string.Empty;
        public string LecturerName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? MajorId { get; set; }
        public string? MajorCode { get; set; }
        public string? MajorName { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateLecturerSubjectRequest
    {
        [Required(ErrorMessage = "Ngành học là bắt buộc.")]
        public int MajorId { get; set; }

        [Required(ErrorMessage = "Ngành hẹp là bắt buộc.")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Môn học là bắt buộc.")]
        public int SubjectId { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateLecturerSubjectRequest
    {
        [Required(ErrorMessage = "Ngành học là bắt buộc.")]
        public int MajorId { get; set; }

        [Required(ErrorMessage = "Ngành hẹp là bắt buộc.")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Môn học là bắt buộc.")]
        public int SubjectId { get; set; }

        public string? Notes { get; set; }
    }
}

