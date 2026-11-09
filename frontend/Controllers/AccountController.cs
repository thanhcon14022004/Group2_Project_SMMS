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
    }
}

