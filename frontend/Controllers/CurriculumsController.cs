using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class CurriculumsController : BaseController
    {
        public CurriculumsController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? majorId, int? semesterNumber, string? version, string? search, int page = 1, int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var configsTask = ApiClient.GetAsync<List<CurriculumViewModel>>("curriculums");
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");
            var specializationsTask = ApiClient.GetAsync<List<SpecializationViewModel>>("specializations");
            var subjectsTask = ApiClient.GetAsync<List<SubjectViewModel>>("subjects");

            await Task.WhenAll(configsTask, majorsTask, specializationsTask, subjectsTask);

            var majors = (await majorsTask) ?? new List<MajorViewModel>();
            var specializations = (await specializationsTask) ?? new List<SpecializationViewModel>();
            var subjects = (await subjectsTask) ?? new List<SubjectViewModel>();
            var configs = (await configsTask) ?? new List<CurriculumViewModel>();

            ViewBag.Majors = majors;
            ViewBag.Specializations = specializations;
            ViewBag.Subjects = subjects;

            // Lưu trạng thái bộ lọc
            ViewBag.CurrentMajorId = majorId;
            ViewBag.CurrentSemester = semesterNumber;
            ViewBag.CurrentVersion = version;
            ViewBag.CurrentSearch = search;

            var query = configs.AsEnumerable();

            if (majorId.HasValue && majorId.Value > 0)
            {
                query = query.Where(c => c.MajorId == majorId.Value || c.MajorId == null);
            }

            if (semesterNumber.HasValue && semesterNumber.Value > 0)
            {
                query = query.Where(c => c.SemesterNumber == semesterNumber.Value);
            }

            if (!string.IsNullOrWhiteSpace(version))
            {
                query = query.Where(c => string.Equals(c.Version, version.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.MajorCode != null && c.MajorCode.ToLower().Contains(term)) ||
                    (c.SpecializationCode != null && c.SpecializationCode.ToLower().Contains(term)) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)) ||
                    c.Subjects.Any(s => s.EffectiveCode.ToLower().Contains(term) || s.EffectiveName.ToLower().Contains(term))
                );
            }

            var allFiltered = query.ToList();
            var totalCount = allFiltered.Count;

            if (pageSize <= 0) pageSize = 20;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pagedConfigs = allFiltered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalRecords = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;

            return View(pagedConfigs);
        }

        [HttpGet]
        public async Task<IActionResult> Lookup(int? majorId, int? semesterNumber, int? specializationId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            ViewBag.Majors = await ApiClient.GetAsync<List<MajorViewModel>>("majors") ?? new List<MajorViewModel>();
            ViewBag.Specializations = await ApiClient.GetAsync<List<SpecializationViewModel>>("specializations") ?? new List<SpecializationViewModel>();

            var vm = new CurriculumLookupViewModel
            {
                MajorId = majorId ?? 1,
                SemesterNumber = semesterNumber ?? 1,
                SpecializationId = specializationId
            };

            if (majorId.HasValue)
            {
                var (success, msg, data) = await ApiClient.PostAsync<CurriculumLookupViewModel>("curriculums/lookup", new
                {
                    majorId = vm.MajorId,
                    semesterNumber = vm.SemesterNumber,
                    specializationId = vm.SpecializationId
                });

                if (data != null)
                {
                    vm.IsFound = data.IsFound;
                    vm.ConfigName = data.ConfigName;
                    vm.Version = data.Version;
                    vm.MajorCode = data.MajorCode;
                    vm.SpecializationCode = data.SpecializationCode;
                    vm.Message = data.Message;
                    vm.Subjects = data.Subjects;
                }
                else
                {
                    vm.Message = msg;
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string version, int? majorId, int semesterNumber, int? specializationId, string name, string? description, List<int> subjectIds)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("curriculums", new
            {
                version = version.Trim(),
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                semesterNumber,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                name = name.Trim(),
                description = description?.Trim(),
                subjectIds = subjectIds ?? new List<int>()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string version, int? majorId, int semesterNumber, int? specializationId, string name, string? description, bool isActive, List<int> subjectIds)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"curriculums/{id}", new
            {
                version = version?.Trim() ?? "2024.1",
                majorId = (majorId.HasValue && majorId.Value > 0) ? majorId : null,
                semesterNumber,
                specializationId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null,
                name = name?.Trim() ?? string.Empty,
                description = description?.Trim(),
                isActive,
                subjectIds = subjectIds ?? new List<int>()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync($"curriculums/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.DeleteAsync("curriculums");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BatchCreateSections(int administrativeClassId, int semesterId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("coursesections/batch-create-from-curriculum", new
            {
                administrativeClassId,
                semesterId
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction("Index", "CourseSections");
        }
    }
}

