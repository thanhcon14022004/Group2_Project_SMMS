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
    public class MajorsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MajorsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMajors()
        {
            var majors = await _db.Majors
                .Include(m => m.Specializations)
                .AsNoTracking()
                .Select(m => new MajorDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    Specializations = m.Specializations.Select(s => new SpecializationDto
                    {
                        Id = s.Id,
                        MajorId = s.MajorId,
                        Code = s.Code,
                        Name = s.Name,
                        Description = s.Description,
                        IsActive = s.IsActive
                    }).ToList()
                })
                .ToListAsync();

            return Ok(majors);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMajor([FromBody] Major model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
                return BadRequest(new { message = "Vui lòng nhập mã ngành (VD: SE, AI, MKT, IB, MC...)" });

            model.Code = model.Code.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Code;
            }

            if (await _db.Majors.AnyAsync(m => m.Code == model.Code))
                return BadRequest(new { message = $"Mã ngành '{model.Code}' đã tồn tại." });

            _db.Majors.Add(model);
            await _db.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMajor(int id, [FromBody] Major model)
        {
            var major = await _db.Majors.FindAsync(id);
            if (major == null) return NotFound();

            major.Name = model.Name;
            major.Description = model.Description;
            major.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return Ok(major);
        }

        [HttpPost("{majorId}/specializations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSpecialization(int majorId, [FromBody] Specialization model)
        {
            var major = await _db.Majors.FindAsync(majorId);
            if (major == null) return NotFound(new { message = "Ngành đào tạo không tồn tại." });

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "Vui lòng nhập tên chuyên ngành hẹp." });

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                model.Code = model.Name.Trim();
            }

            if (await _db.Specializations.AnyAsync(s => s.Code == model.Code))
                return BadRequest(new { message = $"Mã ngành hẹp '{model.Code}' đã tồn tại trong hệ thống." });

            model.MajorId = majorId;
            _db.Specializations.Add(model);
            await _db.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("specializations/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpecialization(int id, [FromBody] Specialization model)
        {
            var spec = await _db.Specializations.FindAsync(id);
            if (spec == null) return NotFound();

            spec.Name = model.Name;
            spec.Description = model.Description;
            spec.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return Ok(spec);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMajor(int id)
        {
            var major = await _db.Majors
                .Include(m => m.Specializations)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (major == null) return NotFound(new { message = "Không tìm thấy ngành đào tạo." });

            var hasClasses = await _db.AdministrativeClasses.AnyAsync(c => c.MajorId == id);
            var hasCurriculums = await _db.CurriculumConfigs.AnyAsync(c => c.MajorId == id);
            if (hasClasses || hasCurriculums)
            {
                major.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Ngành đang có lớp hoặc CTĐT liên kết, đã chuyển sang trạng thái Ngừng hoạt động (Inactive)." });
            }

            if (major.Specializations.Any())
            {
                _db.Specializations.RemoveRange(major.Specializations);
            }
            _db.Majors.Remove(major);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa ngành đào tạo thành công." });
        }

        [HttpDelete("specializations/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var spec = await _db.Specializations.FindAsync(id);
            if (spec == null) return NotFound(new { message = "Không tìm thấy chuyên ngành hẹp." });

            var hasStudents = await _db.Students.AnyAsync(s => s.SpecializationId == id);
            var hasClasses = await _db.AdministrativeClasses.AnyAsync(c => c.SpecializationId == id);
            if (hasStudents || hasClasses)
            {
                spec.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Chuyên ngành đang có sinh viên hoặc lớp liên kết, đã chuyển sang Inactive." });
            }

            _db.Specializations.Remove(spec);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa chuyên ngành hẹp thành công." });
        }
    }
}

