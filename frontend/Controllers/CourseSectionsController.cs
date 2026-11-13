using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class CourseSectionsController : BaseController
    {
        public CourseSectionsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] int? semesterId,
            [FromQuery] int? subjectId,
            [FromQuery] bool? isLocked,
            [FromQuery] string? orderby,
            [FromQuery] int weekOffset = 0,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (CurrentUser?.Role == "Admin")
            {
                return RedirectToAction("Index", "Semesters");
            }

            if (CurrentUser?.Role == "Student")
            {
                pageSize = 100;
            }

            if (!semesterId.HasValue && !Request.Query.ContainsKey("semesterId") && CurrentUser?.Role == "Student")
            {
                var allSems = await ApiClient.GetAsync<List<SemesterViewModel>>("semesters") ?? new List<SemesterViewModel>();
                var activeSem = allSems.FirstOrDefault(s => string.Equals(s.Status, "Active", StringComparison.OrdinalIgnoreCase))
                                ?? allSems.OrderByDescending(s => s.Id).FirstOrDefault();
                if (activeSem != null)
                {
                    semesterId = activeSem.Id;
                }
            }

            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filters.Add($"(contains(tolower(Code), '{s}') or contains(tolower(Name), '{s}') or contains(tolower(SubjectName), '{s}'))");
            }
            if (semesterId.HasValue && semesterId.Value > 0)
            {
                filters.Add($"SemesterId eq {semesterId.Value}");
            }
            if (subjectId.HasValue && subjectId.Value > 0)
            {
                filters.Add($"SubjectId eq {subjectId.Value}");
            }
            if (isLocked.HasValue)
            {
                filters.Add($"IsGradeLocked eq {isLocked.Value.ToString().ToLower()}");
            }

            var odataPreviewParams = new List<string>();
            if (filters.Any())
            {
                odataPreviewParams.Add($"$filter={string.Join(" and ", filters)}");
            }
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                odataPreviewParams.Add($"$orderby={orderby}");
            }

            var pagedQuery = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(search)) pagedQuery.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (semesterId.HasValue && semesterId.Value > 0) pagedQuery.Add($"semesterId={semesterId.Value}");
            if (subjectId.HasValue && subjectId.Value > 0) pagedQuery.Add($"subjectId={subjectId.Value}");
            if (isLocked.HasValue) pagedQuery.Add($"isLocked={isLocked.Value.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(orderby)) pagedQuery.Add($"orderby={Uri.EscapeDataString(orderby)}");

            var sectionsTask = ApiClient.GetAsync<PagedResult<CourseSectionViewModel>>($"course-sections/paged?{string.Join("&", pagedQuery)}");
            var semestersTask = ApiClient.GetAsync<List<SemesterViewModel>>("semesters");
            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>("subjects");
            var lecturersTask = CurrentUser?.Role == "Admin"
                ? ApiClient.GetAsync<List<LecturerViewModel>>("lecturers")
                : Task.FromResult<List<LecturerViewModel>?>(new List<LecturerViewModel>());
            var transcriptTask = CurrentUser?.Role == "Student"
                ? ApiClient.GetAsync<TranscriptViewModel>("grades/transcript")
                : Task.FromResult<TranscriptViewModel?>(new TranscriptViewModel());

            await Task.WhenAll(sectionsTask, semestersTask, subjectsTask, lecturersTask, transcriptTask);

            ViewBag.Semesters = (await semestersTask) ?? new List<SemesterViewModel>();
            ViewBag.Subjects = (await subjectsTask) ?? new List<SubjectViewModel>();
            ViewBag.Lecturers = (await lecturersTask) ?? new List<LecturerViewModel>();
            ViewBag.Transcript = (await transcriptTask) ?? new TranscriptViewModel();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSemesterId = semesterId;
            ViewBag.CurrentSubjectId = subjectId;
            ViewBag.CurrentIsLocked = isLocked;
            ViewBag.CurrentOrderBy = orderby;
            ViewBag.CurrentODataQuery = odataPreviewParams.Any() ? string.Join("&", odataPreviewParams) : "";
            ViewBag.WeekOffset = weekOffset;

            var sections = (await sectionsTask) ?? new PagedResult<CourseSectionViewModel> { PageIndex = page, PageSize = pageSize };

            return View(sections);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string name, int subjectId, int semesterId, int? lecturerId, int maxCapacity = 30)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("course-sections", new
            {
                code = code.Trim().ToUpper(),
                name = name.Trim(),
                subjectId,
                semesterId,
                lecturerId,
                maxCapacity = maxCapacity > 0 ? maxCapacity : 30
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignLecturer(int sectionId, int? lecturerId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"course-sections/{sectionId}/lecturer", new
            {
                lecturerId
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, int maxCapacity, int? lecturerId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"course-sections/{id}", new
            {
                name = name.Trim(),
                maxCapacity = maxCapacity > 0 ? maxCapacity : 30,
                lecturerId
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"course-sections/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Allocations()
        {
            return RedirectToAction("Allocations", "Classes");
        }

        [HttpPost]
        public IActionResult RunAllocation(int semesterId, int? majorId, int? semesterNumber)
        {
            return RedirectToAction("Allocations", "Classes", new { semesterId });
        }
    }
}
