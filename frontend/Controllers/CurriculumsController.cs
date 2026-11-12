using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class CurriculumsController : BaseController
    {
        public CurriculumsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? majorId, int? semesterNumber, string? version, string? search, int page = 1, int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (majorId.HasValue && majorId.Value > 0) queryParams.Add($"majorId={majorId.Value}");
            if (semesterNumber.HasValue && semesterNumber.Value > 0) queryParams.Add($"semesterNumber={semesterNumber.Value}");
            if (!string.IsNullOrWhiteSpace(version)) queryParams.Add($"version={Uri.EscapeDataString(version.Trim())}");
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");

            var pagedConfigsTask = ApiClient.GetAsync<PagedResult<CurriculumViewModel>>($"curriculums/paged?{string.Join("&", queryParams)}");
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");
            var specializationsTask = ApiClient.GetAsync<List<SpecializationViewModel>>("specializations");
            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>("subjects");

            await Task.WhenAll(pagedConfigsTask, majorsTask, specializationsTask, subjectsTask);

            var pagedConfigs = (await pagedConfigsTask) ?? new PagedResult<CurriculumViewModel> { PageIndex = page, PageSize = pageSize };

            ViewBag.Majors = (await majorsTask) ?? new List<MajorViewModel>();
            ViewBag.Specializations = (await specializationsTask) ?? new List<SpecializationViewModel>();
            ViewBag.Subjects = (await subjectsTask) ?? new List<SubjectViewModel>();

            // Lưu trạng thái bộ lọc
            ViewBag.CurrentMajorId = majorId;
            ViewBag.CurrentSemester = semesterNumber;
            ViewBag.CurrentVersion = version;
            ViewBag.CurrentSearch = search;
            ViewBag.TotalRecords = pagedConfigs.TotalCount;
            ViewBag.CurrentPage = pagedConfigs.PageIndex;
            ViewBag.PageSize = pagedConfigs.PageSize;
            ViewBag.TotalPages = Math.Max(1, pagedConfigs.TotalPages);

            return View(pagedConfigs);
        }

        [HttpGet]
        public IActionResult Lookup()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Create(string version, int? majorId, int semesterNumber, int? specializationId, string name, string? description, List<int> subjectIds)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("curriculums", new
            {
                version = version.Trim(),
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                semesterNumber,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                name = name.Trim(),
                description = description?.Trim(),
                subjectIds = subjectIds ?? new List<int>()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string version, int? majorId, int semesterNumber, int? specializationId, string name, string? description, bool isActive, List<int> subjectIds)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"curriculums/{id}", new
            {
                version = version?.Trim() ?? "2024.1",
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                semesterNumber,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                name = name?.Trim() ?? string.Empty,
                description = description?.Trim(),
                isActive,
                subjectIds = subjectIds ?? new List<int>()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"curriculums/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync("curriculums");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BatchCreateSections(int administrativeClassId, int semesterId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("coursesections/batch-create-from-curriculum", new
            {
                administrativeClassId,
                semesterId
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction("Index", "CourseSections");
        }
    }
}

