using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApiClientService ApiClient;

        protected BaseController(ApiClientService apiClient)
        {
            ApiClient = apiClient;
        }

        protected CurrentUserViewModel? CurrentUser => ApiClient.GetCurrentUser();

        protected bool IsAuthenticated => CurrentUser != null;

        protected IActionResult? CheckAuth(params string[] allowedRoles)
        {
            if (!IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (allowedRoles.Length > 0 && !allowedRoles.Contains(CurrentUser!.Role))
            {
                TempData["Error"] = "Bạn không có quyền truy cập vào chức năng này.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.CurrentUser = CurrentUser;
            return null;
        }

        protected void SetAlert(string message, string type = "success")
        {
            TempData[type == "success" ? "Success" : (type == "danger" || type == "error" ? "Error" : "Warning")] = message;
        }
    }
}

