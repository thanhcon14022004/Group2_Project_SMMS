using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class Semester
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // FA24, SP25, SU25
        public string Name { get; set; } = string.Empty; // Fall 2024
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime TransferClassDeadline { get; set; } // Hạn chuyển lớp hành chính
        public DateTime GradeEntryDeadline { get; set; } // Hạn nhập điểm của giảng viên
        public string Status { get; set; } = "Upcoming"; // Upcoming, Active, Ended
        public bool IsClosedWithExceptions { get; set; } = false;
        public string? CloseReason { get; set; }

        [JsonIgnore]
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
        [JsonIgnore]
        public virtual ICollection<ClassTransferRequest> TransferRequests { get; set; } = new List<ClassTransferRequest>();
    }

    public class CourseSection
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // PRN232_FA24_SE1701
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public int SemesterId { get; set; }
        public int? LecturerId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public int MaxCapacity { get; set; } = 30;
        public bool IsGradeLocked { get; set; } = false;
        public DateTime? GradeLockedAt { get; set; }
        public string? GradeLockedBy { get; set; }
        public string Status { get; set; } = "Open"; // Open, Closed, Completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Subject? Subject { get; set; }
        public virtual Semester? Semester { get; set; }
        public virtual Lecturer? Lecturer { get; set; }
        public virtual AdministrativeClass? AdministrativeClass { get; set; }

        public virtual ICollection<CourseSectionSchedule> Schedules { get; set; } = new List<CourseSectionSchedule>();
        public virtual ICollection<CourseSectionStudent> Enrollments { get; set; } = new List<CourseSectionStudent>();
        public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
    }

    public class CourseSectionSchedule
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // Monday, Tuesday, ...
        public int Slot { get; set; } // 1 (07:30-09:00), 2 (09:15-10:45), 3 (11:00-12:30), 4 (13:00-14:30), 5 (14:45-16:15), 6 (16:30-18:00)
        public string Room { get; set; } = "BE-301";

        [JsonIgnore]
        public virtual CourseSection? CourseSection { get; set; }
    }

    public class CourseSectionStudent
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public int StudentId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Enrolled"; // Enrolled, Dropped, Completed

        [JsonIgnore]
        public virtual CourseSection? CourseSection { get; set; }
        public virtual Student? Student { get; set; }
    }
}

