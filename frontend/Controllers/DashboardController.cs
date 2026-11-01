using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var dashboard = await ApiClient.GetAsync<DashboardViewModel>("reports/dashboard") ?? new DashboardViewModel();
            return View(dashboard);
        }
    }
}

