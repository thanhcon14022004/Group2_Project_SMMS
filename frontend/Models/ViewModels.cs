namespace project_PRN232.Frontend.Models
{
    public class CurrentUserViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? StudentId { get; set; }
        public int? LecturerId { get; set; }
        public string? StudentCode { get; set; }
        public string? LecturerCode { get; set; }
    }

    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalSections { get; set; }
        public decimal AverageGpa { get; set; }
        public string ActiveSemesterName { get; set; } = string.Empty;
        public int SectionsLackingLecturer { get; set; }
        public int SectionsLackingGrades { get; set; }
        public int UnallocatedExceptions { get; set; }
        public List<StandingCountViewModel> StandingDistribution { get; set; } = new();
    }

    public class StandingCountViewModel
    {
        public string Standing { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MajorViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<SpecializationViewModel> Specializations { get; set; } = new();
    }

    public class SpecializationViewModel
    {
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubjectViewModel
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public int? MajorId { get; set; }
        public string? MajorCode { get; set; }
        public string? MajorName { get; set; }
        public bool IsActive { get; set; }
        public List<SubjectGradeComponentViewModel> GradeComponents { get; set; } = new();
        public List<PrerequisiteViewModel> Prerequisites { get; set; } = new();

        public int EffectiveId => Id > 0 ? Id : SubjectId;
        public string EffectiveCode => !string.IsNullOrEmpty(Code) ? Code : SubjectCode;
        public string EffectiveName => !string.IsNullOrEmpty(Name) ? Name : SubjectName;
    }

    public class SubjectGradeComponentViewModel
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Scale { get; set; } = 10m;
        public bool IsAdminOnly { get; set; }
        public int OrderIndex { get; set; }
    }

    public class PrerequisiteViewModel
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int PrerequisiteSubjectId { get; set; }
        public string PrerequisiteSubjectCode { get; set; } = string.Empty;
        public string PrerequisiteSubjectName { get; set; } = string.Empty;
        public decimal MinScore { get; set; } = 5.0m;
    }

    public class CurriculumViewModel
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public int? MajorId { get; set; }
        public string? MajorCode { get; set; } = string.Empty;
        public string? MajorName { get; set; } = string.Empty;
        public int SemesterNumber { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<SubjectViewModel> Subjects { get; set; } = new();
    }

    public class CurriculumLookupViewModel
    {
        public int MajorId { get; set; }
        public int SemesterNumber { get; set; } = 1;
        public int? SpecializationId { get; set; }
        public bool IsFound { get; set; }
        public string? ConfigName { get; set; }
        public string? Version { get; set; }
        public string? MajorCode { get; set; }
        public string? SpecializationCode { get; set; }
        public string? Message { get; set; }
        public List<SubjectViewModel> Subjects { get; set; } = new();
    }

    public class ClassViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public int Cohort { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public int CurrentSemester { get; set; }
        public int StudentCount { get; set; }
        public int CourseSectionsCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClassSectionDetailViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; } = 3;
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public string LecturerName { get; set; } = "Chưa phân công";
        public int MaxCapacity { get; set; } = 30;
        public int EnrolledCount { get; set; }
        public string Status { get; set; } = "Open";
        public bool IsGradeLocked { get; set; }
        public List<ClassSectionScheduleViewModel> Schedules { get; set; } = new();
    }

    public class ClassSectionScheduleViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekStr { get; set; } = string.Empty;
        public int Slot { get; set; }
        public string Room { get; set; } = string.Empty;
    }

    public class ClassSectionsResponseViewModel
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string MajorCode { get; set; } = string.Empty;
        public string? SpecializationCode { get; set; }
        public int CurrentSemester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int SectionsCount { get; set; }
        public List<ClassSectionDetailViewModel> Sections { get; set; } = new();
    }

    public class ClassCreateResponseViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public int Cohort { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }
        public int? SpecializationId { get; set; }
        public int CreatedSectionsCount { get; set; }
        public string? Message { get; set; }
    }

    public class ClassStudentItemViewModel
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ClassStudentsResponseViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string? MajorCode { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public int Cohort { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }
        public bool IsActive { get; set; }
        public List<ClassStudentItemViewModel> Students { get; set; } = new();
    }

    public class ClassLecturersAssignmentViewModel
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public string? SpecializationCode { get; set; }
        public int CurrentSemester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public List<SectionLecturerItemViewModel> Sections { get; set; } = new();
        public List<LecturerOptionViewModel> Lecturers { get; set; } = new();
    }

    public class SectionLecturerItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int? LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerCode { get; set; } = string.Empty;
        public string LecturerDepartment { get; set; } = string.Empty;
        public List<ClassSectionScheduleViewModel> Schedules { get; set; } = new();
    }

    public class LecturerOptionViewModel
    {
        public int Id { get; set; }
        public string LecturerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string AcademicTitle { get; set; } = string.Empty;
        public bool IsPreferred { get; set; }
    }

    public class SectionLecturerAssignmentInputModel
    {
        public int CourseSectionId { get; set; }
        public int? LecturerId { get; set; }
    }

    public class AssignClassLecturersInputModel
    {
        public int ClassId { get; set; }
        public List<SectionLecturerAssignmentInputModel> Assignments { get; set; } = new();
    }

    public class AutoAssignLecturersResponseViewModel
    {
        public string Message { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int ClassesCount { get; set; }
    }

    public class ClassTransferViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CurrentClassName { get; set; } = string.Empty;
        public string TargetClassName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? Note { get; set; }
    }

    public class CourseSectionViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public int? LecturerId { get; set; }
        public string? LecturerName { get; set; }
        public int EnrolledStudentsCount { get; set; }
        public int MaxCapacity { get; set; }
        public bool IsGradeLocked { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CourseScheduleViewModel> Schedules { get; set; } = new();
    }

    public class CourseScheduleViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int Slot { get; set; }
        public string Room { get; set; } = string.Empty;
    }

    public class AllocationViewModel
    {
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public int TotalEligibleStudents { get; set; }
        public int TotalEnrolledSections { get; set; }
        public int AutoCreatedStudentsCount { get; set; }
        public int BlockedPrerequisiteCount { get; set; }
        public int CapacityOverflowCount { get; set; }
        public List<AllocationItemViewModel> Allocations { get; set; } = new();
        public List<UnallocatedExceptionViewModel> Exceptions { get; set; } = new();
        public List<ClassAllocationSummaryViewModel> ClassSummaries { get; set; } = new();
    }

    public class AllocationItemViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string AdministrativeClassCode { get; set; } = string.Empty;
        public bool IsAutoCreated { get; set; }
    }

    public class ClassAllocationSummaryViewModel
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string MajorCode { get; set; } = string.Empty;
        public string? SpecializationCode { get; set; }
        public int SemesterNumber { get; set; }
        public int StudentCount { get; set; }
        public int AutoCreatedCount { get; set; }
        public int SectionsCount { get; set; }
    }

    public class UnallocatedExceptionViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class GradeSheetViewModel
    {
        public int CourseSectionId { get; set; }
        public string CourseSectionCode { get; set; } = string.Empty;
        public string CourseSectionName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? LecturerName { get; set; }
        public int? AdministrativeClassId { get; set; }
        public string? AdministrativeClassCode { get; set; }
        public string? SemesterCode { get; set; }
        public bool IsGradeLocked { get; set; }
        public bool CanEdit { get; set; }
        public List<SubjectGradeComponentViewModel> Components { get; set; } = new();
        public List<StudentGradeRowViewModel> StudentRows { get; set; } = new();
    }

    public class StudentGradeRowViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AdministrativeClassCode { get; set; }
        public Dictionary<int, decimal?> ComponentScores { get; set; } = new();
        public decimal? FinalScore { get; set; }
        public bool IsPassed { get; set; }
    }

    public class TranscriptViewModel
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
        public string AcademicStanding { get; set; } = string.Empty;
        public List<TranscriptSubjectRecordViewModel> SubjectRecords { get; set; } = new();
    }

    public class TranscriptComponentScoreViewModel
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? Score { get; set; }
    }

    public class TranscriptSubjectRecordViewModel
    {
        public int CourseSectionId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public List<TranscriptComponentScoreViewModel> Components { get; set; } = new();
        public decimal? FinalScore { get; set; }
        public bool IsPassed { get; set; }
        public bool IsLocked { get; set; }
    }

    public class SemesterViewModel
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

    public class SemesterClassesViewModel
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
        public List<SemesterAdminClassViewModel> AdministrativeClasses { get; set; } = new();
        public List<SemesterCourseSectionViewModel> CourseSections { get; set; } = new();
    }

    public class SemesterAdminClassViewModel
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

    public class SemesterCourseSectionViewModel
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
        public List<CourseScheduleViewModel> Schedules { get; set; } = new();
    }

    public class SemesterClosingInspectionViewModel
    {
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public bool CanCloseImmediately { get; set; }
        public int SectionsLackingGradesCount { get; set; }
        public int PendingTransfersCount { get; set; }
        public int UnassignedStudentsCount { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    public class UserItemViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuditLogItemViewModel
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string? Details { get; set; }
    }

    public class LecturerViewModel
    {
        public int Id { get; set; }
        public string LecturerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudentViewModel
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? MajorId { get; set; }
        public string? MajorName { get; set; }
        public int? SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public bool IsActive { get; set; }
    }

    public class BackupItemViewModel
    {
        public int Id { get; set; }
        public string BackupFileName { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Notes { get; set; }
    }
}
