using System.Text.Json.Serialization;

namespace project_PRN232.Models.Entities
{
    public class StudentGrade
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public int StudentId { get; set; }
        public int SubjectGradeComponentId { get; set; }
        public decimal? Score { get; set; } // Nullable: Null means not entered yet, 0.0 means scored 0
        public string? Notes { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        [JsonIgnore]
        public virtual CourseSection? CourseSection { get; set; }
        public virtual Student? Student { get; set; }
        public virtual SubjectGradeComponent? SubjectGradeComponent { get; set; }
    }

    public class ClassTransferRequest
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CurrentAdministrativeClassId { get; set; }
        public int TargetAdministrativeClassId { get; set; }
        public int SemesterId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public string? Note { get; set; }

        public virtual Student? Student { get; set; }
        public virtual AdministrativeClass? CurrentClass { get; set; }
        public virtual AdministrativeClass? TargetClass { get; set; }
        public virtual Semester? Semester { get; set; }
    }

    public class SpecializationSelection
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SpecializationId { get; set; }
        public int SemesterId { get; set; }
        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Approved"; // Pending, Approved

        public virtual Student? Student { get; set; }
        public virtual Specialization? Specialization { get; set; }
        public virtual Semester? Semester { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty; // GRADE_UPDATE, GRADE_LOCKED, GRADE_UNLOCKED, LECTURER_CHANGE, CLASS_TRANSFER, CURRICULUM_UPDATE, SEMESTER_CLOSE_EXCEPTION, ACCOUNT_DEACTIVATE, DB_RESTORE
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string PerformedBy { get; set; } = "System";
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
    }

    public class BackupRecord
    {
        public int Id { get; set; }
        public string BackupFileName { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "System";
        public long FileSize { get; set; }
        public string? Notes { get; set; }
        public bool IsAutomated { get; set; } = false;
        public DateTime? LastRestoredAt { get; set; }
        public string? LastRestoredBy { get; set; }
    }
}

