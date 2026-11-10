using Microsoft.AspNetCore.Mvc;
using project_PRN232.Frontend.Models;
using project_PRN232.Frontend.Services;

namespace project_PRN232.Frontend.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(ApiClientService apiClient) : base(apiClient)
        {
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { Username = "admin", Password = "Admin@123" });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await ApiClient.LoginAsync(model.Username, model.Password);
                if (user != null)
                {
                    SetAlert($"Xin chào, {user.FullName}! Đăng nhập thành công.", "success");
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Dashboard");
                }

                model.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickLogin(string username, string password = "Admin@123")
        {
            try
            {
                var user = await ApiClient.LoginAsync(username, password);
                if (user != null)
                {
                    SetAlert($"Đăng nhập thành công với vai trò {user.Role}.", "success");
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult SetClientSession([FromBody] CurrentUserViewModel user)
        {
            if (user != null && !string.IsNullOrWhiteSpace(user.Token))
            {
                HttpContext.Session.SetString("SMMS_JWT_TOKEN", user.Token);
                HttpContext.Session.SetString("SMMS_USER_INFO", System.Text.Json.JsonSerializer.Serialize(user));
                HttpContext.Session.SetString("SMMS_USER_ROLE", user.Role ?? "");
                HttpContext.Session.SetString("SMMS_USER_NAME", user.FullName ?? "");
                HttpContext.Session.SetString("SMMS_USER_ID", user.UserId.ToString());
                HttpContext.Session.SetString("SMMS_USER_USERNAME", user.Username ?? "");

                Response.Cookies.Append("SMMS_JWT_TOKEN", user.Token, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                SetAlert($"Xin chào, {user.FullName}! Đăng nhập thành công.", "success");
                return Ok(new { success = true });
            }
            return BadRequest("Dữ liệu người dùng hoặc token không hợp lệ.");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            ApiClient.Logout();
            SetAlert("Đã đăng xuất khỏi hệ thống thành công.", "info");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var user = await ApiClient.GetAsync<UserProfileViewModel>("auth/me");
            if (user == null)
            {
                SetAlert("Không thể tải thông tin hồ sơ tài khoản.", "danger");
                return RedirectToAction("Index", "Dashboard");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                SetAlert("Họ và tên không được để trống.", "warning");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                SetAlert("Địa chỉ Email không được để trống.", "warning");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    SetAlert("Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu.", "warning");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    SetAlert("Mật khẩu mới và mật khẩu xác nhận không khớp.", "warning");
                    return View(model);
                }

                if (model.NewPassword.Length < 6)
                {
                    SetAlert("Mật khẩu mới phải có tối thiểu 6 ký tự.", "warning");
                    return View(model);
                }
            }

            var requestBody = new
            {
                fullName = model.FullName.Trim(),
                email = model.Email.Trim(),
                phoneNumber = model.PhoneNumber?.Trim(),
                department = model.Department?.Trim(),
                academicTitle = model.AcademicTitle?.Trim(),
                currentPassword = model.CurrentPassword,
                newPassword = model.NewPassword
            };

            var (success, message) = await ApiClient.PutAsync("auth/profile", requestBody);
            if (!success && CurrentUser?.Role == "Admin")
            {
                var fallbackRes = await ApiClient.PutAsync($"users/{CurrentUser.UserId}", requestBody);
                if (fallbackRes.Success)
                {
                    success = true;
                    message = fallbackRes.Message;
                }
            }

            if (success)
            {
                HttpContext.Session.SetString("SMMS_USER_NAME", model.FullName.Trim());

                var updatedUser = await ApiClient.GetAsync<UserProfileViewModel>("auth/me");
                if (updatedUser != null)
                {
                    var currentUser = ApiClient.GetCurrentUser();
                    if (currentUser != null)
                    {
                        currentUser.FullName = updatedUser.FullName;
                        HttpContext.Session.SetString("SMMS_USER_INFO", System.Text.Json.JsonSerializer.Serialize(currentUser));
                    }
                    SetAlert("Cập nhật hồ sơ cá nhân thành công!", "success");
                    return View(updatedUser);
                }

                SetAlert("Cập nhật hồ sơ cá nhân thành công!", "success");
                return RedirectToAction("Profile");
            }

            SetAlert(message ?? "Cập nhật hồ sơ thất bại.", "danger");
            return View(model);
        }
    }
}

