using Microsoft.EntityFrameworkCore;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;

namespace project_PRN232.Services
{
    public interface IClassTransferService
    {
        Task<ClassTransferCheckResultDto> CheckTransferEligibilityAsync(int studentId, int targetClassId);
        Task<ClassTransferRequest> RequestTransferAsync(int studentId, RequestClassTransferDto request);
        Task<bool> ProcessTransferRequestAsync(ProcessClassTransferRequestDto request, string performedBy);
        Task<List<ClassTransferRequest>> GetTransferRequestsAsync(int? semesterId = null, string? status = null);
    }

    public class ClassTransferService : IClassTransferService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _auditLog;
        private readonly IStudentAllocationService _allocationService;

        public ClassTransferService(AppDbContext db, IAuditLogService auditLog, IStudentAllocationService allocationService)
        {
            _db = db;
            _auditLog = auditLog;
            _allocationService = allocationService;
        }

        public async Task<ClassTransferCheckResultDto> CheckTransferEligibilityAsync(int studentId, int targetClassId)
        {
            var student = await _db.Students
                .Include(s => s.AdministrativeClass)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) throw new InvalidOperationException("Sinh viên không tồn tại.");

            var targetClass = await _db.AdministrativeClasses
                .Include(c => c.Major)
                .FirstOrDefaultAsync(c => c.Id == targetClassId);

            if (targetClass == null) throw new InvalidOperationException("Lớp hành chính đích không tồn tại.");

            // Kiểm tra ngành và khóa
            if (student.MajorId != targetClass.MajorId)
            {
                return new ClassTransferCheckResultDto
                {
                    IsEligible = false,
                    Message = $"Lớp đích ({targetClass.Code} - {targetClass.Major?.Code}) khác ngành đào tạo với bạn ({student.Major?.Code})."
                };
            }

            if (student.Cohort != targetClass.Cohort)
            {
                return new ClassTransferCheckResultDto
                {
                    IsEligible = false,
                    Message = $"Lớp đích ({targetClass.Code}) thuộc khóa {targetClass.Cohort}, khác khóa của bạn (K{student.Cohort})."
                };
            }

            // Lấy học kỳ hiện tại
            var currentSemester = await _db.Semesters
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (currentSemester == null)
            {
                return new ClassTransferCheckResultDto
                {
                    IsEligible = false,
                    Message = "Hiện không có học kỳ nào đang mở để tiếp nhận chuyển lớp."
                };
            }

            // Kiểm tra hạn chuyển lớp
            if (DateTime.UtcNow > currentSemester.TransferClassDeadline)
            {
                return new ClassTransferCheckResultDto
                {
                    IsEligible = false,
                    Message = $"Đã hết thời hạn yêu cầu chuyển lớp cho học kỳ {currentSemester.Code} (Hạn chót: {currentSemester.TransferClassDeadline:dd/MM/yyyy HH:mm})."
                };
            }

            // Kiểm tra các lớp học phần hiện tại
            var enrolledSections = await _db.CourseSectionStudents
                .Include(css => css.CourseSection)
                    .ThenInclude(cs => cs!.Subject)
                .Include(css => css.CourseSection)
                    .ThenInclude(cs => cs!.Schedules)
                .Where(css => css.StudentId == studentId && css.CourseSection!.SemesterId == currentSemester.Id)
                .Select(css => css.CourseSection!)
                .ToListAsync();

            var compatible = new List<string>();
            var incompatible = new List<string>();

            foreach (var sec in enrolledSections)
            {
                // Kiểm tra xem môn này có lớp tương ứng theo lớp hành chính mới hay không
                var targetSection = await _db.CourseSections
                    .Include(cs => cs.Schedules)
                    .FirstOrDefaultAsync(cs => cs.SemesterId == currentSemester.Id &&
                                               cs.SubjectId == sec.SubjectId &&
                                               cs.Code.Contains(targetClass.Code));

                if (targetSection == null)
                {
                    compatible.Add($"{sec.Code} (Giữ nguyên - không có lớp riêng của lớp đích)");
                }
                else
                {
                    incompatible.Add($"{sec.Code} -> Sẽ chuyển sang {targetSection.Code}");
                }
            }

