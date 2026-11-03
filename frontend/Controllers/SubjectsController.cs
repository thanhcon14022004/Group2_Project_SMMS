using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class SubjectsController : BaseController
    {
        public SubjectsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] int? majorId,
            [FromQuery] int? specializationId,
            [FromQuery] int? credits,
            [FromQuery] string? orderby)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filters.Add($"(contains(tolower(Code), '{s}') or contains(tolower(Name), '{s}'))");
            }
            if (majorId.HasValue && majorId.Value > 0)
            {
                filters.Add($"MajorId eq {majorId.Value}");
            }
            if (specializationId.HasValue && specializationId.Value > 0)
            {
                filters.Add($"SpecializationId eq {specializationId.Value}");
            }
            if (credits.HasValue && credits.Value > 0)
            {
                if (credits.Value >= 5)
                    filters.Add("Credits ge 5");
                else
                    filters.Add($"Credits eq {credits.Value}");
            }

            var queryParams = new List<string>();
            if (filters.Any())
            {
                queryParams.Add($"$filter={string.Join(" and ", filters)}");
            }
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                queryParams.Add($"$orderby={orderby}");
            }

            var subjectEndpoint = "subjects" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");

            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>(subjectEndpoint);
            var specializationsTask = ApiClient.GetAsync<List<SpecializationViewModel>>("specializations");
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");

            await Task.WhenAll(subjectsTask, specializationsTask, majorsTask);

            ViewBag.Majors = (await majorsTask) ?? new List<MajorViewModel>();
            ViewBag.Specializations = (await specializationsTask) ?? new List<SpecializationViewModel>();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentMajorId = majorId;
            ViewBag.CurrentSpecializationId = specializationId;
            ViewBag.CurrentCredits = credits;
            ViewBag.CurrentOrderBy = orderby;
            ViewBag.CurrentODataQuery = queryParams.Any() ? string.Join("&", queryParams) : "";

            var subjects = (await subjectsTask) ?? new List<SubjectViewModel>();
            return View(subjects);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string name, int credits, string? description, int? majorId, int? specializationId, bool isAdminOnlySubject = false)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, createdData) = await ApiClient.PostAsync<SubjectViewModel>("subjects", new
            {
                code = code.Trim().ToUpper(),
                name = name.Trim(),
                credits = credits > 0 ? credits : 3,
                description,
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                isActive = true
            });

            if (success && createdData != null && isAdminOnlySubject)
            {
                // Tự động cấu hình đầu điểm 100% Chỉ Admin cho môn này
                await ApiClient.PostAsync<object>($"subjects/{createdData.Id}/components", new
                {
                    components = new List<object>
                    {
                        new
                        {
                            subjectId = createdData.Id,
                            name = "Đánh giá & Bảo vệ tốt nghiệp (Hội đồng / Admin)",
                            weight = 100m,
                            scale = 10m,
                            isAdminOnly = true,
                            orderIndex = 1
                        }
                    }
                });
            }

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SaveComponents(int subjectId, List<SubjectGradeComponentViewModel> components)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            if (components == null || !components.Any())
            {
                SetAlert("Danh sách đầu điểm không được để trống.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var totalWeight = components.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 100m) > 0.01m)
            {
                SetAlert($"Tổng tỷ trọng các đầu điểm phải bằng đúng 100%. Hiện tại là {totalWeight}%.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var (success, msg) = await ApiClient.PutAsync($"subjects/{subjectId}/components", new
            {
                components = components.Select(c => new
                {
                    id = c.Id,
                    name = c.Name.Trim(),
                    weight = c.Weight,
                    scale = c.Scale > 0 ? c.Scale : 10m,
                    isAdminOnly = c.IsAdminOnly
                }).ToList()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddPrerequisite(int subjectId, int prerequisiteSubjectId, decimal minScore = 5.0m)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>($"subjects/{subjectId}/prerequisites", new
            {
                subjectId,
                prerequisiteSubjectId,
                minScore = minScore > 0 ? minScore : 5.0m
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemovePrerequisite(int subjectId, int prerequisiteSubjectId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"subjects/{subjectId}/prerequisites/{prerequisiteSubjectId}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, int credits, string? description, int? majorId, int? specializationId, bool isActive)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"subjects/{id}", new
            {
                name = name.Trim(),
                credits = credits > 0 ? credits : 3,
                description,
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                isActive
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"subjects/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }
    }
}

