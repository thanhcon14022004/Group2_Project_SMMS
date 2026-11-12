using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/class-transfers")]
    [Authorize]
    public class ClassTransfersController : ControllerBase
    {
        private readonly IClassTransferService _transferService;
        private readonly AppDbContext _db;

        public ClassTransfersController(IClassTransferService transferService, AppDbContext db)
        {
            _transferService = transferService;
            _db = db;
        }

        [HttpGet("eligibility")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckEligibility([FromQuery] int targetClassId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var check = await _transferService.CheckTransferEligibilityAsync(student.Id, targetClassId);
            return Ok(check);
        }

        [HttpPost]
        [HttpPost("request")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> RequestTransfer([FromBody] RequestClassTransferDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            try
            {
                var req = await _transferService.RequestTransferAsync(student.Id, request);
                return Ok(new { message = "Gửi yêu cầu chuyển lớp thành công.", req });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRequests([FromQuery] int? semesterId, [FromQuery] string? status)
        {
            var list = await _transferService.GetTransferRequestsAsync(semesterId, status);
            return Ok(list);
        }

        [HttpGet("requests/paged")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetRequestsPaged(
            [FromQuery] int? semesterId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int? studentUserId = null;
            if (User.IsInRole("Student"))
            {
                var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out var uid)) studentUserId = uid;
            }

            var paged = await _transferService.GetTransferRequestsPagedAsync(semesterId, status, studentUserId, page, pageSize);
            return Ok(ApiResponse<PagedResult<ClassTransferRequestDto>>.Ok(paged));
        }

        [HttpPost("process")]
        [HttpPost("{id}/process")]
        [HttpPost("requests/{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessRequest([FromBody] ProcessClassTransferRequestDto request, [FromRoute] int? id = null)
        {
            try
            {
                if (id.HasValue && request.RequestId == 0)
                {
                    request.RequestId = id.Value;
                }
                var username = User.Identity?.Name ?? "Admin";
                await _transferService.ProcessTransferRequestAsync(request, username);
                return Ok(new { message = "Xử lý yêu cầu chuyển lớp thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

