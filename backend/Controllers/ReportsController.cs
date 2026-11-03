using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly AppDbContext _db;

        public ReportsController(IReportService reportService, AppDbContext db)
        {
            _reportService = reportService;
            _db = db;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _reportService.GetDashboardSummaryAsync();
            return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
        }

        [HttpGet("gpa")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> GetGpaReport([FromQuery] int? majorId, [FromQuery] int? administrativeClassId)
        {
            var report = await _reportService.GetStudentsGpaReportAsync(majorId, administrativeClassId);
            return Ok(ApiResponse<List<StudentSemesterTranscriptDto>>.Ok(report));
        }

        [HttpGet("students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentsReport([FromQuery] int? majorId, [FromQuery] int? cohort)
        {
            var query = _db.Students
                .Include(s => s.User)
                .Include(s => s.Major)
                .Include(s => s.AdministrativeClass)
                .AsNoTracking()
                .AsQueryable();

            if (majorId.HasValue) query = query.Where(s => s.MajorId == majorId.Value);
            if (cohort.HasValue) query = query.Where(s => s.Cohort == cohort.Value);

            var list = await query.Select(s => new
            {
                s.Id,
                s.StudentCode,
                FullName = s.User != null ? s.User.FullName : "",
                Email = s.User != null ? s.User.Email : "",
                MajorCode = s.Major != null ? s.Major.Code : "",
                ClassName = s.AdministrativeClass != null ? s.AdministrativeClass.Name : "",
                s.Cohort,
                s.CurrentSemester,
                s.Status,
                IsActive = s.User != null && s.User.IsActive
            }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("grades")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGradesReport([FromQuery] int? semesterId)
        {
            var activeSemId = semesterId ?? (await _db.Semesters.OrderByDescending(s => s.StartDate).Select(s => s.Id).FirstOrDefaultAsync());

            var sections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.SemesterId == activeSemId)
                .AsNoTracking()
                .ToListAsync();

            var sectionIds = sections.Select(cs => cs.Id).ToList();
            var gradeCounts = await _db.StudentGrades
                .Where(g => sectionIds.Contains(g.CourseSectionId) && g.Score.HasValue)
                .GroupBy(g => g.CourseSectionId)
                .Select(grp => new { SectionId = grp.Key, Count = grp.Count() })
                .ToDictionaryAsync(x => x.SectionId, x => x.Count);

            var result = sections.Select(cs => new
            {
                cs.Id,
                SectionCode = cs.Code,
                SubjectCode = cs.Subject?.Code ?? "",
                LecturerName = cs.Lecturer?.User?.FullName ?? "Chưa phân công",
                TotalStudents = cs.Enrollments.Count,
                TotalGradesEntered = gradeCounts.ContainsKey(cs.Id) ? gradeCounts[cs.Id] : 0,
                cs.IsGradeLocked,
                Status = cs.Status
            }).ToList();

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("course-sections")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCourseSectionsReport([FromQuery] int? semesterId)
        {
            var activeSemId = semesterId ?? (await _db.Semesters.OrderByDescending(s => s.StartDate).Select(s => s.Id).FirstOrDefaultAsync());

            var sections = await _db.CourseSections
                .Include(cs => cs.Subject)
                .Include(cs => cs.Semester)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.SemesterId == activeSemId)
                .AsNoTracking()
                .Select(cs => new
                {
                    cs.Id,
                    cs.Code,
                    cs.Name,
                    SubjectCode = cs.Subject != null ? cs.Subject.Code : "",
                    LecturerName = cs.Lecturer != null && cs.Lecturer.User != null ? cs.Lecturer.User.FullName : "Chưa phân công",
                    EnrolledCount = cs.Enrollments.Count,
                    cs.MaxCapacity,
                    cs.IsGradeLocked,
                    cs.Status
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(sections));
        }

        [HttpGet("unassigned")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnassignedStudentsReport([FromQuery] int? semesterId)
        {
            var activeSemId = semesterId ?? (await _db.Semesters.OrderByDescending(s => s.StartDate).Select(s => s.Id).FirstOrDefaultAsync());

            // Sinh viên đang học nhưng chưa có trong bất kỳ CourseSectionStudent nào của học kỳ này
            var enrolledStudentIds = await _db.CourseSectionStudents
                .Include(css => css.CourseSection)
                .Where(css => css.CourseSection != null && css.CourseSection.SemesterId == activeSemId)
                .Select(css => css.StudentId)
                .Distinct()
                .ToListAsync();

            var unassigned = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Major)
                .Include(s => s.AdministrativeClass)
                .Where(s => s.Status == "Studying" && !enrolledStudentIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.StudentCode,
                    FullName = s.User != null ? s.User.FullName : "",
                    MajorCode = s.Major != null ? s.Major.Code : "",
                    ClassName = s.AdministrativeClass != null ? s.AdministrativeClass.Name : "",
                    s.CurrentSemester
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(unassigned));
        }

        [HttpGet("missing-grades")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMissingGradesReport([FromQuery] int? semesterId)
        {
            var activeSemId = semesterId ?? (await _db.Semesters.OrderByDescending(s => s.StartDate).Select(s => s.Id).FirstOrDefaultAsync());

            var sections = await _db.CourseSections
                .Include(cs => cs.Subject).ThenInclude(sub => sub!.GradeComponents)
                .Include(cs => cs.Lecturer).ThenInclude(l => l!.User)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.SemesterId == activeSemId)
                .AsNoTracking()
                .ToListAsync();

            var sectionIds = sections.Select(cs => cs.Id).ToList();
            var gradeCounts = await _db.StudentGrades
                .Where(g => sectionIds.Contains(g.CourseSectionId) && g.Score.HasValue)
                .GroupBy(g => g.CourseSectionId)
                .Select(grp => new { SectionId = grp.Key, Count = grp.Count() })
                .ToDictionaryAsync(x => x.SectionId, x => x.Count);

            var missing = sections
                .Where(cs =>
                {
                    int compCount = cs.Subject?.GradeComponents.Count ?? 0;
                    int expectedGrades = cs.Enrollments.Count * compCount;
                    int actualEntered = gradeCounts.ContainsKey(cs.Id) ? gradeCounts[cs.Id] : 0;
                    return actualEntered < expectedGrades;
                })
                .Select(cs =>
                {
                    int compCount = cs.Subject?.GradeComponents.Count ?? 0;
                    int expectedGrades = cs.Enrollments.Count * compCount;
                    int actualEntered = gradeCounts.ContainsKey(cs.Id) ? gradeCounts[cs.Id] : 0;
                    return new
                    {
                        cs.Id,
                        SectionCode = cs.Code,
                        SubjectCode = cs.Subject?.Code ?? "",
                        LecturerName = cs.Lecturer?.User?.FullName ?? "Chưa phân công",
                        EnrolledStudents = cs.Enrollments.Count,
                        ExpectedGrades = expectedGrades,
                        EnteredGrades = actualEntered,
                        MissingGrades = expectedGrades - actualEntered
                    };
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(missing));
        }

        [HttpGet("export/gpa-csv")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> ExportGpaCsv([FromQuery] int? majorId, [FromQuery] int? administrativeClassId)
        {
            var bytes = await _reportService.ExportGpaReportCsvAsync(majorId, administrativeClassId);
            return File(bytes, "text/csv", $"SMMS_BaoCao_GPA_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet("export/gpa-word")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> ExportGpaWord([FromQuery] int? majorId, [FromQuery] int? administrativeClassId)
        {
            var bytes = await _reportService.ExportGpaReportWordAsync(majorId, administrativeClassId);
            return File(bytes, "application/msword", $"SMMS_BaoCao_GPA_{DateTime.UtcNow:yyyyMMdd_HHmm}.doc");
        }

        [HttpGet("export/section-grades-csv/{courseSectionId}")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> ExportSectionGradesCsv(int courseSectionId)
        {
            var bytes = await _reportService.ExportSectionGradesCsvAsync(courseSectionId);
            return File(bytes, "text/csv", $"SMMS_DiemLopHocPhan_{courseSectionId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet("export/section-grades-word/{courseSectionId}")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> ExportSectionGradesWord(int courseSectionId)
        {
            var bytes = await _reportService.ExportSectionGradesWordAsync(courseSectionId);
            return File(bytes, "application/msword", $"SMMS_DiemLopHocPhan_{courseSectionId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.doc");
        }
    }
}
