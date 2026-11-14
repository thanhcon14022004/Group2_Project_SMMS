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
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SubjectsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [EnableQuery(PageSize = 100)]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _db.Subjects
                .AsNoTracking()
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                    .ThenInclude(sp => sp!.Major)
                .OrderBy(s => s.Code)
                .ToListAsync();

            var dtos = await BuildSubjectDtosAsync(subjects);
            return Ok(dtos.AsQueryable());
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetSubjectsPaged(
            [FromQuery] string? search,
            [FromQuery] int? majorId,
            [FromQuery] int? specializationId,
            [FromQuery] int? credits,
            [FromQuery] string? orderby,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (normalizedPage, normalizedSize) = PaginationHelper.Normalize(page, pageSize, defaultPageSize: 20);

            var query = _db.Subjects
                .AsNoTracking()
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                    .ThenInclude(sp => sp!.Major)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(s => s.Code.ToLower().Contains(q) || s.Name.ToLower().Contains(q));
            }
            if (majorId.HasValue && majorId.Value > 0)
            {
                query = query.Where(s => s.MajorId == majorId.Value || (s.Specialization != null && s.Specialization.MajorId == majorId.Value));
            }
            if (specializationId.HasValue && specializationId.Value > 0)
            {
                query = query.Where(s => s.SpecializationId == specializationId.Value);
            }
            if (credits.HasValue && credits.Value > 0)
            {
                query = credits.Value >= 5
                    ? query.Where(s => s.Credits >= 5)
                    : query.Where(s => s.Credits == credits.Value);
            }

            query = orderby switch
            {
                "Code desc" => query.OrderByDescending(s => s.Code),
                "Credits asc" => query.OrderBy(s => s.Credits).ThenBy(s => s.Code),
                "Credits desc" => query.OrderByDescending(s => s.Credits).ThenBy(s => s.Code),
                _ => query.OrderBy(s => s.Code)
            };

            var totalCount = await query.CountAsync();
            var totalPages = normalizedSize > 0 ? (int)Math.Ceiling((double)totalCount / normalizedSize) : 0;
            if (totalPages > 0 && normalizedPage > totalPages)
            {
                normalizedPage = totalPages;
            }

            var subjects = await query
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            var dtos = await BuildSubjectDtosAsync(subjects);

            return Ok(ApiResponse<PagedResult<SubjectDto>>.Ok(new PagedResult<SubjectDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageIndex = normalizedPage,
                PageSize = normalizedSize
            }));
        }

        private async Task<List<SubjectDto>> BuildSubjectDtosAsync(List<Subject> subjects)
        {
            if (subjects.Count == 0) return new List<SubjectDto>();

            var subjectIds = subjects.Select(s => s.Id).ToList();

            var components = await _db.SubjectGradeComponents
                .AsNoTracking()
                .Where(c => subjectIds.Contains(c.SubjectId))
                .ToListAsync();
            var compLookup = components.ToLookup(c => c.SubjectId);

            var prereqs = await _db.PrerequisiteSubjects
                .Include(p => p.Prerequisite)
                .AsNoTracking()
                .Where(p => subjectIds.Contains(p.SubjectId))
                .ToListAsync();
            var prereqLookup = prereqs.ToLookup(p => p.SubjectId);

            return subjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Credits = s.Credits,
                Description = s.Description,
                SpecializationId = s.SpecializationId,
                SpecializationCode = s.Specialization?.Code,
                SpecializationName = s.Specialization?.Name,
                MajorId = s.MajorId ?? s.Specialization?.MajorId,
                MajorCode = s.Major?.Code ?? s.Specialization?.Major?.Code,
                MajorName = s.Major?.Name ?? s.Specialization?.Major?.Name,
                IsActive = s.IsActive,
                GradeComponents = compLookup[s.Id].OrderBy(g => g.OrderIndex).Select(g => new SubjectGradeComponentDto
                {
                    Id = g.Id,
                    SubjectId = g.SubjectId,
                    Name = g.Name,
                    Weight = g.Weight,
                    Scale = g.Scale,
                    IsAdminOnly = g.IsAdminOnly,
                    OrderIndex = g.OrderIndex
                }).ToList(),
                Prerequisites = prereqLookup[s.Id].Select(p => new PrerequisiteDto
                {
                    Id = p.Id,
                    SubjectId = p.SubjectId,
                    PrerequisiteSubjectId = p.PrerequisiteSubjectId,
                    PrerequisiteSubjectCode = p.Prerequisite != null ? p.Prerequisite.Code : "",
                    PrerequisiteSubjectName = p.Prerequisite != null ? p.Prerequisite.Name : "",
                    MinScore = p.MinScore
                }).ToList()
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubject(int id)
        {
            var s = await _db.Subjects
                .AsNoTracking()
                .Include(s => s.Major)
                .Include(s => s.Specialization)
                    .ThenInclude(sp => sp!.Major)
                .Include(s => s.GradeComponents)
                .Include(s => s.PrerequisiteRequirements)
                    .ThenInclude(p => p.Prerequisite)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (s == null) return NotFound();

            var dto = new SubjectDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Credits = s.Credits,
                Description = s.Description,
                SpecializationId = s.SpecializationId,
                SpecializationCode = s.Specialization?.Code,
                SpecializationName = s.Specialization?.Name,
                MajorId = s.MajorId ?? s.Specialization?.MajorId,
                MajorCode = s.Major?.Code ?? s.Specialization?.Major?.Code,
                MajorName = s.Major?.Name ?? s.Specialization?.Major?.Name,
                IsActive = s.IsActive,
                GradeComponents = s.GradeComponents.OrderBy(g => g.OrderIndex).Select(g => new SubjectGradeComponentDto
                {
                    Id = g.Id,
                    SubjectId = g.SubjectId,
                    Name = g.Name,
                    Weight = g.Weight,
                    Scale = g.Scale,
                    IsAdminOnly = g.IsAdminOnly,
                    OrderIndex = g.OrderIndex
                }).ToList(),
                Prerequisites = s.PrerequisiteRequirements.Select(p => new PrerequisiteDto
                {
                    Id = p.Id,
                    SubjectId = p.SubjectId,
                    PrerequisiteSubjectId = p.PrerequisiteSubjectId,
                    PrerequisiteSubjectCode = p.Prerequisite != null ? p.Prerequisite.Code : "",
                    PrerequisiteSubjectName = p.Prerequisite != null ? p.Prerequisite.Name : "",
                    MinScore = p.MinScore
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubject([FromBody] Subject model)
        {
            if (await _db.Subjects.AnyAsync(s => s.Code == model.Code))
                return BadRequest(new { message = $"Mã môn học '{model.Code}' đã tồn tại." });

            model.MajorId = (model.MajorId.HasValue && model.MajorId.Value > 0) ? model.MajorId : null;
            model.SpecializationId = (model.SpecializationId.HasValue && model.SpecializationId.Value > 0) ? model.SpecializationId : null;
            if (model.SpecializationId.HasValue && !await _db.Specializations.AnyAsync(sp => sp.Id == model.SpecializationId.Value))
            {
                return BadRequest(new { message = "Chuyên ngành hẹp không tồn tại." });
            }

            _db.Subjects.Add(model);
            await _db.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] Subject model)
        {
            var subject = await _db.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            subject.Name = model.Name;
            subject.Credits = model.Credits;
            subject.Description = model.Description;
            subject.MajorId = (model.MajorId.HasValue && model.MajorId.Value > 0) ? model.MajorId : null;
            subject.SpecializationId = (model.SpecializationId.HasValue && model.SpecializationId.Value > 0) ? model.SpecializationId : null;
            subject.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return Ok(subject);
        }

        // Cấu hình đầu điểm của môn học: QUY TẮC BẮT BUỘC TỔNG TỶ TRỌNG = 100%
        [HttpPut("{id}/components")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveGradeComponents(int id, [FromBody] SaveSubjectComponentsRequest request)
        {
            var subject = await _db.Subjects.Include(s => s.GradeComponents).FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound(new { message = "Môn học không tồn tại." });

            if (request.Components == null || !request.Components.Any())
            {
                return BadRequest(new { message = "Môn học phải có ít nhất 1 đầu điểm." });
            }

            // Kiểm tra tổng tỷ trọng
            var totalWeight = request.Components.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 100.0m) > 0.01m)
            {
                return BadRequest(new
                {
                    message = $"Tổng tỷ trọng các đầu điểm phải bằng đúng 100%. Tổng hiện tại là {totalWeight}%."
                });
            }

            var existingComponents = subject.GradeComponents.ToList();
            var incomingIds = request.Components.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();

            // 1. Kiểm tra các component cũ bị loại bỏ xem có bị ràng buộc điểm sinh viên không
            var toRemove = existingComponents.Where(e => !incomingIds.Contains(e.Id)).ToList();
            foreach (var rem in toRemove)
            {
                var hasScores = await _db.StudentGrades.AnyAsync(g => g.SubjectGradeComponentId == rem.Id && g.Score.HasValue);
                if (hasScores)
                {
                    return BadRequest(new
                    {
                        message = $"Đầu điểm '{rem.Name}' đã được nhập điểm cho sinh viên trong hệ thống, không thể xóa bỏ. Bạn hãy điều chỉnh lại tỷ trọng của đầu điểm này thay vì xóa."
                    });
                }
                _db.SubjectGradeComponents.Remove(rem);
            }

            // 2. Cập nhật các component hiện có hoặc thêm mới
            int order = 1;
            foreach (var comp in request.Components)
            {
                SubjectGradeComponent? target = null;
                if (comp.Id > 0)
                {
                    target = existingComponents.FirstOrDefault(e => e.Id == comp.Id);
                }

                if (target != null)
                {
                    // Cập nhật component hiện có (giữ nguyên Id để không vỡ điểm sinh viên)
                    target.Name = comp.Name.Trim();
                    target.Weight = comp.Weight;
                    target.Scale = comp.Scale > 0 ? comp.Scale : 10.0m;
                    target.IsAdminOnly = comp.IsAdminOnly;
                    target.OrderIndex = order++;
                }
                else
                {
                    // Thêm mới component
                    _db.SubjectGradeComponents.Add(new SubjectGradeComponent
                    {
                        SubjectId = id,
                        Name = comp.Name.Trim(),
                        Weight = comp.Weight,
                        Scale = comp.Scale > 0 ? comp.Scale : 10.0m,
                        IsAdminOnly = comp.IsAdminOnly,
                        OrderIndex = order++
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Lưu cấu hình đầu điểm thành công." });
        }

        // Cấu hình môn tiên quyết
        [HttpPost("{id}/prerequisites")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPrerequisite(int id, [FromBody] PrerequisiteDto dto)
        {
            if (id == dto.PrerequisiteSubjectId)
                return BadRequest(new { message = "Môn học không thể là môn tiên quyết của chính nó." });

            var exists = await _db.PrerequisiteSubjects.AnyAsync(p => p.SubjectId == id && p.PrerequisiteSubjectId == dto.PrerequisiteSubjectId);
            if (exists)
                return BadRequest(new { message = "Điều kiện tiên quyết này đã tồn tại." });

            var prereq = new PrerequisiteSubject
            {
                SubjectId = id,
                PrerequisiteSubjectId = dto.PrerequisiteSubjectId,
                MinScore = dto.MinScore > 0 ? dto.MinScore : 5.0m
            };

            _db.PrerequisiteSubjects.Add(prereq);
            await _db.SaveChangesAsync();
            return Ok(prereq);
        }

        [HttpDelete("{id}/prerequisites/{prerequisiteSubjectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemovePrerequisite(int id, int prerequisiteSubjectId)
        {
            var item = await _db.PrerequisiteSubjects.FirstOrDefaultAsync(p => p.SubjectId == id && p.PrerequisiteSubjectId == prerequisiteSubjectId);
            if (item == null) return NotFound();

            _db.PrerequisiteSubjects.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa môn tiên quyết." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _db.Subjects
                .Include(s => s.GradeComponents)
                .Include(s => s.PrerequisiteRequirements)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound(new { message = "Không tìm thấy môn học." });

            var hasSections = await _db.CourseSections.AnyAsync(cs => cs.SubjectId == id);
            var hasCurriculums = await _db.CurriculumSubjects.AnyAsync(cs => cs.SubjectId == id);
            var hasAsPrereq = await _db.PrerequisiteSubjects.AnyAsync(p => p.PrerequisiteSubjectId == id);

            if (hasSections || hasCurriculums || hasAsPrereq)
            {
                subject.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Môn học đang có lớp HP hoặc CTĐT liên kết, đã chuyển sang trạng thái Ngừng hoạt động (Inactive)." });
            }

            _db.SubjectGradeComponents.RemoveRange(subject.GradeComponents);
            _db.PrerequisiteSubjects.RemoveRange(subject.PrerequisiteRequirements);
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã xóa môn học thành công." });
        }
    }
}

