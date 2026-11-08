using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class OperationsController : BaseController
    {
        public OperationsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Users(string? role, string? search)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            string endpoint = "users";
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(role)) q.Add($"role={role}");
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={search}");
            if (q.Count > 0) endpoint += "?" + string.Join("&", q);

            var users = await ApiClient.GetAsync<List<UserItemViewModel>>(endpoint) ?? new List<UserItemViewModel>();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id, bool isActive)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PatchAsync($"users/{id}/status", new { isActive });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string password, string fullName, string email, string? phoneNumber, string role)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("users", new
            {
                username = username.Trim(),
                password = password.Trim(),
                fullName = fullName.Trim(),
                email = email.Trim(),
                phoneNumber,
                role
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, string fullName, string email, string? phoneNumber, string? password)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"users/{id}", new
            {
                fullName = fullName.Trim(),
                email = email.Trim(),
                phoneNumber,
                password = !string.IsNullOrWhiteSpace(password) ? password.Trim() : null
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"users/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public IActionResult AuditLogs()
        {
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public IActionResult Backups()
        {
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult CreateBackup(string? notes)
        {
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult RestoreBackup(int backupId, string confirmationText)
        {
            return RedirectToAction(nameof(Users));
        }
    }
}

