using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                if (response == null)
                {
                    return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
                }
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ khi đăng nhập.", error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(user);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? search)
        {
            var users = await _authService.GetUsersAsync(role, search);
            return Ok(users);
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var user = await _authService.CreateUserAsync(request, performedBy);
                return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
            {
                return BadRequest(new { message = "Bạn không thể tự khóa tài khoản của chính mình đang đăng nhập." });
            }

            var performedBy = User.Identity?.Name ?? "Admin";
            var (success, msg) = await _authService.ToggleUserStatusAsync(id, request.IsActive, performedBy);
            if (!success) return BadRequest(new { message = msg });

            return Ok(new { message = msg });
        }
    }
}

