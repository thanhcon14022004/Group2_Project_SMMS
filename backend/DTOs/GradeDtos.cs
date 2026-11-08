namespace project_PRN232.DTOs
{
    public class SectionGradeSheetDto
    {
        public int CourseSectionId { get; set; }
        public string CourseSectionCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? AdministrativeClassId { get; set; }
        public string? AdministrativeClassCode { get; set; }
        public string? LecturerName { get; set; }
        public string? SemesterCode { get; set; }
        public bool IsGradeLocked { get; set; }
        public bool CanEdit { get; set; } // true if not locked and within deadline and user is assigned lecturer or admin
        public List<SubjectGradeComponentDto> Components { get; set; } = new();
        public List<StudentGradeRowDto> StudentRows { get; set; } = new();
    }

    public class StudentGradeRowDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AdministrativeClassCode { get; set; }
        public Dictionary<int, decimal?> ComponentScores { get; set; } = new(); // ComponentId -> Score
        public decimal? FinalScore { get; set; }
        public bool IsPassed { get; set; }
    }

    public class UpdateStudentGradeRequest
    {
        public int CourseSectionId { get; set; }
        public List<SingleGradeUpdateDto> Grades { get; set; } = new();
    }

    public class SingleGradeUpdateDto
    {
        public int StudentId { get; set; }
        public int SubjectGradeComponentId { get; set; }
        public decimal? Score { get; set; } // null or 0.0 - 10.0
        public string? Notes { get; set; }
    }

    public class StudentSemesterTranscriptDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string? SpecializationName { get; set; }
        public int CurrentSemester { get; set; }
        public decimal CumulativeGpa { get; set; }
        public int TotalPassedCredits { get; set; }
        public int TotalEnrolledCredits { get; set; }
        public string AcademicStanding { get; set; } = string.Empty; // Xuất sắc, Giỏi, Khá, Trung bình, Yếu
        public List<StudentSubjectTranscriptDto> SubjectRecords { get; set; } = new();
    }

    public class StudentSubjectTranscriptDto
    {
        public int CourseSectionId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public List<ComponentScoreDto> Components { get; set; } = new();
        public decimal? FinalScore { get; set; }
        public bool IsPassed { get; set; }
        public bool IsLocked { get; set; }
    }

    public class ComponentScoreDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? Score { get; set; }
    }
}

