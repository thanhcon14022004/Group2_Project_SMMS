using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationsController : ControllerBase
    {
        private readonly IStudentAllocationService _allocationService;

        public AllocationsController(IStudentAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        [HttpPost("run")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunAllocation([FromBody] RunAllocationRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var result = await _allocationService.RunAllocationAsync(request, performedBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exceptions/{semesterId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetExceptions(int semesterId)
        {
            var exceptions = await _allocationService.GetUnallocatedExceptionsAsync(semesterId);
            return Ok(exceptions);
        }

        [HttpPost("manual-enroll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManualEnroll([FromQuery] int courseSectionId, [FromQuery] int studentId)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                await _allocationService.EnrollSingleStudentAsync(courseSectionId, studentId, performedBy);
                return Ok(new { message = "Đã xếp sinh viên vào lớp học phần thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("manual-remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManualRemove([FromQuery] int courseSectionId, [FromQuery] int studentId)
        {
            var performedBy = User.Identity?.Name ?? "Admin";
            var success = await _allocationService.RemoveStudentFromSectionAsync(courseSectionId, studentId, performedBy);
            if (!success) return NotFound();
            return Ok(new { message = "Đã hủy sinh viên khỏi lớp học phần." });
        }
    }
}

