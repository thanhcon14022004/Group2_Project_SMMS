using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? role, 
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _authService.GetUsersPagedAsync(role, search, page, pageSize);
            return Ok(ApiResponse<PagedResult<UserDto>>.Ok(pagedResult));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));

            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var user = await _authService.CreateUserAsync(request, performedBy);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse<UserDto>.Ok(user, "Tạo tài khoản thành công."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi tạo người dùng: " + ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var user = await _authService.UpdateUserAsync(id, request, performedBy);
                if (user == null)
                    return NotFound(ApiResponse.ErrorResult("Không tìm thấy người dùng."));

                return Ok(ApiResponse<UserDto>.Ok(user, "Cập nhật người dùng thành công."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
            {
                return BadRequest(ApiResponse.ErrorResult("Bạn không thể tự khóa tài khoản của chính mình đang đăng nhập."));
            }

            var performedBy = User.Identity?.Name ?? "Admin";
            var (success, msg) = await _authService.ToggleUserStatusAsync(id, request.IsActive, performedBy);
            if (!success)
                return BadRequest(ApiResponse.ErrorResult(msg));

            return Ok(ApiResponse.SuccessResult(msg));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == id)
            {
                return BadRequest(ApiResponse.ErrorResult("Bạn không thể tự xóa tài khoản của chính mình đang đăng nhập."));
            }

            var performedBy = User.Identity?.Name ?? "Admin";
            var (success, msg) = await _authService.DeleteUserAsync(id, performedBy);
            if (!success)
                return BadRequest(ApiResponse.ErrorResult(msg));

            return Ok(ApiResponse.SuccessResult(msg));
        }
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }
}

