using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpecializationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SpecializationsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecializations([FromQuery] int? majorId)
        {
            var query = _db.Specializations.Include(s => s.Major).AsNoTracking().AsQueryable();
            if (majorId.HasValue) query = query.Where(s => s.MajorId == majorId.Value);

            var list = await query.Select(s => new SpecializationDto
            {
                Id = s.Id,
                MajorId = s.MajorId,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToListAsync();

            return Ok(ApiResponse<List<SpecializationDto>>.Ok(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecializationById(int id)
        {
            var s = await _db.Specializations.Include(s => s.Major).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy chuyên ngành hẹp."));

            return Ok(ApiResponse<Specialization>.Ok(s));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSpecialization([FromBody] Specialization model)
        {
            var major = await _db.Majors.FindAsync(model.MajorId);
            if (major == null) return BadRequest(ApiResponse.ErrorResult("Ngành đào tạo không tồn tại."));

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(ApiResponse.ErrorResult("Vui lòng nhập tên chuyên ngành hẹp."));

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                model.Code = model.Name.Trim();
            }

            if (await _db.Specializations.AnyAsync(s => s.Code == model.Code))
                return BadRequest(ApiResponse.ErrorResult($"Mã ngành hẹp '{model.Code}' đã tồn tại trong hệ thống."));

            _db.Specializations.Add(model);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<Specialization>.Ok(model, "Tạo chuyên ngành hẹp thành công."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpecialization(int id, [FromBody] Specialization model)
        {
            var spec = await _db.Specializations.FindAsync(id);
            if (spec == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy chuyên ngành hẹp."));

            spec.Name = model.Name;
            spec.Description = model.Description;
            spec.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<Specialization>.Ok(spec, "Cập nhật chuyên ngành hẹp thành công."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var spec = await _db.Specializations.FindAsync(id);
            if (spec == null) return NotFound(ApiResponse.ErrorResult("Không tìm thấy chuyên ngành hẹp."));

            var hasStudents = await _db.Students.AnyAsync(s => s.SpecializationId == id);
            var hasClasses = await _db.AdministrativeClasses.AnyAsync(c => c.SpecializationId == id);
            if (hasStudents || hasClasses)
            {
                spec.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(new { }, "Chuyên ngành đang có sinh viên/lớp liên kết, đã chuyển sang trạng thái Ngừng hoạt động."));
            }

            _db.Specializations.Remove(spec);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { }, "Đã xóa chuyên ngành hẹp thành công."));
        }
    }
}

