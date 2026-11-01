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
            [FromQuery] string? orderby)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (CurrentUser?.Role == "Admin")
            {
                return RedirectToAction("Index", "Semesters");
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

            var queryParams = new List<string>();
            if (filters.Any())
            {
                queryParams.Add($"$filter={string.Join(" and ", filters)}");
            }
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                queryParams.Add($"$orderby={orderby}");
            }

            var endpoint = "course-sections" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");

            var sectionsTask = ApiClient.GetAsync<List<CourseSectionViewModel>>(endpoint);
            var semestersTask = ApiClient.GetAsync<List<SemesterViewModel>>("semesters");
            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>("subjects");
            var lecturersTask = ApiClient.GetAsync<List<LecturerViewModel>>("lecturers");

            await Task.WhenAll(sectionsTask, semestersTask, subjectsTask, lecturersTask);

            ViewBag.Semesters = (await semestersTask) ?? new List<SemesterViewModel>();
            ViewBag.Subjects = (await subjectsTask) ?? new List<SubjectViewModel>();
            ViewBag.Lecturers = (await lecturersTask) ?? new List<LecturerViewModel>();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSemesterId = semesterId;
            ViewBag.CurrentSubjectId = subjectId;
            ViewBag.CurrentIsLocked = isLocked;
            ViewBag.CurrentOrderBy = orderby;
            ViewBag.CurrentODataQuery = queryParams.Any() ? string.Join("&", queryParams) : "";

            var sections = (await sectionsTask) ?? new List<CourseSectionViewModel>();

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
        public async Task<IActionResult> RunAllocation(int semesterId, int? majorId, int? semesterNumber)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, data) = await ApiClient.PostAsync<AllocationViewModel>("allocations/run", new
            {
                semesterId,
                majorId,
                semesterNumber
            });

            SetAlert(msg, success ? "success" : "danger");
            var allSemesters = await ApiClient.GetAsync<List<SemesterViewModel>>("semesters") ?? new List<SemesterViewModel>();
            var activeSemesters = allSemesters.Where(s => string.Equals(s.Status, "Active", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Semesters = activeSemesters.Any() ? activeSemesters : allSemesters;

            return View("Allocations", data ?? new AllocationViewModel());
        }
    }
}
