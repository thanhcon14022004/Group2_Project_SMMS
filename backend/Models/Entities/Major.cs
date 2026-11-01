using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class Major
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // SE, IA, GD
        public string Name { get; set; } = string.Empty; // Kỹ thuật phần mềm
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
        [JsonIgnore]
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        [JsonIgnore]
        public virtual ICollection<AdministrativeClass> AdministrativeClasses { get; set; } = new List<AdministrativeClass>();
        [JsonIgnore]
        public virtual ICollection<CurriculumConfig> CurriculumConfigs { get; set; } = new List<CurriculumConfig>();
    }

    public class Specialization
    {
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string Code { get; set; } = string.Empty; // SE-IS, SE-JS
        public string Name { get; set; } = string.Empty; // Hệ thống thông tin
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Major? Major { get; set; }
        [JsonIgnore]
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        [JsonIgnore]
        public virtual ICollection<CurriculumConfig> CurriculumConfigs { get; set; } = new List<CurriculumConfig>();
    }
}

