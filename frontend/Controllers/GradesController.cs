using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class GradesController : BaseController
    {
        public GradesController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? sectionId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            ViewBag.Sections = await ApiClient.GetAsync<List<CourseSectionViewModel>>("course-sections") ?? new List<CourseSectionViewModel>();

            if (!sectionId.HasValue && ViewBag.Sections.Count > 0)
            {
                sectionId = ((List<CourseSectionViewModel>)ViewBag.Sections)[0].Id;
            }

            if (sectionId.HasValue)
            {
                var gradeSheet = await ApiClient.GetAsync<GradeSheetViewModel>($"grades/section/{sectionId.Value}");
                ViewBag.SelectedSectionId = sectionId.Value;
                return View(gradeSheet ?? new GradeSheetViewModel { CourseSectionId = sectionId.Value });
            }

            return View(new GradeSheetViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(int courseSectionId, List<StudentGradeEntryModel> grades)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("grades", new
            {
                courseSectionId,
                grades = grades.Select(g => new
                {
                    studentId = g.StudentId,
                    subjectGradeComponentId = g.ComponentId,
                    score = g.Score
                }).ToList()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { sectionId = courseSectionId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int sectionId, bool isLocked)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = isLocked
                ? await ApiClient.PostAsync<object>($"grades/{sectionId}/lock", new { })
                : await ApiClient.PostAsync<object>($"grades/{sectionId}/unlock", new { });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { sectionId });
        }

        [HttpGet]
        public async Task<IActionResult> Transcript(int? studentId)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            string endpoint = studentId.HasValue ? $"students/{studentId.Value}/transcript" : "grades/transcript";
            var transcript = await ApiClient.GetAsync<TranscriptViewModel>(endpoint) ?? new TranscriptViewModel();

            return View(transcript);
        }
    }

    public class StudentGradeEntryModel
    {
        public int StudentId { get; set; }
        public int ComponentId { get; set; }
        public decimal? Score { get; set; }
    }
}
