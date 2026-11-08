namespace project_PRN232.DTOs
{
    // Phân bổ sinh viên
    public class RunAllocationRequest
    {
        public int SemesterId { get; set; }
        public int? MajorId { get; set; }
        public int? AdministrativeClassId { get; set; }
        public string? CurriculumVersion { get; set; } = "2024.1";
    }

    public class AllocationResultDto
    {
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public int TotalStudentsProcessed { get; set; }
        public int TotalEligibleStudents { get; set; }
        public int TotalEnrolledSections { get; set; }
        public int AutoCreatedStudentsCount { get; set; }
        public int BlockedPrerequisiteCount { get; set; }
        public int CapacityOverflowCount { get; set; }
        public int UnallocatedCount { get; set; }
        public List<AllocationItemDto> Allocations { get; set; } = new();
        public List<UnallocatedExceptionDto> UnallocatedExceptions { get; set; } = new();
        public List<UnallocatedExceptionDto> Exceptions => UnallocatedExceptions;
        public List<ClassAllocationSummaryDto> ClassSummaries { get; set; } = new();
    }

    public class AllocationItemDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string AdministrativeClassCode { get; set; } = string.Empty;
        public bool IsAutoCreated { get; set; }
    }

    public class ClassAllocationSummaryDto
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

    public class UnallocatedExceptionDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string AdministrativeClassCode { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty; // "Thiếu môn tiên quyết: PRF192", "Lớp đã đầy", "Trùng lịch học", "Chưa mở lớp học phần cho môn này"
    }

    // Kết thúc học kỳ
    public class SemesterClosingCheckResultDto
    {
        public int SemesterId { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public bool CanCloseImmediately { get; set; }
        public List<string> BlockingIssues { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int UnassignedLecturerSectionsCount { get; set; }
        public int MissingGradeSectionsCount { get; set; }
        public int UnlockedSectionsCount { get; set; }
        public int UnallocatedCompulsoryStudentsCount { get; set; }
    }

    public class CloseSemesterRequest
    {
        public int SemesterId { get; set; }
        public bool ForceWithExceptions { get; set; }
        public string? Reason { get; set; }
    }

    // Chuyển lớp
    public class RequestClassTransferDto
    {
        public int TargetAdministrativeClassId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ClassTransferCheckResultDto
    {
        public bool IsEligible { get; set; }
        public string? Message { get; set; }
        public List<string> IncompatibleSections { get; set; } = new();
        public List<string> CompatibleSections { get; set; } = new();
    }

    public class ProcessClassTransferRequestDto
    {
        public int RequestId { get; set; }
        public bool Approve { get; set; }
        public string? Note { get; set; }
    }

    // Chọn ngành hẹp (Kỳ 4 lên kỳ 5)
    public class SelectSpecializationRequest
    {
        public int SpecializationId { get; set; }
    }

    // Báo cáo & Thống kê
    public class DashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalSections { get; set; }
        public int ActiveSemesterId { get; set; }
        public string ActiveSemesterName { get; set; } = string.Empty;
        public int UnallocatedExceptionsCount { get; set; }
        public int SectionsLackingGradesCount { get; set; }
        public int SectionsLackingLecturerCount { get; set; }
        public decimal AverageSystemGpa { get; set; }
        public List<GpaDistributionDto> GpaDistribution { get; set; } = new();
    }

    public class GpaDistributionDto
    {
        public string Rank { get; set; } = string.Empty; // Xuất sắc, Giỏi, Khá, Trung bình, Yếu
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    // Sao lưu & Khôi phục
    public class RestoreDatabaseRequest
    {
        public int BackupId { get; set; }
        public string ConfirmationText { get; set; } = string.Empty; // Phải gõ đúng "RESTORE" để xác nhận
    }

    // Phân bổ giáo viên vào lớp hành chính
    public class AssignClassLecturersRequest
    {
        public List<SectionLecturerAssignmentDto> Assignments { get; set; } = new();
    }

    public class SectionLecturerAssignmentDto
    {
        public int CourseSectionId { get; set; }
        public int? LecturerId { get; set; }
    }

    public class AutoAssignLecturersRequest
    {
        public int? ClassId { get; set; }
        public int? SemesterId { get; set; }
    }

    public class ClassLecturersAssignmentDto
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public string? SpecializationCode { get; set; }
        public int CurrentSemester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public List<SectionLecturerItemDto> Sections { get; set; } = new();
        public List<LecturerOptionDto> Lecturers { get; set; } = new();
    }

    public class SectionLecturerItemDto
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
        public List<CourseSectionScheduleDto> Schedules { get; set; } = new();
    }

    public class LecturerOptionDto
    {
        public int Id { get; set; }
        public string LecturerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string AcademicTitle { get; set; } = string.Empty;
        public bool IsPreferred { get; set; }
        public List<int> RegisteredSubjectIds { get; set; } = new();
    }
}

