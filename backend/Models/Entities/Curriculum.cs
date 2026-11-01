using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class CurriculumConfig
    {
        public int Id { get; set; }
        public string Version { get; set; } = "2024.1"; // e.g. K17, 2024.1
        public int? MajorId { get; set; } // Nullable nếu là môn/chương trình dùng chung tất cả các ngành (vd: Kỳ 6 OJT)
        public int SemesterNumber { get; set; } // 1 to 9
        public int? SpecializationId { get; set; } // Bắt buộc cho Kỳ 5, 7, 8 nếu ngành có ngành hẹp; Kỳ 6 là kỳ thực tập OJT không yêu cầu ngành hẹp
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Major? Major { get; set; }
        public virtual Specialization? Specialization { get; set; }

        public virtual ICollection<CurriculumSubject> CurriculumSubjects { get; set; } = new List<CurriculumSubject>();
    }

    public class CurriculumSubject
    {
        public int Id { get; set; }
        public int CurriculumConfigId { get; set; }
        public int SubjectId { get; set; }
        public bool IsCompulsory { get; set; } = true;

        [JsonIgnore]
        public virtual CurriculumConfig? CurriculumConfig { get; set; }
        public virtual Subject? Subject { get; set; }
    }
}

