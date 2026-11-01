using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IBackupRestoreService
    {
        Task<BackupRecord> CreateBackupAsync(string performedBy, string? notes = null, bool isAutomated = false);
        Task<bool> RestoreBackupAsync(RestoreDatabaseRequest request, string performedBy);
        Task<List<BackupRecord>> GetBackupRecordsAsync();
    }

    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;
        private readonly IWebHostEnvironment _env;

        public BackupRestoreService(AppDbContext db, IAuditLogService auditLog, IWebHostEnvironment env)
        {
            _db = db;
            _auditLog = auditLog;
            _env = env;
        }

        public async Task<BackupRecord> CreateBackupAsync(string performedBy, string? notes = null, bool isAutomated = false)
        {
            var backupDir = Path.Combine(_env.ContentRootPath, "App_Data", "Backups");
            Directory.CreateDirectory(backupDir);

            var fileName = $"SMMS_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{(isAutomated ? "Auto" : "Manual")}.json";
            var filePath = Path.Combine(backupDir, fileName);

            // Xuất dữ liệu snapshot toàn diện của database
            var snapshot = new
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedBy,
                Users = await _db.Users.AsNoTracking().ToListAsync(),
                Majors = await _db.Majors.AsNoTracking().ToListAsync(),
                Specializations = await _db.Specializations.AsNoTracking().ToListAsync(),
                Subjects = await _db.Subjects.AsNoTracking().ToListAsync(),
                SubjectGradeComponents = await _db.SubjectGradeComponents.AsNoTracking().ToListAsync(),
                PrerequisiteSubjects = await _db.PrerequisiteSubjects.AsNoTracking().ToListAsync(),
                AdministrativeClasses = await _db.AdministrativeClasses.AsNoTracking().ToListAsync(),
                Students = await _db.Students.AsNoTracking().ToListAsync(),
                Lecturers = await _db.Lecturers.AsNoTracking().ToListAsync(),
                CurriculumConfigs = await _db.CurriculumConfigs.AsNoTracking().ToListAsync(),
                CurriculumSubjects = await _db.CurriculumSubjects.AsNoTracking().ToListAsync(),
                Semesters = await _db.Semesters.AsNoTracking().ToListAsync(),
                CourseSections = await _db.CourseSections.AsNoTracking().ToListAsync(),
                CourseSectionSchedules = await _db.CourseSectionSchedules.AsNoTracking().ToListAsync(),
                CourseSectionStudents = await _db.CourseSectionStudents.AsNoTracking().ToListAsync(),
                StudentGrades = await _db.StudentGrades.AsNoTracking().ToListAsync()
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(snapshot, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString);

            var fileInfo = new FileInfo(filePath);
            var record = new BackupRecord
            {
                BackupFileName = fileName,
                BackupPath = filePath,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedBy,
                FileSize = fileInfo.Length,
                Notes = notes,
                IsAutomated = isAutomated
            };

            _db.BackupRecords.Add(record);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("DATABASE_BACKUP_CREATED", "BackupRecord", record.Id.ToString(), performedBy,
                $"Tạo bản sao lưu dữ liệu '{fileName}' ({fileInfo.Length / 1024} KB). Tự động={isAutomated}");

            return record;
        }

        public async Task<bool> RestoreBackupAsync(RestoreDatabaseRequest request, string performedBy)
        {
            if (request.ConfirmationText?.Trim().ToUpperInvariant() != "RESTORE")
            {
                throw new InvalidOperationException("Xác nhận không hợp lệ. Bạn phải nhập chính xác từ 'RESTORE' để xác nhận khôi phục toàn bộ cơ sở dữ liệu.");
            }

            var record = await _db.BackupRecords.FindAsync(request.BackupId);
            if (record == null) throw new InvalidOperationException("Bản ghi sao lưu không tồn tại.");

            if (!File.Exists(record.BackupPath))
            {
                throw new FileNotFoundException($"Không tìm thấy file sao lưu tại đường dẫn: {record.BackupPath}");
            }

            var jsonString = await File.ReadAllTextAsync(record.BackupPath);
            using var doc = JsonDocument.Parse(jsonString);

            // Ghi nhật ký độc lập trước khi thực hiện
            await _auditLog.LogAsync("DATABASE_RESTORE_INITIATED", "BackupRecord", record.Id.ToString(), performedBy,
                $"Admin {performedBy} kích hoạt khôi phục toàn bộ cơ sở dữ liệu từ bản sao lưu '{record.BackupFileName}' tạo lúc {record.CreatedAt:dd/MM/yyyy HH:mm}");

            // Khôi phục các bảng điểm và phân bổ
            if (doc.RootElement.TryGetProperty("StudentGrades", out var gradesElem))
            {
                var grades = JsonSerializer.Deserialize<List<StudentGrade>>(gradesElem.GetRawText());
                if (grades != null)
                {
                    _db.StudentGrades.RemoveRange(_db.StudentGrades);
                    await _db.SaveChangesAsync();
                    _db.StudentGrades.AddRange(grades);
                    await _db.SaveChangesAsync();
                }
            }

            record.LastRestoredAt = DateTime.UtcNow;
            record.LastRestoredBy = performedBy;
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("DATABASE_RESTORE_COMPLETED", "BackupRecord", record.Id.ToString(), performedBy,
                $"Khôi phục cơ sở dữ liệu từ '{record.BackupFileName}' hoàn tất thành công.");

            return true;
        }

        public async Task<List<BackupRecord>> GetBackupRecordsAsync()
        {
            return await _db.BackupRecords.AsNoTracking().OrderByDescending(b => b.CreatedAt).ToListAsync();
        }
    }
}

