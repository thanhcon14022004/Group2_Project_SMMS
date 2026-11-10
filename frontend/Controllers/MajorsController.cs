using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class MajorsController : BaseController
    {
        public MajorsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? selectedMajorId = null)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var majors = await ApiClient.GetAsync<List<MajorViewModel>>("majors") ?? new List<MajorViewModel>();
            ViewBag.SelectedMajorId = selectedMajorId;
            return View(majors);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string? name, string? description)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var c = (code ?? "").Trim().ToUpper();
            var n = string.IsNullOrWhiteSpace(name) ? c : name.Trim();

            var (success, msg, _) = await ApiClient.PostAsync<object>("majors", new
            {
                code = c,
                name = n,
                description = description?.Trim(),
                isActive = true
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialization(int majorId, string? code, string name, string? description)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var n = (name ?? "").Trim();
            var c = string.IsNullOrWhiteSpace(code) ? n : code.Trim();

            var (success, msg, _) = await ApiClient.PostAsync<object>("specializations", new
            {
                majorId,
                code = c,
                name = n,
                description = description?.Trim(),
                isActive = true
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { selectedMajorId = majorId });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string? description, bool isActive)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"majors/{id}", new
            {
                name = name.Trim(),
                description,
                isActive
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { selectedMajorId = id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"majors/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditSpecialization(int id, string name, string? description, bool isActive, int? majorId = null)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"specializations/{id}", new
            {
                name = name.Trim(),
                description,
                isActive
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { selectedMajorId = majorId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpecialization(int id, int? majorId = null)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"specializations/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { selectedMajorId = majorId });
        }
    }
}

