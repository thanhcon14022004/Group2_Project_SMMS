using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SemestersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SemestersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [EnableQuery(PageSize = 100)]
        public IQueryable<SemesterDto> GetSemesters()
        {
            return _db.Semesters
                .AsNoTracking()
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SemesterDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    TransferClassDeadline = s.TransferClassDeadline,
                    GradeEntryDeadline = s.GradeEntryDeadline,
                    Status = s.Status,
                    IsClosedWithExceptions = s.IsClosedWithExceptions,
                    AdministrativeClassesCount = _db.AdministrativeClasses.Count(c => c.IsActive),
                    CourseSectionsCount = _db.CourseSections.Count(cs => cs.SemesterId == s.Id)
                });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSemester()
        {
            var semester = await _db.Semesters
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                semester = await _db.Semesters.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();
            }

            return Ok(semester);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSemester([FromBody] Semester model)
        {
            if (await _db.Semesters.AnyAsync(s => s.Code == model.Code))
                return BadRequest(new { message = $"Mã học kỳ '{model.Code}' đã tồn tại." });

            _db.Semesters.Add(model);
            await _db.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSemester(int id, [FromBody] Semester model)
        {
            var sem = await _db.Semesters.FindAsync(id);
            if (sem == null) return NotFound();

            sem.Name = model.Name;
            sem.StartDate = model.StartDate;
            sem.EndDate = model.EndDate;
            sem.TransferClassDeadline = model.TransferClassDeadline;
            sem.GradeEntryDeadline = model.GradeEntryDeadline;
            sem.Status = model.Status;

            await _db.SaveChangesAsync();
            return Ok(sem);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            var sem = await _db.Semesters.FindAsync(id);
            if (sem == null) return NotFound(new { message = "Không tìm thấy học kỳ." });

            var hasSections = await _db.CourseSections.AnyAsync(cs => cs.SemesterId == id);
            if (hasSections)
            {
                return BadRequest(new { message = "Học kỳ đã có lớp học phần mở, không thể xóa. Bạn có thể sử dụng chức năng Đóng học kỳ." });
            }

            _db.Semesters.Remove(sem);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa học kỳ thành công." });
        }

        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockSemester(int id)
        {
            var sem = await _db.Semesters.FindAsync(id);
            if (sem == null) return NotFound(new { message = "Không tìm thấy học kỳ." });

            sem.Status = "Closed";
            await _db.SaveChangesAsync();
            return Ok(new { message = $"Đã khóa học kỳ '{sem.Code}' thành công.", status = sem.Status });
        }

        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockSemester(int id)
        {
            var sem = await _db.Semesters.FindAsync(id);
            if (sem == null) return NotFound(new { message = "Không tìm thấy học kỳ." });

            sem.Status = "Active";
            await _db.SaveChangesAsync();
            return Ok(new { message = $"Đã mở lại học kỳ '{sem.Code}' thành công.", status = sem.Status });
        }
    }
}

