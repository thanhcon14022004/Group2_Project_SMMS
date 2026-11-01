namespace project_PRN232.DTOs
{
    public class MajorDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<SpecializationDto> Specializations { get; set; } = new();
    }

    public class SpecializationDto
    {
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubjectDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public int? MajorId { get; set; }
        public string? MajorCode { get; set; }
        public string? MajorName { get; set; }
        public bool IsActive { get; set; }
        public List<SubjectGradeComponentDto> GradeComponents { get; set; } = new();
        public List<PrerequisiteDto> Prerequisites { get; set; } = new();
    }

    public class SubjectGradeComponentDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Scale { get; set; } = 10.0m;
        public bool IsAdminOnly { get; set; }
        public int OrderIndex { get; set; }
    }

    public class SaveSubjectComponentsRequest
    {
        public List<SubjectGradeComponentDto> Components { get; set; } = new();
    }

    public class PrerequisiteDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int PrerequisiteSubjectId { get; set; }
        public string? PrerequisiteSubjectCode { get; set; }
        public string? PrerequisiteSubjectName { get; set; }
        public decimal MinScore { get; set; } = 5.0m;
    }

    public class AdministrativeClassDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string? MajorCode { get; set; }
        public int Cohort { get; set; }
        public string AcademicYear { get; set; } = "2023-2027";
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public int CurrentSemester { get; set; }
        public int StudentCount { get; set; }
        public int CourseSectionsCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class SemesterDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime TransferClassDeadline { get; set; }
        public DateTime GradeEntryDeadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsClosedWithExceptions { get; set; }
        public int AdministrativeClassesCount { get; set; }
        public int CourseSectionsCount { get; set; }
    }

    public class SemesterClassesDto
    {
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int AdministrativeClassesCount { get; set; }
        public int CourseSectionsCount { get; set; }
        public int TotalStudentsCount { get; set; }
        public List<SemesterAdminClassDto> AdministrativeClasses { get; set; } = new();
        public List<SemesterCourseSectionDto> CourseSections { get; set; } = new();
    }

    public class SemesterAdminClassDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string? MajorCode { get; set; }
        public string? MajorName { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public int CurrentSemester { get; set; }
        public int StudentsCount { get; set; }
        public int CourseSectionsCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class SemesterCourseSectionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? AdministrativeClassId { get; set; }
        public string? AdministrativeClassCode { get; set; }
        public int? LecturerId { get; set; }
        public string? LecturerCode { get; set; }
        public string? LecturerName { get; set; }
        public int MaxCapacity { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsGradeLocked { get; set; }
        public string Status { get; set; } = "Open";
        public List<CourseSectionScheduleDto> Schedules { get; set; } = new();
    }
}

