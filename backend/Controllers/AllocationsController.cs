using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_PRN232.DTOs;
using project_PRN232.Services;

namespace project_PRN232.Controllers
{
    /// <summary>
    /// API xử lý phân bổ sinh viên vào lớp học phần và quản lý kết quả phân bổ.
    /// </summary>
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

        /// <summary>
        /// Thực thi thuật toán phân bổ sinh viên vào các lớp học phần theo học kỳ.
        /// </summary>
        [HttpPost("run")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AllocationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RunAllocation([FromBody] RunAllocationRequest request)
        {
            if (request == null || request.SemesterId <= 0)
            {
                return BadRequest(new { message = "Yêu cầu phân bổ không hợp lệ hoặc thiếu mã học kỳ." });
            }

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

        /// <summary>
        /// Lấy trạng thái và tổng kết các lớp hành chính đã được phân bổ trong học kỳ.
        /// </summary>
        [HttpGet("status/{semesterId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AllocationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllocationStatus(int semesterId)
        {
            if (semesterId <= 0)
            {
                return BadRequest(new { message = "Mã học kỳ không hợp lệ." });
            }

            try
            {
                var result = await _allocationService.GetAllocationStatusAsync(semesterId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status/{semesterId}/paged")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<AllocationStatusPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllocationStatusPaged(
            int semesterId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (semesterId <= 0)
            {
                return BadRequest(ApiResponse.ErrorResult("Mã học kỳ không hợp lệ."));
            }

            try
            {
                var result = await _allocationService.GetAllocationStatusAsync(semesterId);
                var dto = new AllocationStatusPagedDto
                {
                    SemesterId = result.SemesterId,
                    SemesterCode = result.SemesterCode,
                    TotalStudentsProcessed = result.TotalStudentsProcessed,
                    TotalEligibleStudents = result.TotalEligibleStudents,
                    TotalEnrolledSections = result.TotalEnrolledSections,
                    AutoCreatedStudentsCount = result.AutoCreatedStudentsCount,
                    BlockedPrerequisiteCount = result.BlockedPrerequisiteCount,
                    CapacityOverflowCount = result.CapacityOverflowCount,
                    UnallocatedCount = result.UnallocatedCount,
                    PagedClassSummaries = PaginationHelper.Paginate(result.ClassSummaries, page, pageSize)
                };
                return Ok(ApiResponse<AllocationStatusPagedDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// Lấy danh sách sinh viên chưa được phân bổ do ngoại lệ (tiên quyết, trùng lịch, sĩ số).
        /// </summary>
        [HttpGet("exceptions/{semesterId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<UnallocatedExceptionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExceptions(int semesterId)
        {
            if (semesterId <= 0)
            {
                return BadRequest(new { message = "Mã học kỳ không hợp lệ." });
            }

            var exceptions = await _allocationService.GetUnallocatedExceptionsAsync(semesterId);
            return Ok(exceptions);
        }

        [HttpGet("exceptions/{semesterId}/paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetExceptionsPaged(
            int semesterId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (semesterId <= 0)
            {
                return BadRequest(ApiResponse.ErrorResult("Mã học kỳ không hợp lệ."));
            }

            var exceptions = await _allocationService.GetUnallocatedExceptionsAsync(semesterId);
            var paged = PaginationHelper.Paginate(exceptions, page, pageSize);
            return Ok(ApiResponse<PagedResult<UnallocatedExceptionDto>>.Ok(paged));
        }

        /// <summary>
        /// Ghi danh thủ công sinh viên vào một lớp học phần cụ thể.
        /// </summary>
        [HttpPost("manual-enroll")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ManualEnroll([FromQuery] int courseSectionId, [FromQuery] int studentId)
        {
            if (courseSectionId <= 0 || studentId <= 0)
            {
                return BadRequest(new { message = "Mã lớp học phần hoặc mã sinh viên không hợp lệ." });
            }

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

        /// <summary>
        /// Hủy ghi danh sinh viên khỏi lớp học phần.
        /// </summary>
        [HttpDelete("manual-remove")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ManualRemove([FromQuery] int courseSectionId, [FromQuery] int studentId)
        {
            if (courseSectionId <= 0 || studentId <= 0)
            {
                return BadRequest(new { message = "Mã lớp học phần hoặc mã sinh viên không hợp lệ." });
            }

            var performedBy = User.Identity?.Name ?? "Admin";
            var success = await _allocationService.RemoveStudentFromSectionAsync(courseSectionId, studentId, performedBy);
            if (!success) return NotFound();
            return Ok(new { message = "Đã hủy sinh viên khỏi lớp học phần." });
        }
    }
}

