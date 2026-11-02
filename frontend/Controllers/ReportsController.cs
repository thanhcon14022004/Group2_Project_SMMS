using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class ReportsController : BaseController
    {
        public ReportsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? majorId, int? classId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            string endpoint = "reports/gpa";
            if (majorId.HasValue || classId.HasValue)
            {
                var q = new List<string>();
                if (majorId.HasValue) q.Add($"majorId={majorId}");
                if (classId.HasValue) q.Add($"administrativeClassId={classId}");
                endpoint += "?" + string.Join("&", q);
            }

            var gpaTask = ApiClient.GetAsync<List<TranscriptViewModel>>(endpoint);
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");
            var classesTask = ApiClient.GetAsync<List<ClassViewModel>>("classes");

            await Task.WhenAll(gpaTask, majorsTask, classesTask);

            ViewBag.Majors = (await majorsTask) ?? new List<MajorViewModel>();
            ViewBag.Classes = (await classesTask) ?? new List<ClassViewModel>();
            var gpaList = (await gpaTask) ?? new List<TranscriptViewModel>();

            return View(gpaList);
        }

        [HttpGet]
        public async Task<IActionResult> ExportGpaCsv(int? majorId, int? classId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            string endpoint = "reports/export/gpa-csv";
            if (majorId.HasValue || classId.HasValue)
            {
                var q = new List<string>();
                if (majorId.HasValue) q.Add($"majorId={majorId}");
                if (classId.HasValue) q.Add($"administrativeClassId={classId}");
                endpoint += "?" + string.Join("&", q);
            }

            var bytes = await ApiClient.GetByteArrayAsync(endpoint);
            if (bytes == null)
            {
                SetAlert("Không thể xuất file báo cáo GPA.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return File(bytes, "text/csv", $"SMMS_BaoCao_GPA_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportGpaWord(int? majorId, int? classId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            string endpoint = "reports/export/gpa-word";
            if (majorId.HasValue || classId.HasValue)
            {
                var q = new List<string>();
                if (majorId.HasValue) q.Add($"majorId={majorId}");
                if (classId.HasValue) q.Add($"administrativeClassId={classId}");
                endpoint += "?" + string.Join("&", q);
            }

            var bytes = await ApiClient.GetByteArrayAsync(endpoint);
            if (bytes == null)
            {
                SetAlert("Không thể xuất file báo cáo Word.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return File(bytes, "application/msword", $"SMMS_BaoCao_GPA_{DateTime.UtcNow:yyyyMMdd_HHmm}.doc");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSectionGradesCsv(int sectionId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            var bytes = await ApiClient.GetByteArrayAsync($"reports/export/section-grades-csv/{sectionId}");
            if (bytes == null)
            {
                SetAlert("Không thể xuất file điểm lớp học phần.", "danger");
                return RedirectToAction("Index", "Grades", new { sectionId });
            }

            return File(bytes, "text/csv", $"SMMS_DiemLopHocPhan_{sectionId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }
    }
}

