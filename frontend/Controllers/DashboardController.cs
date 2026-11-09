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

            var dashboard = new DashboardViewModel();

            if (CurrentUser?.Role == "Lecturer")
            {
                var lecturerDashboard = await ApiClient.GetAsync<LecturerDashboardViewModel>("reports/lecturer-dashboard");
                dashboard.LecturerDashboard = lecturerDashboard;
                if (lecturerDashboard != null)
                {
                    dashboard.ActiveSemesterName = lecturerDashboard.ActiveSemesterName;
                    dashboard.TotalSections = lecturerDashboard.TotalAssignedSections;
                    dashboard.AverageGpa = lecturerDashboard.AverageTeachingScore;
                }
            }
            else
            {
                var adminDashboard = await ApiClient.GetAsync<DashboardViewModel>("reports/dashboard");
                if (adminDashboard != null)
                {
                    dashboard = adminDashboard;
                }
            }

            return View(dashboard);
        }
    }
}

