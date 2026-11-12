using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int limit = 200, [FromQuery] string? action = null)
        {
            var logs = await _auditLogService.GetLogsAsync(limit, action);
            return Ok(logs);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetLogsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? action = null,
            [FromQuery] string? search = null)
        {
            var paged = await _auditLogService.GetLogsPagedAsync(page, pageSize, action, search);
            return Ok(ApiResponse<PagedResult<Models.Entities.AuditLog>>.Ok(paged));
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BackupsController : ControllerBase
    {
        private readonly IBackupRestoreService _backupService;

        public BackupsController(IBackupRestoreService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBackups()
        {
            var records = await _backupService.GetBackupRecordsAsync();
            return Ok(records);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBackup([FromQuery] string? notes)
        {
            var username = User.Identity?.Name ?? "Admin";
            var record = await _backupService.CreateBackupAsync(username, notes, isAutomated: false);
            return Ok(new { message = "Tạo bản sao lưu dữ liệu thành công.", record });
        }

        [HttpPost("restore")]
        public async Task<IActionResult> RestoreBackup([FromBody] RestoreDatabaseRequest request)
        {
            try
            {
                var username = User.Identity?.Name ?? "Admin";
                var success = await _backupService.RestoreBackupAsync(request, username);
                return Ok(new { message = "Khôi phục dữ liệu thành công.", success });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

