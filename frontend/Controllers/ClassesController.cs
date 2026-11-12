using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class ClassesController : BaseController
    {
        public ClassesController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] int? majorId,
            [FromQuery] int? specializationId,
            [FromQuery] int? currentSemester,
            [FromQuery] int? cohort,
            [FromQuery] string? orderby,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin", "Student");
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
            if (currentSemester.HasValue && currentSemester.Value > 0)
            {
                filters.Add($"CurrentSemester eq {currentSemester.Value}");
            }
            if (cohort.HasValue && cohort.Value > 0)
            {
                filters.Add($"Cohort eq {cohort.Value}");
            }

            var odataPreviewParams = new List<string>();
            if (filters.Any())
            {
                odataPreviewParams.Add($"$filter={string.Join(" and ", filters)}");
            }
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                odataPreviewParams.Add($"$orderby={orderby}");
            }

            var pagedQuery = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(search)) pagedQuery.Add($"search={Uri.EscapeDataString(search.Trim())}");
            if (majorId.HasValue && majorId.Value > 0) pagedQuery.Add($"majorId={majorId.Value}");
            if (specializationId.HasValue && specializationId.Value > 0) pagedQuery.Add($"specializationId={specializationId.Value}");
            if (currentSemester.HasValue && currentSemester.Value > 0) pagedQuery.Add($"currentSemester={currentSemester.Value}");
            if (cohort.HasValue && cohort.Value > 0) pagedQuery.Add($"cohort={cohort.Value}");
            if (!string.IsNullOrWhiteSpace(orderby)) pagedQuery.Add($"orderby={Uri.EscapeDataString(orderby)}");

            var classesTask = ApiClient.GetAsync<PagedResult<ClassViewModel>>($"classes/paged?{string.Join("&", pagedQuery)}");
            var majorsTask = ApiClient.GetAsync<List<MajorViewModel>>("majors");
            var semestersTask = ApiClient.GetAsync<List<SemesterViewModel>>("semesters");
            var specializationsTask = ApiClient.GetAsync<List<SpecializationViewModel>>("specializations");

            await Task.WhenAll(classesTask, majorsTask, semestersTask, specializationsTask);

            ViewBag.Majors = (await majorsTask) ?? new List<MajorViewModel>();
            ViewBag.Semesters = (await semestersTask) ?? new List<SemesterViewModel>();
            ViewBag.Specializations = (await specializationsTask) ?? new List<SpecializationViewModel>();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentMajorId = majorId;
            ViewBag.CurrentSpecializationId = specializationId;
            ViewBag.CurrentSemesterNum = currentSemester;
            ViewBag.CurrentCohort = cohort;
            ViewBag.CurrentOrderBy = orderby;
            ViewBag.CurrentODataQuery = odataPreviewParams.Any() ? string.Join("&", odataPreviewParams) : "";

            var classes = (await classesTask) ?? new PagedResult<ClassViewModel> { PageIndex = page, PageSize = pageSize };

            return View(classes);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, int majorId, int currentSemester, string? academicYear, int? specializationId, string? name, int? cohort)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var trimmedCode = (code ?? "").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(trimmedCode))
            {
                SetAlert("Mã lớp không được để trống.", "danger");
                return RedirectToAction(nameof(Index));
            }

            // Tự động suy ra Khóa học từ mã lớp (VD: SE1801 -> 18, AI1902 -> 19)
            int autoCohort = cohort.HasValue && cohort.Value > 0 ? cohort.Value : 18;
            var matchDigits = System.Text.RegularExpressions.Regex.Match(trimmedCode, @"\d{2}");
            if (matchDigits.Success && int.TryParse(matchDigits.Value, out int extractedCohort))
            {
                autoCohort = extractedCohort;
            }

            // Tên lớp mặc định nếu người dùng không nhập
            var className = !string.IsNullOrWhiteSpace(name) ? name.Trim() : $"Lớp {trimmedCode}";

            // Học phần / Mùa học (VD: Summer 26, Spring 26, Fall 26)
            var semesterSeason = !string.IsNullOrWhiteSpace(academicYear) ? academicYear.Trim() : "Summer 26";
            var specId = (specializationId.HasValue && specializationId.Value > 0) ? specializationId : null;

            var (success, msg, data) = await ApiClient.PostAsync<ClassCreateResponseViewModel>("classes", new
            {
                code = trimmedCode,
                name = className,
                majorId,
                specializationId = specId,
                cohort = autoCohort,
                academicYear = semesterSeason,
                currentSemester = currentSemester > 0 ? currentSemester : 1,
                isActive = true
            });

            if (success && data != null && !string.IsNullOrWhiteSpace(data.Message))
            {
                SetAlert(data.Message, "success");
            }
            else
            {
                SetAlert(msg, success ? "success" : "danger");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int id)
        {
            var authCheck = CheckAuth("Admin", "Student");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var res = await ApiClient.GetAsync<ClassSectionsResponseViewModel>($"classes/{id}/sections");
            if (res == null)
            {
                return Json(new { success = false, message = "Không thể tải danh sách môn học của lớp." });
            }

            return Json(new { success = true, data = res });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents(int id)
        {
            var authCheck = CheckAuth("Admin", "Student");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var res = await ApiClient.GetAsync<ClassStudentsResponseViewModel>($"classes/{id}");
            if (res == null)
            {
                return Json(new { success = false, message = "Không thể tải danh sách sinh viên của lớp." });
            }

            return Json(new { success = true, data = res });
        }

        [HttpGet]
        public async Task<IActionResult> GetLecturersAssignment(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var res = await ApiClient.GetAsync<ClassLecturersAssignmentViewModel>($"classes/{id}/lecturers-assignment");
            if (res == null)
            {
                return Json(new { success = false, message = "Không thể tải dữ liệu phân bổ giáo viên của lớp." });
            }

            return Json(new { success = true, data = res });
        }

        [HttpPost]
        public async Task<IActionResult> SaveLecturersAssignment([FromBody] AssignClassLecturersInputModel model)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            if (model == null || model.ClassId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var (success, msg, data) = await ApiClient.PostAsync<object>($"classes/{model.ClassId}/assign-lecturers", new
            {
                assignments = model.Assignments
            });

            return Json(new { success, message = msg });
        }

        [HttpPost]
        public async Task<IActionResult> AutoAssignLecturers(int? classId, int? semesterId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var (success, msg, data) = await ApiClient.PostAsync<AutoAssignLecturersResponseViewModel>("classes/auto-assign-lecturers", new
            {
                classId,
                semesterId
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("json") == true)
            {
                return Json(new { success, message = data?.Message ?? msg });
            }

            SetAlert(data?.Message ?? msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Allocations(int? semesterId, int page = 1, int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var allSemesters = await ApiClient.GetAsync<List<SemesterViewModel>>("semesters") ?? new List<SemesterViewModel>();
            var activeSemesters = allSemesters.Where(s => s.Status == "Active").ToList();
            ViewBag.Semesters = activeSemesters.Any() ? activeSemesters : allSemesters;

            var selectedSemesterId = semesterId ?? activeSemesters.FirstOrDefault()?.Id ?? allSemesters.FirstOrDefault()?.Id ?? 0;

            AllocationStatusPagedViewModel? pagedStatus = null;
            if (selectedSemesterId > 0)
            {
                pagedStatus = await ApiClient.GetAsync<AllocationStatusPagedViewModel>($"allocations/status/{selectedSemesterId}/paged?page={page}&pageSize={pageSize}");
            }

            AllocationViewModel model;
            if (pagedStatus == null)
            {
                model = new AllocationViewModel
                {
                    SemesterId = selectedSemesterId,
                    ClassSummaries = new List<ClassAllocationSummaryViewModel>(),
                    Allocations = new List<AllocationItemViewModel>(),
                    Exceptions = new List<UnallocatedExceptionViewModel>()
                };
                ViewBag.PagedClassSummaries = new PagedResult<ClassAllocationSummaryViewModel> { PageIndex = page, PageSize = pageSize };
            }
            else
            {
                var pagedSummaries = pagedStatus.PagedClassSummaries ?? new PagedResult<ClassAllocationSummaryViewModel>();
                model = new AllocationViewModel
                {
                    SemesterId = selectedSemesterId,
                    SemesterCode = pagedStatus.SemesterCode,
                    TotalEligibleStudents = pagedStatus.TotalEligibleStudents,
                    TotalEnrolledSections = pagedStatus.TotalEnrolledSections,
                    AutoCreatedStudentsCount = pagedStatus.AutoCreatedStudentsCount,
                    BlockedPrerequisiteCount = pagedStatus.BlockedPrerequisiteCount,
                    CapacityOverflowCount = pagedStatus.CapacityOverflowCount,
                    ClassSummaries = pagedSummaries.Items ?? new List<ClassAllocationSummaryViewModel>(),
                    Allocations = new List<AllocationItemViewModel>(),
                    Exceptions = new List<UnallocatedExceptionViewModel>()
                };
                ViewBag.PagedClassSummaries = pagedSummaries;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RunAllocation(int semesterId, int? majorId, int? administrativeClassId)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, data) = await ApiClient.PostAsync<AllocationViewModel>("allocations/run", new
            {
                semesterId,
                majorId,
                administrativeClassId,
                curriculumVersion = "2024.1"
            });

            var allSemesters = await ApiClient.GetAsync<List<SemesterViewModel>>("semesters") ?? new List<SemesterViewModel>();
            var activeSemesters = allSemesters.Where(s => s.Status == "Active").ToList();
            ViewBag.Semesters = activeSemesters.Any() ? activeSemesters : allSemesters;

            if (success && data != null)
            {
                data.ClassSummaries ??= new List<ClassAllocationSummaryViewModel>();
                data.Allocations ??= new List<AllocationItemViewModel>();
                data.Exceptions ??= new List<UnallocatedExceptionViewModel>();
                SetAlert($"Đã phân bổ thành công {data.TotalEligibleStudents} sinh viên vào các lớp hành chính ({data.AutoCreatedStudentsCount} sinh viên tự động tạo mới vào CSDL để đạt chuẩn 28-31 SV/lớp)!", "success");
                return View("Allocations", data);
            }

            SetAlert(string.IsNullOrWhiteSpace(msg) ? "Phân bổ sinh viên thất bại. Vui lòng kiểm tra lại cấu hình học kỳ và lớp hành chính." : msg, "danger");
            return View("Allocations", new AllocationViewModel 
            { 
                SemesterId = semesterId,
                ClassSummaries = new List<ClassAllocationSummaryViewModel>(),
                Allocations = new List<AllocationItemViewModel>(),
                Exceptions = new List<UnallocatedExceptionViewModel>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> SyncCurriculum(int id)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>($"classes/{id}/sync-curriculum", new { });
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? name, int? cohort, string? academicYear, int currentSemester, int? specializationId, bool isActive)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg) = await ApiClient.PutAsync($"classes/{id}", new
            {
                name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : null,
                cohort = cohort.HasValue && cohort.Value > 0 ? cohort.Value : (int?)null,
                academicYear = !string.IsNullOrWhiteSpace(academicYear) ? academicYear.Trim() : null,
                currentSemester = currentSemester > 0 ? currentSemester : 1,
                specializationId,
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

            var (success, msg) = await ApiClient.DeleteAsync($"classes/{id}");
            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Transfers(string? status, int page = 1, int pageSize = 20)
        {
            var authCheck = CheckAuth("Admin", "Student");
            if (authCheck != null) return authCheck;

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status.Trim())}");

            var requestsTask = ApiClient.GetAsync<PagedResult<ClassTransferViewModel>>($"classtransfers/requests/paged?{string.Join("&", queryParams)}");
            var classesTask = ApiClient.GetAsync<List<ClassViewModel>>("classes");

            await Task.WhenAll(requestsTask, classesTask);

            ViewBag.Classes = (await classesTask) ?? new List<ClassViewModel>();
            ViewBag.CurrentStatus = status;

            var requests = (await requestsTask) ?? new PagedResult<ClassTransferViewModel> { PageIndex = page, PageSize = pageSize };
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> RequestTransfer(int targetClassId, string reason)
        {
            var authCheck = CheckAuth("Student");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>("classtransfers/request", new
            {
                targetClassId,
                reason = reason.Trim()
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Transfers));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(int requestId, bool approve, string? note)
        {
            var authCheck = CheckAuth("Admin");
            if (authCheck != null) return authCheck;

            var (success, msg, _) = await ApiClient.PostAsync<object>($"classtransfers/requests/{requestId}/process", new
            {
                approve,
                note
            });

            SetAlert(msg, success ? "success" : "danger");
            return RedirectToAction(nameof(Transfers));
        }
    }
}

