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
        public async Task<IActionResult> Index(int? classId, string? classSearch, int? sectionId, int? semesterId)
        {
            var authCheck = CheckAuth("Admin", "Lecturer");
            if (authCheck != null) return authCheck;

            bool isLecturer = CurrentUser?.Role == "Lecturer";

            if (isLecturer)
            {
                // Luồng nghiệp vụ dành riêng cho GIẢNG VIÊN:
                // Tải danh sách lớp học phần do chính giảng viên này phụ trách
                var lecturerSections = await ApiClient.GetAsync<List<CourseSectionViewModel>>("course-sections") ?? new List<CourseSectionViewModel>();
                ViewBag.LecturerSections = lecturerSections;

                // Tự động chọn lớp đầu tiên nếu chưa chọn sectionId
                if (!sectionId.HasValue && lecturerSections.Any())
                {
                    sectionId = lecturerSections.First().Id;
                }

                GradeSheetViewModel? lecturerSheet = null;
                if (sectionId.HasValue && sectionId.Value > 0)
                {
                    lecturerSheet = await ApiClient.GetAsync<GradeSheetViewModel>($"grades/section/{sectionId.Value}");
                    ViewBag.SelectedSectionId = sectionId.Value;
                }

                var lecturerModel = lecturerSheet ?? new GradeSheetViewModel
                {
                    CourseSectionId = sectionId ?? 0
                };

                return View(lecturerModel);
            }

            // Luồng nghiệp vụ dành cho QUẢN TRỊ VIÊN (Admin):
            // 1. Tải danh sách học kỳ và lớp hành chính
            var semestersTask = ApiClient.GetAsync<List<SemesterViewModel>>("semesters");
            var classesTask = ApiClient.GetAsync<List<ClassViewModel>>("classes");

            await Task.WhenAll(semestersTask, classesTask);

            var semesters = await semestersTask ?? new List<SemesterViewModel>();
            var allClasses = await classesTask ?? new List<ClassViewModel>();

            ViewBag.Semesters = semesters;
            ViewBag.AllClasses = allClasses;
            ViewBag.ClassSearch = classSearch;
            ViewBag.SelectedSemesterId = semesterId;

            // 2. Xử lý tìm kiếm theo tên lớp / mã lớp
            var displayClasses = allClasses;
            if (!string.IsNullOrWhiteSpace(classSearch))
            {
                var kw = classSearch.Trim().ToLower();
                displayClasses = allClasses.Where(c =>
                    c.Code.ToLower().Contains(kw) ||
                    c.Name.ToLower().Contains(kw) ||
                    (!string.IsNullOrEmpty(c.MajorCode) && c.MajorCode.ToLower().Contains(kw))
                ).ToList();

                // Nếu người dùng nhập tìm kiếm và chưa chọn classId cụ thể, tự động chọn lớp khớp nhất
                if (!classId.HasValue && displayClasses.Any())
                {
                    var exactMatch = displayClasses.FirstOrDefault(c => c.Code.Equals(classSearch.Trim(), StringComparison.OrdinalIgnoreCase));
                    classId = exactMatch != null ? exactMatch.Id : displayClasses.First().Id;
                }
            }
            ViewBag.AdministrativeClasses = displayClasses;

            // 3. Tải thông tin lớp và danh sách các môn học của lớp đó đang học
            ClassSectionsResponseViewModel? classDetail = null;
            if (classId.HasValue && classId.Value > 0)
            {
                classDetail = await ApiClient.GetAsync<ClassSectionsResponseViewModel>($"classes/{classId.Value}/sections");
                ViewBag.SelectedClass = classDetail;
                ViewBag.SelectedClassId = classId.Value;
            }

            // 4. Nếu có chọn môn học cụ thể (sectionId), tải bảng điểm của môn học đó
            GradeSheetViewModel? gradeSheet = null;
            if (sectionId.HasValue && sectionId.Value > 0)
            {
                gradeSheet = await ApiClient.GetAsync<GradeSheetViewModel>($"grades/section/{sectionId.Value}");
                if (gradeSheet != null)
                {
                    ViewBag.SelectedSectionId = sectionId.Value;

                    // Nếu chưa có classId nhưng lớp học phần có liên kết lớp hành chính, đồng bộ classDetail
                    if (!classId.HasValue && gradeSheet.AdministrativeClassId.HasValue)
                    {
                        classId = gradeSheet.AdministrativeClassId.Value;
                        classDetail = await ApiClient.GetAsync<ClassSectionsResponseViewModel>($"classes/{classId.Value}/sections");
                        ViewBag.SelectedClass = classDetail;
                        ViewBag.SelectedClassId = classId.Value;
                    }
                }
            }

            var model = gradeSheet ?? new GradeSheetViewModel
            {
                CourseSectionId = sectionId ?? 0,
                AdministrativeClassId = classId,
                AdministrativeClassCode = classDetail?.ClassCode
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int courseSectionId, int? classId, List<StudentGradeEntryModel> grades)
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
            return RedirectToAction(nameof(Index), new { classId, sectionId = courseSectionId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int sectionId, int? classId, bool isLocked)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = isLocked
                ? await ApiClient.PostAsync<object>($"grades/{sectionId}/lock", new { })
                : await ApiClient.PostAsync<object>($"grades/{sectionId}/unlock", new { });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index), new { classId, sectionId });
        }

        [HttpGet]
        public async Task<IActionResult> Transcript(int? studentId, string? semesterCode, string? search, int page = 1, int pageSize = 20)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var queryParts = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (studentId.HasValue && studentId.Value > 0) queryParts.Add($"studentId={studentId.Value}");
            if (!string.IsNullOrWhiteSpace(semesterCode)) queryParts.Add($"semesterCode={Uri.EscapeDataString(semesterCode.Trim())}");
            if (!string.IsNullOrWhiteSpace(search)) queryParts.Add($"search={Uri.EscapeDataString(search.Trim())}");

            ViewBag.CurrentStudentId = studentId;
            ViewBag.CurrentSemesterCode = semesterCode ?? "";
            ViewBag.CurrentSearch = search ?? "";

            var transcript = await ApiClient.GetAsync<StudentTranscriptPagedViewModel>($"grades/transcript/paged?{string.Join("&", queryParts)}")
                ?? new StudentTranscriptPagedViewModel();

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
