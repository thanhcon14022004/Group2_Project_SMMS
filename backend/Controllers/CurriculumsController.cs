using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CurriculumsController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public CurriculumsController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
        }

        // Chức năng "Tra cứu cấu hình"
        [HttpPost("lookup")]
        public async Task<IActionResult> LookupCurriculum([FromBody] CurriculumLookupRequest request)
        {
            var result = await _curriculumService.LookupCurriculumConfigAsync(request);
            if (!result.IsFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigs([FromQuery] int? majorId, [FromQuery] string? version)
        {
            var list = await _curriculumService.GetAllConfigsAsync(majorId, version);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfigById(int id)
        {
            var config = await _curriculumService.GetConfigByIdAsync(id);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOrUpdateConfig([FromBody] CreateCurriculumConfigRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var config = await _curriculumService.CreateOrUpdateConfigAsync(request, performedBy);
                return Ok(config);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] UpdateCurriculumConfigRequest request)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "Admin";
                var config = await _curriculumService.UpdateConfigAsync(id, request, performedBy);
                return Ok(config);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var performedBy = User.Identity?.Name ?? "Admin";
            var success = await _curriculumService.DeleteConfigAsync(id, performedBy);
            if (!success) return NotFound(new { message = "Không tìm thấy cấu hình chương trình." });
            return Ok(new { message = "Đã xóa vĩnh viễn cấu hình chương trình đào tạo." });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllConfigs()
        {
            var performedBy = User.Identity?.Name ?? "Admin";
            var count = await _curriculumService.DeleteAllConfigsAsync(performedBy);
            return Ok(new { message = $"Đã xóa sạch toàn bộ ({count}) cấu hình chương trình đào tạo." });
        }
    }
}

