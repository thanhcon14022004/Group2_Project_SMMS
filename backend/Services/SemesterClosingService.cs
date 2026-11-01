using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface ISemesterClosingService
    {
        Task<SemesterClosingCheckResultDto> InspectSemesterClosingConditionsAsync(int semesterId);
        Task<bool> CloseSemesterAsync(CloseSemesterRequest request, string performedBy);
    }

    public class SemesterClosingService : ISemesterClosingService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;

        public SemesterClosingService(AppDbContext db, IAuditLogService auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<SemesterClosingCheckResultDto> InspectSemesterClosingConditionsAsync(int semesterId)
        {
            var semester = await _db.Semesters
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.Subject)
                        .ThenInclude(sub => sub!.GradeComponents)
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.Enrollments)
                .Include(s => s.CourseSections)
                    .ThenInclude(cs => cs.StudentGrades)
                .FirstOrDefaultAsync(s => s.Id == semesterId);

            if (semester == null) throw new InvalidOperationException("Học kỳ không tồn tại.");

            var result = new SemesterClosingCheckResultDto
            {
                SemesterId = semester.Id,
                SemesterCode = semester.Code,
                CanCloseImmediately = true
            };

            // 1. Kiểm tra lớp học phần chưa có giảng viên
            var unassignedSections = semester.CourseSections.Where(cs => !cs.LecturerId.HasValue).ToList();
            result.UnassignedLecturerSectionsCount = unassignedSections.Count;
            if (unassignedSections.Count > 0)
            {
                result.CanCloseImmediately = false;
                result.BlockingIssues.Add($"Có {unassignedSections.Count} lớp học phần chưa được phân công giảng viên: {string.Join(", ", unassignedSections.Take(5).Select(s => s.Code))}{(unassignedSections.Count > 5 ? "..." : "")}");
            }

            // 2. Kiểm tra lớp học phần chưa khóa điểm
            var unlockedSections = semester.CourseSections.Where(cs => !cs.IsGradeLocked).ToList();
            result.UnlockedSectionsCount = unlockedSections.Count;
            if (unlockedSections.Count > 0)
            {
                result.CanCloseImmediately = false;
                result.Warnings.Add($"Có {unlockedSections.Count} lớp học phần chưa được Admin khóa điểm.");
            }

            // 3. Kiểm tra thiếu điểm (sinh viên trong lớp chưa có đủ điểm thành phần)
            int missingGradesSections = 0;
            foreach (var sec in semester.CourseSections)
            {
                int compCount = sec.Subject?.GradeComponents.Count ?? 0;
                if (compCount == 0) continue;

                foreach (var en in sec.Enrollments)
                {
                    var gradedCount = sec.StudentGrades.Count(g => g.StudentId == en.StudentId && g.Score.HasValue);
                    if (gradedCount < compCount)
                    {
                        missingGradesSections++;
                        break;
                    }
                }
            }

            result.MissingGradeSectionsCount = missingGradesSections;
            if (missingGradesSections > 0)
            {
                result.CanCloseImmediately = false;
                result.BlockingIssues.Add($"Có {missingGradesSections} lớp học phần vẫn còn sinh viên chưa có đầy đủ điểm thành phần.");
            }

            // 4. Kiểm tra sinh viên chưa được phân bổ đủ môn bắt buộc
            // Đếm số sinh viên đang học nhưng chưa có lớp nào trong kỳ
            var activeStudents = await _db.Students.Where(s => s.Status == "Studying" && s.User!.IsActive).ToListAsync();
            int unallocatedStudents = 0;
            foreach (var st in activeStudents)
            {
                bool hasAnyEnrollment = semester.CourseSections.Any(cs => cs.Enrollments.Any(e => e.StudentId == st.Id));
                if (!hasAnyEnrollment)
                {
                    unallocatedStudents++;
                }
            }

            result.UnallocatedCompulsoryStudentsCount = unallocatedStudents;
            if (unallocatedStudents > 0)
            {
                result.Warnings.Add($"Có {unallocatedStudents} sinh viên đang hoạt động nhưng chưa có môn học nào trong học kỳ này.");
            }

            return result;
        }

        public async Task<bool> CloseSemesterAsync(CloseSemesterRequest request, string performedBy)
        {
            var semester = await _db.Semesters
                .Include(s => s.CourseSections)
                .FirstOrDefaultAsync(s => s.Id == request.SemesterId);

            if (semester == null) throw new InvalidOperationException("Học kỳ không tồn tại.");
            if (semester.Status == "Ended") throw new InvalidOperationException("Học kỳ này đã kết thúc trước đó.");

            var inspection = await InspectSemesterClosingConditionsAsync(request.SemesterId);

            if (!inspection.CanCloseImmediately && !request.ForceWithExceptions)
            {
                throw new InvalidOperationException(
                    "Học kỳ chưa đủ điều kiện kết thúc tiêu chuẩn:\n" +
                    string.Join("\n", inspection.BlockingIssues) +
                    "\n\nNếu muốn kết thúc ngoại lệ, vui lòng bật tùy chọn 'Kết thúc ngoại lệ' và cung cấp lý do bắt buộc."
                );
            }

            if (request.ForceWithExceptions && string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new InvalidOperationException("Kết thúc học kỳ theo trường hợp ngoại lệ bắt buộc phải có lý do cụ thể.");
            }

            // Tự động khóa toàn bộ các lớp học phần trong kỳ
            foreach (var sec in semester.CourseSections)
            {
                sec.IsGradeLocked = true;
                sec.GradeLockedAt ??= DateTime.UtcNow;
                sec.GradeLockedBy ??= performedBy;
                sec.Status = "Completed";
            }

            semester.Status = "Ended";
            semester.IsClosedWithExceptions = request.ForceWithExceptions;
            semester.CloseReason = request.Reason;

            // Xử lý chuyển tiếp sinh viên lên học kỳ tiếp theo:
            // Tăng CurrentSemester của sinh viên đang học lên 1 kỳ
            var studentsInSemester = await _db.CourseSectionStudents
                .Where(css => css.CourseSection!.SemesterId == semester.Id)
                .Select(css => css.StudentId)
                .Distinct()
                .ToListAsync();

            var students = await _db.Students.Where(s => studentsInSemester.Contains(s.Id)).ToListAsync();
            foreach (var st in students)
            {
                if (st.CurrentSemester < 9)
                {
                    st.CurrentSemester++;
                }
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                request.ForceWithExceptions ? "SEMESTER_CLOSED_EXCEPTION" : "SEMESTER_CLOSED",
                "Semester",
                semester.Id.ToString(),
                performedBy,
                $"Admin {performedBy} kết thúc học kỳ {semester.Code}. Ngoại lệ={request.ForceWithExceptions}. Lý do: {request.Reason ?? "Kết thúc bình thường"}"
            );

            return true;
        }
    }
}

