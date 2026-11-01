namespace project_PRN232.DTOs
{
    public class CourseSectionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int Credits { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterCode { get; set; }
        public int? LecturerId { get; set; }
        public string? LecturerCode { get; set; }
        public string? LecturerName { get; set; }
        public int MaxCapacity { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsGradeLocked { get; set; }
        public DateTime? GradeLockedAt { get; set; }
        public string? GradeLockedBy { get; set; }
        public string Status { get; set; } = "Open";
        public List<CourseSectionScheduleDto> Schedules { get; set; } = new();
    }

    public class CourseSectionScheduleDto
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int Slot { get; set; }
        public string Room { get; set; } = string.Empty;
    }

    public class CreateCourseSectionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public int SemesterId { get; set; }
        public int? LecturerId { get; set; }
        public int MaxCapacity { get; set; } = 30;
        public List<CourseSectionScheduleDto> Schedules { get; set; } = new();
    }

    public class AssignLecturerRequest
    {
        public int? LecturerId { get; set; }
    }

    public class BatchCreateSectionsFromCurriculumRequest
    {
        public int AdministrativeClassId { get; set; }
        public int SemesterId { get; set; }
        public string? Version { get; set; }
        public int DefaultMaxCapacity { get; set; } = 30;
    }
}

