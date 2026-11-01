using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // PRF192, PRN211, etc.
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; } = 3;
        public string? Description { get; set; }
        public int? MajorId { get; set; }
        public int? SpecializationId { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Major? Major { get; set; }
        public virtual Specialization? Specialization { get; set; }

        public virtual ICollection<SubjectGradeComponent> GradeComponents { get; set; } = new List<SubjectGradeComponent>();
        
        [JsonIgnore]
        public virtual ICollection<PrerequisiteSubject> PrerequisiteRequirements { get; set; } = new List<PrerequisiteSubject>(); // Subject has prerequisites
        [JsonIgnore]
        public virtual ICollection<PrerequisiteSubject> PrerequisiteOf { get; set; } = new List<PrerequisiteSubject>(); // Subject is prerequisite of other
        [JsonIgnore]
        public virtual ICollection<CurriculumSubject> CurriculumSubjects { get; set; } = new List<CurriculumSubject>();
        [JsonIgnore]
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }

    public class SubjectGradeComponent
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty; // Lab 1, Assignment, Final Exam...
        public decimal Weight { get; set; } // e.g. 10.0, 30.0, 40.0 (%)
        public decimal Scale { get; set; } = 10.0m; // Default 10.0
        public bool IsAdminOnly { get; set; } = false; // Only Admin can edit this grade if true
        public int OrderIndex { get; set; } = 0;

        [JsonIgnore]
        public virtual Subject? Subject { get; set; }
        [JsonIgnore]
        public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
    }

    public class PrerequisiteSubject
    {
        public int Id { get; set; }
        public int SubjectId { get; set; } // Môn cần điều kiện
        public int PrerequisiteSubjectId { get; set; } // Môn tiên quyết
        public decimal MinScore { get; set; } = 5.0m; // Điểm tối thiểu để đạt

        public virtual Subject? Subject { get; set; }
        public virtual Subject? Prerequisite { get; set; }
    }
}

