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
        public async Task<IActionResult> Users(string? role, string? search, int page = 1, int pageSize = 10)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            string endpoint = $"users?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(role)) endpoint += $"&role={role}";
            if (!string.IsNullOrWhiteSpace(search)) endpoint += $"&search={Uri.EscapeDataString(search.Trim())}";

            var usersTask = ApiClient.GetAsync<PagedResult<UserItemViewModel>>(endpoint);
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");
            var specsTask = ApiClient.GetAsync<List<SpecializationViewModel>>("specializations");
            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>("subjects");
            var classesTask = ApiClient.GetAsync<List<ClassViewModel>>("administrativeclasses");

            await Task.WhenAll(usersTask, majorsTask, specsTask, subjectsTask, classesTask);

            ViewBag.Majors = (await majorsTask) ?? new List<MajorViewModel>();
            ViewBag.Specializations = (await specsTask) ?? new List<SpecializationViewModel>();
            ViewBag.Subjects = (await subjectsTask) ?? new List<SubjectViewModel>();
            ViewBag.Classes = (await classesTask) ?? new List<ClassViewModel>();

            ViewBag.CurrentRole = role ?? "";
            ViewBag.CurrentSearch = search ?? "";

            var pagedUsers = (await usersTask) ?? new PagedResult<UserItemViewModel>
            {
                Items = new List<UserItemViewModel>(),
                TotalCount = 0,
                PageIndex = page,
                PageSize = pageSize
            };

            return View(pagedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id, bool isActive)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var currentUserIdStr = HttpContext.Session.GetString("SMMS_USER_ID");
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
            {
                SetAlert("Bạn không thể tự khóa tài khoản của chính mình đang đăng nhập!", "danger");
                return RedirectToAction(nameof(Users));
            }

            var (success, msg) = await ApiClient.PatchAsync($"users/{id}/status", new { isActive });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string password, string fullName, string email, string? phoneNumber, string role, string? code, int? majorId, int? administrativeClassId, int? cohort, string? department, string? academicTitle)
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
                role,
                code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : null,
                majorId,
                administrativeClassId,
                cohort,
                department = !string.IsNullOrWhiteSpace(department) ? department.Trim() : null,
                academicTitle = !string.IsNullOrWhiteSpace(academicTitle) ? academicTitle.Trim() : null
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

            var currentUserIdStr = HttpContext.Session.GetString("SMMS_USER_ID");
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
            {
                SetAlert("Bạn không thể tự xóa tài khoản của chính mình đang đăng nhập!", "danger");
                return RedirectToAction(nameof(Users));
            }

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

        #region Môn đăng ký của Giảng viên (AJAX)

        [HttpGet]
        public async Task<IActionResult> GetLecturerSubjects(int lecturerId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập." });

            var list = await ApiClient.GetAsync<List<LecturerSubjectViewModel>>($"lecturers/{lecturerId}/subjects");
            return Json(new { success = true, data = list ?? new List<LecturerSubjectViewModel>() });
        }

        [HttpPost]
        public async Task<IActionResult> AddBatchLecturerSubjects([FromBody] BatchLecturerSubjectInputModel model)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập." });

            if (model == null || model.LecturerId <= 0 || model.SubjectIds == null || model.SubjectIds.Count == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một môn học để đăng ký." });
            }

            var (success, msg, data) = await ApiClient.PostAsync<object>($"lecturers/{model.LecturerId}/batch-subjects", new
            {
                majorId = model.MajorId,
                subjectIds = model.SubjectIds,
                notes = model.Notes
            });

            return Json(new { success, message = msg, data });
        }

        [HttpPost]
        public async Task<IActionResult> AddLecturerSubject([FromBody] AddLecturerSubjectInputModel model)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập." });

            if (model == null || model.LecturerId <= 0 || model.MajorId <= 0 || model.SpecializationId <= 0 || model.SubjectId <= 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn đầy đủ Ngành, Ngành hẹp và Môn học." });
            }

            var (success, msg, data) = await ApiClient.PostAsync<LecturerSubjectViewModel>($"lecturers/{model.LecturerId}/subjects", new
            {
                majorId = model.MajorId,
                specializationId = model.SpecializationId,
                subjectId = model.SubjectId,
                notes = model.Notes
            });

            return Json(new { success, message = msg, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLecturerSubject([FromBody] UpdateLecturerSubjectInputModel model)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập." });

            if (model == null || model.LecturerId <= 0 || model.Id <= 0 || model.MajorId <= 0 || model.SpecializationId <= 0 || model.SubjectId <= 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn đầy đủ Ngành, Ngành hẹp và Môn học." });
            }

            var (success, msg) = await ApiClient.PutAsync($"lecturers/{model.LecturerId}/subjects/{model.Id}", new
            {
                majorId = model.MajorId,
                specializationId = model.SpecializationId,
                subjectId = model.SubjectId,
                notes = model.Notes
            });

            return Json(new { success, message = msg });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLecturerSubject(int lecturerId, int subjectRegId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập." });

            var (success, msg) = await ApiClient.DeleteAsync($"lecturers/{lecturerId}/subjects/{subjectRegId}");
            return Json(new { success, message = msg });
        }

        #endregion
    }
}

