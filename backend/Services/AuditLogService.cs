using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityName, string? entityId, string performedBy, string? details, string? ipAddress = null);
        Task<List<AuditLog>> GetLogsAsync(int limit = 200, string? action = null);
        Task<PagedResult<AuditLog>> GetLogsPagedAsync(int page = 1, int pageSize = 20, string? action = null, string? search = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(AppDbContext db, ILogger<AuditLogService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityName, string? entityId, string performedBy, string? details, string? ipAddress = null)
        {
            try
            {
                var log = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "System" : performedBy,
                    PerformedAt = DateTime.UtcNow,
                    Details = details,
                    IpAddress = ipAddress
                };

                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi AuditLog: Action={Action}, Entity={Entity}", action, entityName);
            }
        }

        public async Task<List<AuditLog>> GetLogsAsync(int limit = 200, string? action = null)
        {
            var query = _db.AuditLogs.AsNoTracking().OrderByDescending(l => l.PerformedAt).AsQueryable();
            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(l => l.Action == action);
            }
            return await query.Take(limit).ToListAsync();
        }

        public async Task<PagedResult<AuditLog>> GetLogsPagedAsync(int page = 1, int pageSize = 20, string? action = null, string? search = null)
        {
            var query = _db.AuditLogs.AsNoTracking().OrderByDescending(l => l.PerformedAt).AsQueryable();
            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(l => l.Action == action);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(l =>
                    l.Action.ToLower().Contains(q) ||
                    l.EntityName.ToLower().Contains(q) ||
                    l.PerformedBy.ToLower().Contains(q) ||
                    (l.Details != null && l.Details.ToLower().Contains(q)));
            }
            return await PaginationHelper.PaginateAsync(query, page, pageSize);
        }
    }
}

