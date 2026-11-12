using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface ICurriculumService
    {
        Task<CurriculumLookupResponse> LookupCurriculumConfigAsync(CurriculumLookupRequest request);
        Task<CurriculumConfigDto> CreateOrUpdateConfigAsync(CreateCurriculumConfigRequest request, string performedBy);
        Task<CurriculumConfigDto> UpdateConfigAsync(int id, UpdateCurriculumConfigRequest request, string performedBy);
        Task<List<CurriculumConfigDto>> GetAllConfigsAsync(int? majorId = null, string? version = null);
        Task<CurriculumConfigDto?> GetConfigByIdAsync(int id);
        Task<PagedResult<CurriculumConfigDto>> GetConfigsPagedAsync(int? majorId = null, int? semesterNumber = null, string? version = null, string? search = null, int page = 1, int pageSize = 20);
        Task<bool> DeleteConfigAsync(int id, string performedBy);
        Task<int> DeleteAllConfigsAsync(string performedBy);
    }

    public class CurriculumService : ICurriculumService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;

        public CurriculumService(AppDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<CurriculumLookupResponse> LookupCurriculumConfigAsync(CurriculumLookupRequest request)
        {
            var major = await _db.Majors.Include(m => m.Specializations).FirstOrDefaultAsync(m => m.Id == request.MajorId);
            if (major == null)
            {
                return new CurriculumLookupResponse
                {
                    IsFound = false,
                    Message = $"Không tìm thấy ngành đào tạo với ID: {request.MajorId}"
                };
            }

            // Ngành hẹp là tùy chọn (không bắt buộc). Nếu có chọn ngành hẹp thì ưu tiên cấu hình của ngành hẹp đó.
            var baseQuery = _db.CurriculumConfigs
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .Include(c => c.CurriculumSubjects)
                    .ThenInclude(cs => cs.Subject)
                        .ThenInclude(s => s!.PrerequisiteRequirements)
                            .ThenInclude(p => p.Prerequisite)
                .AsNoTracking()
                .AsSplitQuery()
                .Where(c => c.MajorId == request.MajorId && c.SemesterNumber == request.SemesterNumber && c.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Version))
            {
                baseQuery = baseQuery.Where(c => c.Version == request.Version);
            }

            CurriculumConfig? config = null;

            if (request.SpecializationId.HasValue && request.SpecializationId.Value > 0)
            {
                // Ưu tiên tìm theo ngành hẹp đã chọn
                config = await baseQuery.Where(c => c.SpecializationId == request.SpecializationId)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                // Nếu không có cấu hình riêng cho ngành hẹp, fallback về cơ sở ngành (SpecializationId == null)
                if (config == null)
                {
                    config = await baseQuery.Where(c => c.SpecializationId == null)
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();
                }
            }
            else
            {
                // Không chọn ngành hẹp: tìm cấu hình cơ sở ngành
                config = await baseQuery.Where(c => c.SpecializationId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                // Nếu không có bản ghi SpecializationId == null, lấy cấu hình bất kỳ của kỳ này
                if (config == null)
                {
                    config = await baseQuery
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();
                }
            }

            // Nếu không tìm thấy cấu hình theo ngành ở Kỳ 6, thử tìm cấu hình OJT dùng chung cho tất cả các ngành (MajorId == null)
            if (config == null && request.SemesterNumber == 6)
            {
                var universalQuery = _db.CurriculumConfigs
                    .Include(c => c.Major)
                    .Include(c => c.Specialization)
                    .Include(c => c.CurriculumSubjects)
                        .ThenInclude(cs => cs.Subject)
                            .ThenInclude(s => s!.PrerequisiteRequirements)
                                .ThenInclude(p => p.Prerequisite)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(c => c.MajorId == null && c.SemesterNumber == 6 && c.IsActive);

                if (!string.IsNullOrWhiteSpace(request.Version))
                {
                    universalQuery = universalQuery.Where(c => c.Version == request.Version);
                }

                config = await universalQuery.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();
            }

            if (config == null)
            {
                return new CurriculumLookupResponse
                {
                    IsFound = false,
                    Message = $"Chưa có cấu hình chương trình đào tạo cho Ngành: {major.Code}, Học kỳ: {request.SemesterNumber}" +
                              (request.SpecializationId.HasValue ? $", Ngành hẹp ID: {request.SpecializationId}" : "") +
                              (!string.IsNullOrWhiteSpace(request.Version) ? $", Phiên bản: {request.Version}" : "")
                };
            }

            var subjectDetails = config.CurriculumSubjects.Select(cs => new CurriculumSubjectDetailDto
            {
                SubjectId = cs.SubjectId,
                Id = cs.SubjectId,
                SubjectCode = cs.Subject?.Code ?? "",
                Code = cs.Subject?.Code ?? "",
                SubjectName = cs.Subject?.Name ?? "",
                Name = cs.Subject?.Name ?? "",
                Credits = cs.Subject?.Credits ?? 0,
                IsCompulsory = cs.IsCompulsory,
                GradeComponents = cs.Subject?.GradeComponents.OrderBy(gc => gc.OrderIndex).Select(gc => new SubjectGradeComponentDto
                {
                    Id = gc.Id,
                    SubjectId = gc.SubjectId,
                    Name = gc.Name,
                    Weight = gc.Weight,
                    Scale = gc.Scale,
                    IsAdminOnly = gc.IsAdminOnly,
                    OrderIndex = gc.OrderIndex
                }).ToList() ?? new(),
                Prerequisites = cs.Subject?.PrerequisiteRequirements.Select(pr => new PrerequisiteDto
                {
                    Id = pr.Id,
                    SubjectId = pr.SubjectId,
                    PrerequisiteSubjectId = pr.PrerequisiteSubjectId,
                    PrerequisiteSubjectCode = pr.Prerequisite?.Code,
                    PrerequisiteSubjectName = pr.Prerequisite?.Name,
                    MinScore = pr.MinScore
                }).ToList() ?? new()
            }).ToList();

            return new CurriculumLookupResponse
            {
                IsFound = true,
                Message = "Tra cứu cấu hình chương trình thành công.",
                CurriculumConfigId = config.Id,
                ConfigName = config.Name,
                Version = config.Version,
                MajorCode = config.Major?.Code ?? major.Code,
                SemesterNumber = config.SemesterNumber,
                SpecializationCode = config.Specialization?.Code,
                Subjects = subjectDetails
            };
        }

        public async Task<CurriculumConfigDto> CreateOrUpdateConfigAsync(CreateCurriculumConfigRequest request, string performedBy)
        {
            Major? major = null;
            int? majorId = (request.MajorId.HasValue && request.MajorId.Value > 0) ? request.MajorId.Value : null;

            if (majorId.HasValue)
            {
                major = await _db.Majors.Include(m => m.Specializations).FirstOrDefaultAsync(m => m.Id == majorId.Value);
                if (major == null) throw new InvalidOperationException("Ngành đào tạo không tồn tại.");

                // Quy tắc: Chỉ các học kỳ 5, 7, 8 nếu ngành có phân nhánh ngành hẹp thì bắt buộc phải chọn ngành hẹp.
                // Kỳ 6 là kỳ thực tập doanh nghiệp (OJT) không yêu cầu ngành hẹp.
                if ((request.SemesterNumber == 5 || request.SemesterNumber == 7 || request.SemesterNumber == 8) && major.Specializations.Any(s => s.IsActive))
                {
                    if (!request.SpecializationId.HasValue || request.SpecializationId <= 0)
                    {
                        throw new InvalidOperationException($"Ở học kỳ {request.SemesterNumber}, ngành {major.Code} bắt buộc phải chọn ngành hẹp (Kỳ 6 là kỳ thực tập OJT không yêu cầu ngành hẹp).");
                    }
                }
            }
            else if (request.SemesterNumber != 6)
            {
                throw new InvalidOperationException("Vui lòng chọn ngành đào tạo cho học kỳ này (Chỉ kỳ 6 OJT mới được phép dùng chung cho tất cả các ngành).");
            }

            // Kiểm tra các môn học được chọn: phải tồn tại và tổng tỷ trọng đầu điểm phải bằng 100%
            var subjects = await _db.Subjects
                .Include(s => s.GradeComponents)
                .Where(s => request.SubjectIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            if (subjects.Count != request.SubjectIds.Distinct().Count())
            {
                throw new InvalidOperationException("Có môn học trong danh sách không tồn tại hoặc đã ngừng sử dụng.");
            }

            foreach (var subject in subjects)
            {
                var totalWeight = subject.GradeComponents.Sum(g => g.Weight);
                if (Math.Abs(totalWeight - 100.0m) > 0.01m)
                {
                    throw new InvalidOperationException($"Môn {subject.Code} ({subject.Name}) chưa có cấu hình đầu điểm hợp lệ (Tổng tỷ trọng hiện tại: {totalWeight}%, yêu cầu đúng 100%). Hãy cấu hình đầu điểm môn học trước.");
                }
            }

            var specId = (request.SpecializationId.HasValue && request.SpecializationId.Value > 0) ? request.SpecializationId : null;
            var existingConfig = await _db.CurriculumConfigs
                .Include(c => c.CurriculumSubjects)
                .FirstOrDefaultAsync(c => c.MajorId == majorId &&
                                          c.SemesterNumber == request.SemesterNumber &&
                                          c.SpecializationId == specId &&
                                          c.Version == request.Version);

            if (existingConfig == null)
            {
                existingConfig = new CurriculumConfig
                {
                    Version = request.Version,
                    MajorId = majorId,
                    SemesterNumber = request.SemesterNumber,
                    SpecializationId = specId,
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CurriculumConfigs.Add(existingConfig);
                await _db.SaveChangesAsync();
            }
            else
            {
                existingConfig.Name = request.Name;
                existingConfig.Description = request.Description;
                existingConfig.IsActive = true;
                _db.CurriculumSubjects.RemoveRange(existingConfig.CurriculumSubjects);
                await _db.SaveChangesAsync();
            }

            foreach (var sId in request.SubjectIds.Distinct())
            {
                _db.CurriculumSubjects.Add(new CurriculumSubject
                {
                    CurriculumConfigId = existingConfig.Id,
                    SubjectId = sId,
                    IsCompulsory = true
                });
            }

            await _db.SaveChangesAsync();

            var majorLabel = major != null ? major.Code : "Tất cả các ngành (OJT)";
            await _auditLog.LogAsync("CURRICULUM_UPDATE", "CurriculumConfig", existingConfig.Id.ToString(), performedBy,
                $"Cấu hình chương trình đào tạo cho Ngành {majorLabel}, Kỳ {existingConfig.SemesterNumber}, Phiên bản {existingConfig.Version}, Số môn: {subjects.Count}");

            return (await GetConfigByIdAsync(existingConfig.Id))!;
        }

        public async Task<CurriculumConfigDto> UpdateConfigAsync(int id, UpdateCurriculumConfigRequest request, string performedBy)
        {
            var config = await _db.CurriculumConfigs
                .Include(c => c.CurriculumSubjects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (config == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cấu hình chương trình đào tạo với ID: {id}");
            }

            Major? major = null;
            int? majorId = (request.MajorId.HasValue && request.MajorId.Value > 0) ? request.MajorId.Value : null;

            if (majorId.HasValue)
            {
                major = await _db.Majors.Include(m => m.Specializations).FirstOrDefaultAsync(m => m.Id == majorId.Value);
                if (major == null) throw new InvalidOperationException("Ngành đào tạo không tồn tại.");

                // Quy tắc: Chỉ các học kỳ 5, 7, 8 nếu ngành có chuyên ngành hẹp thì bắt buộc chọn ngành hẹp.
                // Kỳ 6 là kỳ thực tập doanh nghiệp (OJT) không yêu cầu ngành hẹp.
                if ((request.SemesterNumber == 5 || request.SemesterNumber == 7 || request.SemesterNumber == 8) && major.Specializations.Any(s => s.IsActive))
                {
                    if (!request.SpecializationId.HasValue || request.SpecializationId.Value <= 0)
                    {
                        throw new InvalidOperationException($"Ở học kỳ {request.SemesterNumber}, ngành {major.Code} bắt buộc phải chọn ngành hẹp (Kỳ 6 là kỳ thực tập OJT không yêu cầu ngành hẹp).");
                    }
                }
            }
            else if (request.SemesterNumber != 6)
            {
                throw new InvalidOperationException("Vui lòng chọn ngành đào tạo cho học kỳ này (Chỉ kỳ 6 OJT mới được phép dùng chung cho tất cả các ngành).");
            }

            // Kiểm tra trùng lặp với cấu hình khác
            var specId = (request.SpecializationId.HasValue && request.SpecializationId.Value > 0) ? request.SpecializationId : null;
            var duplicate = await _db.CurriculumConfigs
                .AnyAsync(c => c.Id != id &&
                               c.MajorId == majorId &&
                               c.SemesterNumber == request.SemesterNumber &&
                               c.SpecializationId == specId &&
                               c.Version == request.Version.Trim());
            if (duplicate)
            {
                var mName = major != null ? major.Code : "Tất cả các ngành";
                throw new InvalidOperationException($"Đã tồn tại cấu hình khác cho Ngành {mName}, Học kỳ {request.SemesterNumber}, Phiên bản {request.Version}.");
            }

            // Kiểm tra các môn học được chọn
            var distinctSubjectIds = request.SubjectIds.Distinct().ToList();
            var subjects = await _db.Subjects
                .Include(s => s.GradeComponents)
                .Where(s => distinctSubjectIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            if (subjects.Count != distinctSubjectIds.Count)
            {
                throw new InvalidOperationException("Có môn học trong danh sách không tồn tại hoặc đã ngừng sử dụng.");
            }

            foreach (var subject in subjects)
            {
                var totalWeight = subject.GradeComponents.Sum(g => g.Weight);
                if (Math.Abs(totalWeight - 100.0m) > 0.01m)
                {
                    throw new InvalidOperationException($"Môn {subject.Code} ({subject.Name}) chưa có cấu hình đầu điểm hợp lệ (Tổng tỷ trọng hiện tại: {totalWeight}%, yêu cầu đúng 100%). Hãy cấu hình đầu điểm môn học trước.");
                }
            }

            // Cập nhật thông tin cấu hình
            config.Version = request.Version.Trim();
            config.MajorId = majorId;
            config.SemesterNumber = request.SemesterNumber;
            config.SpecializationId = specId;
            config.Name = request.Name.Trim();
            config.Description = request.Description?.Trim();
            config.IsActive = request.IsActive;

            // Cập nhật danh sách môn học
            _db.CurriculumSubjects.RemoveRange(config.CurriculumSubjects);
            foreach (var sId in distinctSubjectIds)
            {
                _db.CurriculumSubjects.Add(new CurriculumSubject
                {
                    CurriculumConfigId = config.Id,
                    SubjectId = sId,
                    IsCompulsory = true
                });
            }

            await _db.SaveChangesAsync();

            var majorLabel = major != null ? major.Code : "Tất cả các ngành (OJT)";
            await _auditLog.LogAsync("CURRICULUM_UPDATE", "CurriculumConfig", config.Id.ToString(), performedBy,
                $"Cập nhật cấu hình chương trình đào tạo ID {config.Id}: Ngành {majorLabel}, Kỳ {config.SemesterNumber}, Phiên bản {config.Version}, Số môn: {subjects.Count}, Trạng thái: {(config.IsActive ? "Active" : "Inactive")}");

            return (await GetConfigByIdAsync(config.Id))!;
        }

        public async Task<List<CurriculumConfigDto>> GetAllConfigsAsync(int? majorId = null, string? version = null)
        {
            var query = _db.CurriculumConfigs
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .AsNoTracking()
                .AsQueryable();

            if (majorId.HasValue && majorId.Value > 0)
            {
                query = query.Where(c => c.MajorId == majorId.Value || c.MajorId == null);
            }
            if (!string.IsNullOrWhiteSpace(version)) query = query.Where(c => c.Version == version);

            // Fetch records without SQL ORDER BY to avoid heavy SORT operator and RESOURCE_SEMAPHORE memory wait on LocalDB
            var rawConfigs = await query.ToListAsync();
            var configs = rawConfigs.OrderBy(c => c.MajorId ?? 0).ThenBy(c => c.SemesterNumber).ToList();
            return await MapConfigsToDtosAsync(configs);
        }

        public async Task<PagedResult<CurriculumConfigDto>> GetConfigsPagedAsync(
            int? majorId = null,
            int? semesterNumber = null,
            string? version = null,
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            var (normalizedPage, normalizedSize) = PaginationHelper.Normalize(page, pageSize, defaultPageSize: 20);

            var query = _db.CurriculumConfigs
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .AsNoTracking()
                .AsQueryable();

            if (majorId.HasValue && majorId.Value > 0)
            {
                query = query.Where(c => c.MajorId == majorId.Value || c.MajorId == null);
            }
            if (semesterNumber.HasValue && semesterNumber.Value > 0)
            {
                query = query.Where(c => c.SemesterNumber == semesterNumber.Value);
            }
            if (!string.IsNullOrWhiteSpace(version))
            {
                var ver = version.Trim();
                query = query.Where(c => c.Version == ver);
            }

            var rawConfigs = await query.ToListAsync();
            var orderedConfigs = rawConfigs
                .OrderBy(c => c.MajorId ?? 0)
                .ThenBy(c => c.SemesterNumber)
                .ThenBy(c => c.Id)
                .ToList();

            var mappedAll = await MapConfigsToDtosAsync(orderedConfigs);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                mappedAll = mappedAll.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.MajorCode) && c.MajorCode.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.SpecializationCode) && c.SpecializationCode.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(term)) ||
                    c.Subjects.Any(s => s.SubjectCode.ToLower().Contains(term) || s.SubjectName.ToLower().Contains(term))
                ).ToList();
            }

            return PaginationHelper.Paginate(mappedAll, normalizedPage, normalizedSize);
        }

        private async Task<List<CurriculumConfigDto>> MapConfigsToDtosAsync(List<CurriculumConfig> configs)
        {
            if (configs.Count == 0) return new List<CurriculumConfigDto>();

            var configIds = configs.Select(c => c.Id).ToHashSet();

            var currSubjects = await _db.CurriculumSubjects
                .AsNoTracking()
                .Include(cs => cs.Subject)
                .Where(cs => configIds.Contains(cs.CurriculumConfigId))
                .ToListAsync();

            var csLookup = currSubjects.ToLookup(cs => cs.CurriculumConfigId);

            return configs.Select(c => new CurriculumConfigDto
            {
                Id = c.Id,
                Version = c.Version,
                MajorId = c.MajorId,
                MajorCode = c.Major?.Code ?? (c.MajorId == null ? "TẤT CẢ" : ""),
                SemesterNumber = c.SemesterNumber,
                SpecializationId = c.SpecializationId,
                SpecializationCode = c.Specialization?.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                Subjects = csLookup[c.Id].Select(cs => new CurriculumSubjectDetailDto
                {
                    SubjectId = cs.SubjectId,
                    Id = cs.SubjectId,
                    SubjectCode = cs.Subject?.Code ?? "",
                    Code = cs.Subject?.Code ?? "",
                    SubjectName = cs.Subject?.Name ?? "",
                    Name = cs.Subject?.Name ?? "",
                    Credits = cs.Subject?.Credits ?? 0,
                    IsCompulsory = cs.IsCompulsory,
                    GradeComponents = new(),
                    Prerequisites = new()
                }).ToList()
            }).ToList();
        }

        public async Task<CurriculumConfigDto?> GetConfigByIdAsync(int id)
        {
            var c = await _db.CurriculumConfigs
                .Include(c => c.Major)
                .Include(c => c.Specialization)
                .Include(c => c.CurriculumSubjects)
                    .ThenInclude(cs => cs.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return c == null ? null : MapToDto(c);
        }

        public async Task<bool> DeleteConfigAsync(int id, string performedBy)
        {
            var config = await _db.CurriculumConfigs
                .Include(c => c.CurriculumSubjects)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (config == null) return false;

            _db.CurriculumSubjects.RemoveRange(config.CurriculumSubjects);
            _db.CurriculumConfigs.Remove(config);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CURRICULUM_DELETE", "CurriculumConfig", id.ToString(), performedBy,
                $"Xóa vĩnh viễn cấu hình chương trình ID: {id}");

            return true;
        }

        public async Task<int> DeleteAllConfigsAsync(string performedBy)
        {
            var subjects = await _db.CurriculumSubjects.ToListAsync();
            _db.CurriculumSubjects.RemoveRange(subjects);

            var configs = await _db.CurriculumConfigs.ToListAsync();
            int count = configs.Count;
            _db.CurriculumConfigs.RemoveRange(configs);

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CURRICULUM_DELETE_ALL", "CurriculumConfig", "ALL", performedBy,
                $"Xóa toàn bộ {count} cấu hình chương trình đào tạo.");

            return count;
        }

        private static CurriculumConfigDto MapToDto(CurriculumConfig c)
        {
            return new CurriculumConfigDto
            {
                Id = c.Id,
                Version = c.Version,
                MajorId = c.MajorId,
                MajorCode = c.Major?.Code ?? (c.MajorId == null ? "TẤT CẢ" : ""),
                SemesterNumber = c.SemesterNumber,
                SpecializationId = c.SpecializationId,
                SpecializationCode = c.Specialization?.Code,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                Subjects = c.CurriculumSubjects.Select(cs => new CurriculumSubjectDetailDto
                {
                    SubjectId = cs.SubjectId,
                    Id = cs.SubjectId,
                    SubjectCode = cs.Subject?.Code ?? "",
                    Code = cs.Subject?.Code ?? "",
                    SubjectName = cs.Subject?.Name ?? "",
                    Name = cs.Subject?.Name ?? "",
                    Credits = cs.Subject?.Credits ?? 0,
                    IsCompulsory = cs.IsCompulsory,
                    GradeComponents = new(),
                    Prerequisites = new()
                }).ToList()
            };
        }
    }
}