            return new ClassTransferCheckResultDto
            {
                IsEligible = true,
                Message = "Đủ điều kiện chuyển lớp.",
                CompatibleSections = compatible,
                IncompatibleSections = incompatible
            };
        }

        public async Task<ClassTransferRequest> RequestTransferAsync(int studentId, RequestClassTransferDto request)
        {
            var student = await _db.Students.Include(s => s.AdministrativeClass).FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) throw new InvalidOperationException("Sinh viên không tồn tại.");

            if (!student.AdministrativeClassId.HasValue)
                throw new InvalidOperationException("Sinh viên chưa được xếp vào lớp hành chính nào.");

            var currentSemester = await _db.Semesters
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (currentSemester == null)
                throw new InvalidOperationException("Không có học kỳ nào đang mở.");

            if (DateTime.UtcNow > currentSemester.TransferClassDeadline)
                throw new InvalidOperationException("Đã hết thời hạn yêu cầu chuyển lớp cho học kỳ hiện tại.");

            var eligibility = await CheckTransferEligibilityAsync(studentId, request.TargetAdministrativeClassId);
            if (!eligibility.IsEligible)
            {
                throw new InvalidOperationException(eligibility.Message);
            }

            // Kiểm tra xem đã có request pending chưa
            var existingPending = await _db.ClassTransferRequests
                .AnyAsync(r => r.StudentId == studentId && r.SemesterId == currentSemester.Id && r.Status == "Pending");

            if (existingPending)
                throw new InvalidOperationException("Bạn đã có một yêu cầu chuyển lớp đang chờ xử lý.");

            var transferRequest = new ClassTransferRequest
            {
                StudentId = studentId,
                CurrentAdministrativeClassId = student.AdministrativeClassId.Value,
                TargetAdministrativeClassId = request.TargetAdministrativeClassId,
                SemesterId = currentSemester.Id,
                Reason = request.Reason,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _db.ClassTransferRequests.Add(transferRequest);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CLASS_TRANSFER_REQUESTED", "ClassTransferRequest", transferRequest.Id.ToString(),
                student.StudentCode, $"Yêu cầu chuyển từ lớp {student.AdministrativeClass?.Code} sang lớp ID {request.TargetAdministrativeClassId}");

            return transferRequest;
        }

        public async Task<bool> ProcessTransferRequestAsync(ProcessClassTransferRequestDto request, string performedBy)
        {
            var transferRequest = await _db.ClassTransferRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .Include(r => r.TargetClass)
                .Include(r => r.CurrentClass)
                .Include(r => r.Semester)
                .FirstOrDefaultAsync(r => r.Id == request.RequestId);

            if (transferRequest == null) throw new InvalidOperationException("Yêu cầu chuyển lớp không tồn tại.");
            if (transferRequest.Status != "Pending") throw new InvalidOperationException("Yêu cầu này đã được xử lý trước đó.");

            transferRequest.ProcessedAt = DateTime.UtcNow;
            transferRequest.ProcessedBy = performedBy;
            transferRequest.Note = request.Note;

            if (!request.Approve)
            {
                transferRequest.Status = "Rejected";
                await _db.SaveChangesAsync();
                await _auditLog.LogAsync("CLASS_TRANSFER_REJECTED", "ClassTransferRequest", transferRequest.Id.ToString(),
                    performedBy, $"Từ chối yêu cầu chuyển lớp của sinh viên {transferRequest.Student?.StudentCode}. Lý do: {request.Note}");
                return true;
            }

            // Phê duyệt chuyển lớp
            transferRequest.Status = "Approved";

            var student = transferRequest.Student!;
            student.AdministrativeClassId = transferRequest.TargetAdministrativeClassId;

            // Xử lý các lớp học phần:
            // Giữ nguyên các lớp học phần hiện tại nếu vẫn phù hợp; chỉ phân bổ lại những lớp không còn phù hợp
            var enrolledSections = await _db.CourseSectionStudents
                .Include(css => css.CourseSection)
                    .ThenInclude(cs => cs!.Subject)
                .Where(css => css.StudentId == student.Id && css.CourseSection!.SemesterId == transferRequest.SemesterId)
                .ToListAsync();

            foreach (var en in enrolledSections)
            {
                var cs = en.CourseSection;
                if (cs == null) continue;

                // Nếu lớp học phần này mang tên lớp cũ và lớp mới có lớp học phần riêng còn chỗ
                if (cs.Code.Contains(transferRequest.CurrentClass!.Code))
                {
                    var newSection = await _db.CourseSections
                        .Include(s => s.Schedules)
                        .FirstOrDefaultAsync(s => s.SemesterId == transferRequest.SemesterId &&
                                                  s.SubjectId == cs.SubjectId &&
                                                  s.Code.Contains(transferRequest.TargetClass!.Code) &&
                                                  s.Status == "Open");

                    if (newSection != null)
                    {
                        var enrolledCount = await _db.CourseSectionStudents.CountAsync(x => x.CourseSectionId == newSection.Id);
                        if (enrolledCount < newSection.MaxCapacity)
                        {
                            // Đổi sang lớp mới
                            _db.CourseSectionStudents.Remove(en);
                            _db.CourseSectionStudents.Add(new CourseSectionStudent
                            {
                                CourseSectionId = newSection.Id,
                                StudentId = student.Id,
                                EnrolledAt = DateTime.UtcNow,
                                Status = "Enrolled"
                            });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

            await _auditLog.LogAsync("CLASS_TRANSFERRED", "Student", student.Id.ToString(), performedBy,
                $"Chuyển sinh viên {student.StudentCode} từ lớp {transferRequest.CurrentClass?.Code} sang lớp {transferRequest.TargetClass?.Code}");

            return true;
        }

        public async Task<List<ClassTransferRequest>> GetTransferRequestsAsync(int? semesterId = null, string? status = null)
        {
            var query = _db.ClassTransferRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .Include(r => r.CurrentClass)
                .Include(r => r.TargetClass)
                .Include(r => r.Semester)
                .AsNoTracking()
                .AsQueryable();

            if (semesterId.HasValue) query = query.Where(r => r.SemesterId == semesterId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);

            return await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
        }
    }
}

