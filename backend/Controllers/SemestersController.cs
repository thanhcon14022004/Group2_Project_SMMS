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
            return ProjectToSemesterDto(_db.Semesters.AsNoTracking().OrderByDescending(s => s.StartDate));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetSemestersPaged(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int? year,
            [FromQuery] string? orderby,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Semesters.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(s => s.Code.ToLower().Contains(q) || s.Name.ToLower().Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status.Trim());
            }
            if (year.HasValue && year.Value > 0)
            {
                var yStr = year.Value.ToString();
                var shortY = yStr.Length >= 2 ? yStr.Substring(yStr.Length - 2) : yStr;
                query = query.Where(s => s.StartDate.Year == year.Value || s.EndDate.Year == year.Value || s.Code.Contains(shortY) || s.Name.Contains(yStr));
            }

            query = orderby switch
            {
                "Code asc" => query.OrderBy(s => s.Code),
                "Code desc" => query.OrderByDescending(s => s.Code),
                "StartDate asc" => query.OrderBy(s => s.StartDate),
                _ => query.OrderByDescending(s => s.StartDate)
            };

            var paged = await PaginationHelper.PaginateAsync(ProjectToSemesterDto(query), page, pageSize);
            return Ok(ApiResponse<PagedResult<SemesterDto>>.Ok(paged));
        }

        private IQueryable<SemesterDto> ProjectToSemesterDto(IQueryable<Semester> query)
        {
            var activeClassesCount = _db.AdministrativeClasses.Count(c => c.IsActive);
            return query.Select(s => new SemesterDto
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
                AdministrativeClassesCount = activeClassesCount,
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
            var sem = await _db.Semesters
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.Subject)
                        .ThenInclude(sub => sub!.GradeComponents)
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.Enrollments)
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.StudentGrades)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sem == null) return NotFound(new { message = "Không tìm thấy học kỳ." });

            var username = User.Identity?.Name ?? "Admin";
            sem.Status = "Closed";

            foreach (var cs in sem.CourseSections)
            {
                cs.IsGradeLocked = true;
                cs.GradeLockedAt = DateTime.UtcNow;
                cs.GradeLockedBy = username;
                cs.Status = "Completed";

                var components = cs.Subject?.GradeComponents.ToList() ?? new List<SubjectGradeComponent>();
                foreach (var en in cs.Enrollments)
                {
                    en.Status = "Completed";

                    foreach (var c in components)
                    {
                        var grade = cs.StudentGrades.FirstOrDefault(g => g.StudentId == en.StudentId && g.SubjectGradeComponentId == c.Id);
                        if (grade == null)
                        {
                            cs.StudentGrades.Add(new StudentGrade
                            {
                                CourseSectionId = cs.Id,
                                StudentId = en.StudentId,
                                SubjectGradeComponentId = c.Id,
                                Score = 0.0m,
                                Notes = "Chốt 0 điểm khi khóa học kỳ",
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = username
                            });
                        }
                        else if (!grade.Score.HasValue)
                        {
                            grade.Score = 0.0m;
                            grade.Notes = string.IsNullOrWhiteSpace(grade.Notes) ? "Chốt 0 điểm khi khóa học kỳ" : grade.Notes;
                            grade.UpdatedAt = DateTime.UtcNow;
                            grade.UpdatedBy = username;
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = $"Đã khóa học kỳ '{sem.Code}' và chốt toàn bộ điểm thành công.", status = sem.Status });
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

