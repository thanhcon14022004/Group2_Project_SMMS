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
        public async Task<IActionResult> AuditLogs(string? action)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            string endpoint = "auditlogs";
            if (!string.IsNullOrWhiteSpace(action)) endpoint += $"?action={action}";

            var logs = await ApiClient.GetAsync<List<AuditLogItemViewModel>>(endpoint) ?? new List<AuditLogItemViewModel>();
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Backups()
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var backups = await ApiClient.GetAsync<List<BackupItemViewModel>>("backups") ?? new List<BackupItemViewModel>();
            return View(backups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup(string? notes)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("backups/create", new { notes });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Backups));
        }

        [HttpPost]
        public async Task<IActionResult> RestoreBackup(int backupId, string confirmationText)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("backups/restore", new
            {
                backupId,
                confirmationText
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Backups));
        }
    }
}

