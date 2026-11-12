using Microsoft.EntityFrameworkCore;

namespace project_PRN232.DTOs
{
    public static class PaginationHelper
    {
        public static (int page, int pageSize) Normalize(int page, int pageSize, int defaultPageSize = 20, int maxPageSize = 200)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = defaultPageSize;
            if (pageSize > maxPageSize) pageSize = maxPageSize;
            return (page, pageSize);
        }

        public static PagedResult<T> Paginate<T>(IEnumerable<T> source, int page, int pageSize, int maxPageSize = 200)
        {
            var (normalizedPage, normalizedSize) = Normalize(page, pageSize, maxPageSize: maxPageSize);
            var list = source as IList<T> ?? source.ToList();
            var totalCount = list.Count;
            var totalPages = normalizedSize > 0 ? (int)Math.Ceiling((double)totalCount / normalizedSize) : 0;
            if (totalPages > 0 && normalizedPage > totalPages)
            {
                normalizedPage = totalPages;
            }

            var items = list
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = normalizedPage,
                PageSize = normalizedSize
            };
        }

        public static async Task<PagedResult<T>> PaginateAsync<T>(IQueryable<T> query, int page, int pageSize, int maxPageSize = 200)
        {
            var (normalizedPage, normalizedSize) = Normalize(page, pageSize, maxPageSize: maxPageSize);
            var totalCount = await query.CountAsync();
            var totalPages = normalizedSize > 0 ? (int)Math.Ceiling((double)totalCount / normalizedSize) : 0;
            if (totalPages > 0 && normalizedPage > totalPages)
            {
                normalizedPage = totalPages;
            }

            var items = await query
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = normalizedPage,
                PageSize = normalizedSize
            };
        }
    }

    public class ClassTransferRequestDto
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

    public class GpaReportPagedDto
    {
        public PagedResult<StudentSemesterTranscriptDto> PagedItems { get; set; } = new();
        public decimal AverageCumulativeGpa { get; set; }
        public int HighStandingCount { get; set; }
    }

    public class StudentTranscriptPagedDto
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
        public List<string> AvailableSemesters { get; set; } = new();
        public PagedResult<StudentSubjectTranscriptDto> PagedSubjects { get; set; } = new();
    }

    public class SectionGradeReportPagedDto
    {
        public int CourseSectionId { get; set; }
        public string CourseSectionCode { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? LecturerName { get; set; }
        public string? SemesterCode { get; set; }
        public bool IsGradeLocked { get; set; }
        public bool CanEdit { get; set; }
        public int TotalStudents { get; set; }
        public int GradedStudents { get; set; }
        public int PassedStudents { get; set; }
        public int FailedStudents { get; set; }
        public double PassRate { get; set; }
        public double FailRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal MinScore { get; set; }
        public int CountXuatSac { get; set; }
        public int CountGioi { get; set; }
        public int CountKha { get; set; }
        public int CountTrungBinh { get; set; }
        public int CountTruot { get; set; }
        public List<SubjectGradeComponentDto> Components { get; set; } = new();
        public PagedResult<StudentGradeRowDto> PagedStudentRows { get; set; } = new();
    }

    public class AllocationStatusPagedDto
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
        public PagedResult<ClassAllocationSummaryDto> PagedClassSummaries { get; set; } = new();
    }
}

