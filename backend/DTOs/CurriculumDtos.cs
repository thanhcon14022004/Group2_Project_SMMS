namespace project_PRN232.DTOs
{
    public class CurriculumLookupRequest
    {
        public int MajorId { get; set; }
        public int SemesterNumber { get; set; }
        public int? SpecializationId { get; set; }
        public string? Version { get; set; }
    }

    public class CurriculumLookupResponse
    {
        public bool IsFound { get; set; }
        public string? Message { get; set; }
        public int? CurriculumConfigId { get; set; }
        public string? ConfigName { get; set; }
        public string? Version { get; set; }
        public string? MajorCode { get; set; }
        public int SemesterNumber { get; set; }
        public string? SpecializationCode { get; set; }
        public List<CurriculumSubjectDetailDto> Subjects { get; set; } = new();
    }

    public class CurriculumSubjectDetailDto
    {
        public int SubjectId { get; set; }
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public bool IsCompulsory { get; set; }
        public List<SubjectGradeComponentDto> GradeComponents { get; set; } = new();
        public List<PrerequisiteDto> Prerequisites { get; set; } = new();
    }

    public class CreateCurriculumConfigRequest
    {
        public string Version { get; set; } = "2024.1";
        public int? MajorId { get; set; } // Nullable nếu dùng chung cho tất cả các ngành (Kỳ 6 OJT)
        public int SemesterNumber { get; set; }
        public int? SpecializationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> SubjectIds { get; set; } = new();
    }

    public class UpdateCurriculumConfigRequest
    {
        public string Version { get; set; } = "2024.1";
        public int? MajorId { get; set; } // Nullable nếu dùng chung cho tất cả các ngành (Kỳ 6 OJT)
        public int SemesterNumber { get; set; }
        public int? SpecializationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> SubjectIds { get; set; } = new();
    }

    public class CurriculumConfigDto
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public int? MajorId { get; set; }
        public string? MajorCode { get; set; }
        public int SemesterNumber { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<CurriculumSubjectDetailDto> Subjects { get; set; } = new();
    }
}

