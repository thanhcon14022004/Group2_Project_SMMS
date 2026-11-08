using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class AdministrativeClass
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // SE1701, SE1702
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public int Cohort { get; set; } = 17; // Khóa 17
        public string AcademicYear { get; set; } = "2023-2027";
        public int? SpecializationId { get; set; }
        public int CurrentSemester { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public virtual Major? Major { get; set; }
        public virtual Specialization? Specialization { get; set; }

        [JsonIgnore]
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        [JsonIgnore]
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }

    public class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty; // SE170001
        public int UserId { get; set; }
        public int MajorId { get; set; }
        public int? SpecializationId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public int Cohort { get; set; } = 17;
        public int CurrentSemester { get; set; } = 1;
        public string Status { get; set; } = "Studying"; // Studying, Suspended, Graduated

        public virtual User? User { get; set; }
        public virtual Major? Major { get; set; }
        public virtual Specialization? Specialization { get; set; }
        public virtual AdministrativeClass? AdministrativeClass { get; set; }

        [JsonIgnore]
        public virtual ICollection<CourseSectionStudent> CourseSectionEnrollments { get; set; } = new List<CourseSectionStudent>();
        [JsonIgnore]
        public virtual ICollection<StudentGrade> Grades { get; set; } = new List<StudentGrade>();
        [JsonIgnore]
        public virtual ICollection<ClassTransferRequest> TransferRequests { get; set; } = new List<ClassTransferRequest>();
        [JsonIgnore]
        public virtual ICollection<SpecializationSelection> SpecializationSelections { get; set; } = new List<SpecializationSelection>();
    }

    public class Lecturer
    {
        public int Id { get; set; }
        public string LecturerCode { get; set; } = string.Empty; // GV001
        public int UserId { get; set; }
        public string Department { get; set; } = "Software Engineering";
        public string AcademicTitle { get; set; } = "Master"; // Bachelor, Master, PhD, Associate Professor

        public virtual User? User { get; set; }

        [JsonIgnore]
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

        [JsonIgnore]
        public virtual ICollection<LecturerSubject> LecturerSubjects { get; set; } = new List<LecturerSubject>();
    }

    public class LecturerSubject
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public int SubjectId { get; set; }
        public int? MajorId { get; set; }
        public int? SpecializationId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public virtual Lecturer? Lecturer { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual Major? Major { get; set; }
        public virtual Specialization? Specialization { get; set; }
    }
}

