using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SemesterClosingController : ControllerBase
    {
        private readonly ISemesterClosingService _closingService;

        public SemesterClosingController(ISemesterClosingService closingService)
        {
            _closingService = closingService;
        }

        [HttpGet("inspect/{semesterId}")]
        public async Task<IActionResult> InspectSemester(int semesterId)
        {
            try
            {
                var result = await _closingService.InspectSemesterClosingConditionsAsync(semesterId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseSemester([FromBody] CloseSemesterRequest request)
        {
            try
            {
                var username = User.Identity?.Name ?? "Admin";
                var success = await _closingService.CloseSemesterAsync(request, username);
                return Ok(new { message = "Kết thúc học kỳ thành công.", success });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

