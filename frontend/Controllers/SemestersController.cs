using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class SemestersController : BaseController
    {
        public SemestersController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int? year,
            [FromQuery] string? orderby,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filters.Add($"(contains(tolower(Code), '{s}') or contains(tolower(Name), '{s}'))");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                filters.Add($"Status eq '{status.Trim()}'");
            }
            if (year.HasValue && year.Value > 0)
            {
                var yStr = year.Value.ToString();
                var shortY = yStr.Length >= 2 ? yStr.Substring(yStr.Length - 2) : yStr;
                filters.Add($"(contains(Code, '{shortY}') or contains(Name, '{yStr}'))");
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
            if (!string.IsNullOrWhiteSpace(status)) pagedQuery.Add($"status={Uri.EscapeDataString(status.Trim())}");
            if (year.HasValue && year.Value > 0) pagedQuery.Add($"year={year.Value}");
            if (!string.IsNullOrWhiteSpace(orderby)) pagedQuery.Add($"orderby={Uri.EscapeDataString(orderby)}");

            var semesters = await ApiClient.GetAsync<PagedResult<SemesterViewModel>>($"semesters/paged?{string.Join("&", pagedQuery)}")
                ?? new PagedResult<SemesterViewModel> { PageIndex = page, PageSize = pageSize };

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentYear = year;
            ViewBag.CurrentOrderBy = orderby;
            ViewBag.CurrentODataQuery = odataPreviewParams.Any() ? string.Join("&", odataPreviewParams) : "";

            return View(semesters);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string name, DateTime startDate, DateTime endDate, DateTime? transferClassDeadline, DateTime? gradeEntryDeadline)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            if (startDate == default) startDate = DateTime.Today;
            if (endDate == default || endDate <= startDate) endDate = startDate.AddMonths(4);
            var transfer = (transferClassDeadline.HasValue && transferClassDeadline.Value != default) ? transferClassDeadline.Value : startDate.AddDays(14);
            var grade = (gradeEntryDeadline.HasValue && gradeEntryDeadline.Value != default) ? gradeEntryDeadline.Value : endDate;

            var (success, msg, _) = await ApiClient.PostAsync<object>("semesters", new
            {
                code = code.Trim().ToUpper(),
                name = name.Trim(),
                startDate,
                endDate,
                transferClassDeadline = transfer,
                gradeEntryDeadline = grade,
                status = "Active"
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, DateTime startDate, DateTime endDate, DateTime? transferClassDeadline, DateTime? gradeEntryDeadline, string status)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            if (startDate == default) startDate = DateTime.Today;
            if (endDate == default || endDate <= startDate) endDate = startDate.AddMonths(4);
            var transfer = (transferClassDeadline.HasValue && transferClassDeadline.Value != default) ? transferClassDeadline.Value : startDate.AddDays(14);
            var grade = (gradeEntryDeadline.HasValue && gradeEntryDeadline.Value != default) ? gradeEntryDeadline.Value : endDate;

            var (success, msg) = await ApiClient.PutAsync($"semesters/{id}", new
            {
                name = name.Trim(),
                startDate,
                endDate,
                transferClassDeadline = transfer,
                gradeEntryDeadline = grade,
                status = !string.IsNullOrWhiteSpace(status) ? status : "Active"
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Lock(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>($"semesters/{id}/lock", new { });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>($"semesters/{id}/unlock", new { });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"semesters/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Inspect(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var inspection = await ApiClient.GetAsync<SemesterClosingInspectionViewModel>($"semesterclosing/inspect/{id}")
                ?? new SemesterClosingInspectionViewModel { SemesterId = id };

            return Json(inspection);
        }

        [HttpPost]
        public async Task<IActionResult> Close(int semesterId, bool allowExceptions, string? exceptionReason)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("semesterclosing/close", new
            {
                semesterId,
                allowExceptions,
                exceptionReason
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }
    }
}

