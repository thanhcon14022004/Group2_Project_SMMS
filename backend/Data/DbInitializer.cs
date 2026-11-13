using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using project_PRN232.Models.Entities;
using System.IO;
using System.Text.RegularExpressions;

namespace project_PRN232.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Luôn đảm bảo bảng LecturerSubjects tồn tại cho tính năng phân bổ giáo viên
            EnsureLecturerSubjectsTableExists(context);

            // Nếu đã có dữ liệu môn học và người dùng, không cần nạp lại
            if (context.Users.Any() && context.Subjects.Any())
            {
                return;
            }

            // 1. Tự động nạp bộ dữ liệu đầy đủ từ SMMS_Database.sql nếu database trống
            if (TrySeedFromSqlScript(context))
            {
                return;
            }

            // 2. Dự phòng: nạp dữ liệu mẫu C# nếu không đọc được file SQL
            SeedDefaultData(context);
        }

        private static void EnsureLecturerSubjectsTableExists(AppDbContext context)
        {
            try
            {
                var sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LecturerSubjects')
BEGIN
    CREATE TABLE [LecturerSubjects] (
        [Id] int NOT NULL IDENTITY,
        [LecturerId] int NOT NULL,
        [SubjectId] int NOT NULL,
        [MajorId] int NULL,
        [SpecializationId] int NULL,
        [RegisteredAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_LecturerSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LecturerSubjects_Lecturers_LecturerId] FOREIGN KEY ([LecturerId]) REFERENCES [Lecturers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LecturerSubjects_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LecturerSubjects_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LecturerSubjects_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LecturerSubjects_LecturerId_SubjectId] ON [LecturerSubjects] ([LecturerId], [SubjectId]);
END

IF NOT EXISTS (SELECT 1 FROM LecturerSubjects)
BEGIN
    INSERT INTO LecturerSubjects (LecturerId, SubjectId, MajorId, SpecializationId, RegisteredAt)
    SELECT l.Id, s.Id, s.MajorId, s.SpecializationId, GETUTCDATE()
    FROM Lecturers l
    CROSS JOIN Subjects s
    WHERE (l.Department = 'Software Engineering' AND s.Code IN ('PRF192', 'PRO192', 'PRN211', 'PRN232', 'SWP391', 'SWR302', 'SWT301', 'DBI202'))
       OR (l.Department = 'Information Assurance' AND s.Code IN ('NWC203c', 'IAO201', 'CSN301', 'ISF301', 'DIF301', 'CRY301'))
       OR (l.Department = 'Artificial Intelligence' AND s.Code IN ('AIL302m', 'AIP391', 'DAP301', 'MLP301', 'NLP301', 'CVP301'))
       OR (l.Department = 'Data Science' AND s.Code IN ('MAS291', 'DBI202', 'DBD301', 'BAP301', 'BDA301', 'DSE301'))
       OR (l.Department = 'Business Administration' AND s.Code IN ('MKT101', 'MGT103', 'ECO111', 'OBE102', 'ENP102', 'BMA202'))
       OR (l.Department = 'Digital Art & Design' AND s.Code IN ('UIX201', 'GRA201', 'DRA101', 'TYP101', 'ANM201', 'VFX201'))
       OR (l.Department = 'Japanese Language' AND s.Code IN ('JPD113', 'JPD123', 'JPD213', 'JPD223', 'JPD313', 'JPD323'))
       OR (l.Department = 'English Language' AND s.Code IN ('TRS101', 'TRS201', 'TRS301', 'TRS401', 'TRS501', 'TRS601'))
       OR (l.Department = 'Korean Language' AND s.Code IN ('KOR111', 'KOR121', 'KOR211', 'KOR221', 'KOR311', 'KOR321'));
END";
                context.Database.ExecuteSqlRaw(sql);
            }
            catch { }
        }

        private static bool TrySeedFromSqlScript(AppDbContext context)
        {
            try
            {
                string[] searchPaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Data", "SMMS_Database.sql"),
                    Path.Combine(AppContext.BaseDirectory, "SMMS_Database.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", "SMMS_Database.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "SMMS_Database.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "SMMS_Database.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "backend", "Data", "SMMS_Database.sql")
                };

                string? sqlFilePath = searchPaths.FirstOrDefault(File.Exists);
                if (string.IsNullOrEmpty(sqlFilePath))
                {
                    return false;
                }

                string fullSql = File.ReadAllText(sqlFilePath);
                if (string.IsNullOrWhiteSpace(fullSql))
                {
                    return false;
                }

                // Tách theo lệnh GO của T-SQL (case-insensitive, độc lập trên 1 dòng)
                var batches = Regex.Split(
                    fullSql, 
                    @"^\s*GO\s*(?:--.*)?$", 
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                using var conn = context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                foreach (var rawBatch in batches)
                {
                    var batch = rawBatch.Trim();
                    if (string.IsNullOrWhiteSpace(batch)) continue;

                    // Bỏ qua lệnh CREATE DATABASE hoặc USE vì kết nối đã trỏ đúng CSDL
                    if (Regex.IsMatch(batch, @"^\s*CREATE\s+DATABASE", RegexOptions.IgnoreCase) ||
                        Regex.IsMatch(batch, @"^\s*USE\s+\[", RegexOptions.IgnoreCase) ||
                        Regex.IsMatch(batch, @"^\s*USE\s+[a-zA-Z0-9_]+", RegexOptions.IgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = batch;
                        cmd.CommandTimeout = 180;
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Tiếp tục nếu một số batch DDL bảng đã tồn tại
                    }
                }

                return context.Users.Any();
            }
            catch
            {
                return false;
            }
        }

        private static void SeedDefaultData(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return; // Database đã có dữ liệu seed
            }

            // 1. Tạo Tài khoản & Người dùng
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = passwordHash,
                FullName = "Quản trị viên Hệ thống SMMS",
                Email = "admin@smms.edu.vn",
                PhoneNumber = "0987654321",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var lecturerUser1 = new User
            {
                Username = "gv_hungnv",
                PasswordHash = passwordHash,
                FullName = "ThS. Nguyễn Văn Hùng",
                Email = "hungnv@smms.edu.vn",
                PhoneNumber = "0912345678",
                Role = "Lecturer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var lecturerUser2 = new User
            {
                Username = "gv_huonglt",
                PasswordHash = passwordHash,
                FullName = "TS. Lê Thị Hương",
                Email = "huonglt@smms.edu.vn",
                PhoneNumber = "0923456789",
                Role = "Lecturer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var studentUser1 = new User
            {
                Username = "he170001",
                PasswordHash = passwordHash,
                FullName = "Trần Quang Minh",
                Email = "minhtqhe170001@fpt.edu.vn",
                PhoneNumber = "0934567890",
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var studentUser2 = new User
            {
                Username = "he170002",
                PasswordHash = passwordHash,
                FullName = "Lê Hoàng Nam",
                Email = "namlhhe170002@fpt.edu.vn",
                PhoneNumber = "0945678901",
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var studentUser3 = new User
            {
                Username = "he170003",
                PasswordHash = passwordHash,
                FullName = "Phạm Mai Anh",
                Email = "anhpmhe170003@fpt.edu.vn",
                PhoneNumber = "0956789012",
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, lecturerUser1, lecturerUser2, studentUser1, studentUser2, studentUser3);
            context.SaveChanges();

            // 2. Ngành & Ngành hẹp
            var majorSE = new Major
            {
                Code = "SE",
                Name = "Kỹ thuật phần mềm (Software Engineering)",
                Description = "Đào tạo kỹ sư phần mềm chất lượng cao",
                IsActive = true
            };

            var majorIA = new Major
            {
                Code = "IA",
                Name = "An toàn thông tin (Information Assurance)",
                Description = "Đào tạo chuyên gia bảo mật và phòng thủ không gian mạng",
                IsActive = true
            };

            context.Majors.AddRange(majorSE, majorIA);
            context.SaveChanges();

            var specSE_IS = new Specialization
            {
                MajorId = majorSE.Id,
                Code = "SE-IS",
                Name = "Hệ thống thông tin doanh nghiệp (Information Systems)",
                Description = "Chuyên sâu về kiến trúc hệ thống và ERP",
                IsActive = true
            };

            var specSE_JS = new Specialization
            {
                MajorId = majorSE.Id,
                Code = "SE-JS",
                Name = "Kỹ thuật phần mềm Nhật Bản (Japanese Software)",
                Description = "Chuyên sâu về cầu nối phát triển phần mềm cho thị trường Nhật",
                IsActive = true
            };

            context.Specializations.AddRange(specSE_IS, specSE_JS);
            context.SaveChanges();

            // 3. Môn học & Đầu điểm
            var subPRF192 = new Subject { Code = "PRF192", Name = "Nhập môn lập trình C", Credits = 3, IsActive = true };
            var subPRO192 = new Subject { Code = "PRO192", Name = "Lập trình hướng đối tượng Java", Credits = 3, IsActive = true };
            var subPRN211 = new Subject { Code = "PRN211", Name = "Lập trình C# cơ bản & Windows Apps", Credits = 3, IsActive = true };
            var subPRN232 = new Subject { Code = "PRN232", Name = "Xây dựng ứng dụng Web với ASP.NET Core", Credits = 3, IsActive = true };
            var subSWP391 = new Subject { Code = "SWP391", Name = "Dự án phát triển phần mềm thực chiến", Credits = 3, IsActive = true };
            var subOJT401 = new Subject { Code = "OJT401", Name = "Thực tập tốt nghiệp (On-the-Job Training)", Credits = 10, Description = "Kỳ thực tập doanh nghiệp thực tế", IsActive = true };

            context.Subjects.AddRange(subPRF192, subPRO192, subPRN211, subPRN232, subSWP391, subOJT401);
            context.SaveChanges();

            // Đầu điểm PRF192: Lab 20%, Assignment 20%, Progress Test 20%, Final Exam 40% (Sum = 100%)
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subPRF192.Id, Name = "Lab", Weight = 20m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subPRF192.Id, Name = "Assignment", Weight = 20m, Scale = 10m, OrderIndex = 2 },
                new SubjectGradeComponent { SubjectId = subPRF192.Id, Name = "Progress Test", Weight = 20m, Scale = 10m, OrderIndex = 3 },
                new SubjectGradeComponent { SubjectId = subPRF192.Id, Name = "Final Exam", Weight = 40m, Scale = 10m, OrderIndex = 4, IsAdminOnly = true }
            );

            // Đầu điểm PRO192: Quiz 10%, Lab 20%, Practical Exam 30%, Final Exam 40% (Sum = 100%)
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subPRO192.Id, Name = "Quiz", Weight = 10m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subPRO192.Id, Name = "Lab", Weight = 20m, Scale = 10m, OrderIndex = 2 },
                new SubjectGradeComponent { SubjectId = subPRO192.Id, Name = "Practical Exam", Weight = 30m, Scale = 10m, OrderIndex = 3 },
                new SubjectGradeComponent { SubjectId = subPRO192.Id, Name = "Final Exam", Weight = 40m, Scale = 10m, OrderIndex = 4, IsAdminOnly = true }
            );

            // Đầu điểm PRN211: Lab 20%, Progress Test 20%, Practical Exam 20%, Final Exam 40% (Sum = 100%)
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subPRN211.Id, Name = "Lab Exercises", Weight = 20m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subPRN211.Id, Name = "Progress Test", Weight = 20m, Scale = 10m, OrderIndex = 2 },
                new SubjectGradeComponent { SubjectId = subPRN211.Id, Name = "Practical Exam", Weight = 20m, Scale = 10m, OrderIndex = 3 },
                new SubjectGradeComponent { SubjectId = subPRN211.Id, Name = "Final Exam", Weight = 40m, Scale = 10m, OrderIndex = 4, IsAdminOnly = true }
            );

            // Đầu điểm PRN232: Assignment 1 (15%), Assignment 2 (15%), Progress Test (20%), Final Project (50%) (Sum = 100%)
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subPRN232.Id, Name = "Assignment 1", Weight = 15m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subPRN232.Id, Name = "Assignment 2", Weight = 15m, Scale = 10m, OrderIndex = 2 },
                new SubjectGradeComponent { SubjectId = subPRN232.Id, Name = "Progress Test", Weight = 20m, Scale = 10m, OrderIndex = 3 },
                new SubjectGradeComponent { SubjectId = subPRN232.Id, Name = "Final Project Defense", Weight = 50m, Scale = 10m, OrderIndex = 4, IsAdminOnly = true }
            );

            // Đầu điểm SWP391: Sprint 1 (20%), Sprint 2 (20%), Sprint 3 (20%), Final Presentation (40%) (Sum = 100%)
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subSWP391.Id, Name = "Sprint 1", Weight = 20m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subSWP391.Id, Name = "Sprint 2", Weight = 20m, Scale = 10m, OrderIndex = 2 },
                new SubjectGradeComponent { SubjectId = subSWP391.Id, Name = "Sprint 3", Weight = 20m, Scale = 10m, OrderIndex = 3 },
                new SubjectGradeComponent { SubjectId = subSWP391.Id, Name = "Final Presentation", Weight = 40m, Scale = 10m, OrderIndex = 4, IsAdminOnly = true }
            );

            // Đầu điểm OJT401: Doanh nghiệp 50%, Báo cáo & Thuyết minh 50%
            context.SubjectGradeComponents.AddRange(
                new SubjectGradeComponent { SubjectId = subOJT401.Id, Name = "Đánh giá của Doanh nghiệp", Weight = 50m, Scale = 10m, OrderIndex = 1 },
                new SubjectGradeComponent { SubjectId = subOJT401.Id, Name = "Báo cáo & Thuyết minh thực tập", Weight = 50m, Scale = 10m, OrderIndex = 2 }
            );

            // 4. Môn tiên quyết
            // PRF192 là tiên quyết của PRO192 (>= 5.0)
            context.PrerequisiteSubjects.Add(new PrerequisiteSubject
            {
                SubjectId = subPRO192.Id,
                PrerequisiteSubjectId = subPRF192.Id,
                MinScore = 5.0m
            });

            // PRO192 là tiên quyết của PRN211 (>= 5.0)
            context.PrerequisiteSubjects.Add(new PrerequisiteSubject
            {
                SubjectId = subPRN211.Id,
                PrerequisiteSubjectId = subPRO192.Id,
                MinScore = 5.0m
            });

            // PRN211 là tiên quyết của PRN232 và SWP391 (>= 5.0)
            context.PrerequisiteSubjects.AddRange(
                new PrerequisiteSubject { SubjectId = subPRN232.Id, PrerequisiteSubjectId = subPRN211.Id, MinScore = 5.0m },
                new PrerequisiteSubject { SubjectId = subSWP391.Id, PrerequisiteSubjectId = subPRN211.Id, MinScore = 5.0m }
            );

            context.SaveChanges();

            // 5. Hồ sơ Giảng viên
            var gv1 = new Lecturer
            {
                UserId = lecturerUser1.Id,
                LecturerCode = "GV001",
                Department = "Software Engineering",
                AcademicTitle = "Master"
            };
            var gv2 = new Lecturer
            {
                UserId = lecturerUser2.Id,
                LecturerCode = "GV002",
                Department = "Software Engineering",
                AcademicTitle = "PhD"
            };
            context.Lecturers.AddRange(gv1, gv2);
            context.SaveChanges();

            // 6. Lớp hành chính
            var classSE1701 = new AdministrativeClass
            {
                Code = "SE1701",
                Name = "Kỹ thuật phần mềm K17 Lớp 1",
                MajorId = majorSE.Id,
                Cohort = 17,
                AcademicYear = "2023-2027",
                CurrentSemester = 5,
                IsActive = true
            };
            var classSE1702 = new AdministrativeClass
            {
                Code = "SE1702",
                Name = "Kỹ thuật phần mềm K17 Lớp 2",
                MajorId = majorSE.Id,
                Cohort = 17,
                AcademicYear = "2023-2027",
                CurrentSemester = 5,
                IsActive = true
            };

            context.AdministrativeClasses.AddRange(classSE1701, classSE1702);
            context.SaveChanges();

            // 7. Hồ sơ Sinh viên
            var st1 = new Student
            {
                UserId = studentUser1.Id,
                StudentCode = "SE170001",
                MajorId = majorSE.Id,
                SpecializationId = specSE_IS.Id,
                AdministrativeClassId = classSE1701.Id,
                Cohort = 17,
                CurrentSemester = 5,
                Status = "Studying"
            };
            var st2 = new Student
            {
                UserId = studentUser2.Id,
                StudentCode = "SE170002",
                MajorId = majorSE.Id,
                SpecializationId = specSE_JS.Id,
                AdministrativeClassId = classSE1701.Id,
                Cohort = 17,
                CurrentSemester = 5,
                Status = "Studying"
            };
            var st3 = new Student
            {
                UserId = studentUser3.Id,
                StudentCode = "SE170003",
                MajorId = majorSE.Id,
                SpecializationId = null,
                AdministrativeClassId = classSE1702.Id,
                Cohort = 17,
                CurrentSemester = 4,
                Status = "Studying"
            };

            context.Students.AddRange(st1, st2, st3);
            context.SaveChanges();

            // 8. Cấu hình chương trình đào tạo
            // K17 SE Kỳ 1 -> PRF192
            var configK1 = new CurriculumConfig
            {
                Version = "2024.1",
                MajorId = majorSE.Id,
                SemesterNumber = 1,
                Name = "Chương trình K17 SE - Học kỳ 1",
                IsActive = true
            };
            context.CurriculumConfigs.Add(configK1);
            context.SaveChanges();
            context.CurriculumSubjects.Add(new CurriculumSubject { CurriculumConfigId = configK1.Id, SubjectId = subPRF192.Id, IsCompulsory = true });

            // K17 SE Kỳ 4 -> PRN211
            var configK4 = new CurriculumConfig
            {
                Version = "2024.1",
                MajorId = majorSE.Id,
                SemesterNumber = 4,
                Name = "Chương trình K17 SE - Học kỳ 4",
                IsActive = true
            };
            context.CurriculumConfigs.Add(configK4);
            context.SaveChanges();
            context.CurriculumSubjects.Add(new CurriculumSubject { CurriculumConfigId = configK4.Id, SubjectId = subPRN211.Id, IsCompulsory = true });

            // K17 SE Kỳ 5 (SE-IS) -> PRN232, SWP391
            var configK5_IS = new CurriculumConfig
            {
                Version = "2024.1",
                MajorId = majorSE.Id,
                SemesterNumber = 5,
                SpecializationId = specSE_IS.Id,
                Name = "Chương trình K17 SE-IS - Học kỳ 5",
                IsActive = true
            };
            context.CurriculumConfigs.Add(configK5_IS);
            context.SaveChanges();
            context.CurriculumSubjects.AddRange(
                new CurriculumSubject { CurriculumConfigId = configK5_IS.Id, SubjectId = subPRN232.Id, IsCompulsory = true },
                new CurriculumSubject { CurriculumConfigId = configK5_IS.Id, SubjectId = subSWP391.Id, IsCompulsory = true }
            );

            // K17 SE Kỳ 5 (SE-JS) -> PRN232, SWP391
            var configK5_JS = new CurriculumConfig
            {
                Version = "2024.1",
                MajorId = majorSE.Id,
                SemesterNumber = 5,
                SpecializationId = specSE_JS.Id,
                Name = "Chương trình K17 SE-JS - Học kỳ 5",
                IsActive = true
            };
            context.CurriculumConfigs.Add(configK5_JS);
            context.SaveChanges();
            context.CurriculumSubjects.AddRange(
                new CurriculumSubject { CurriculumConfigId = configK5_JS.Id, SubjectId = subPRN232.Id, IsCompulsory = true },
                new CurriculumSubject { CurriculumConfigId = configK5_JS.Id, SubjectId = subSWP391.Id, IsCompulsory = true }
            );

            // Kỳ 6 OJT: Dùng chung cho tất cả các ngành (MajorId = null)
            var configK6_OJT = new CurriculumConfig
            {
                Version = "2024.1",
                MajorId = null,
                SemesterNumber = 6,
                SpecializationId = null,
                Name = "Chương trình Thực tập doanh nghiệp (OJT)",
                Description = "Kỳ thực tập doanh nghiệp dùng chung cho tất cả các ngành đào tạo",
                IsActive = true
            };
            context.CurriculumConfigs.Add(configK6_OJT);
            context.SaveChanges();
            context.CurriculumSubjects.Add(
                new CurriculumSubject { CurriculumConfigId = configK6_OJT.Id, SubjectId = subOJT401.Id, IsCompulsory = true }
            );

            context.SaveChanges();

            // 9. Học kỳ
            var semFA24 = new Semester
            {
                Code = "FA24",
                Name = "Fall 2024",
                StartDate = DateTime.UtcNow.AddMonths(-8),
                EndDate = DateTime.UtcNow.AddMonths(-4),
                TransferClassDeadline = DateTime.UtcNow.AddMonths(-7),
                GradeEntryDeadline = DateTime.UtcNow.AddMonths(-4).AddDays(-5),
                Status = "Ended"
            };

            var semSP25 = new Semester
            {
                Code = "SP25",
                Name = "Spring 2025",
                StartDate = DateTime.UtcNow.AddMonths(-4),
                EndDate = DateTime.UtcNow.AddDays(-15),
                TransferClassDeadline = DateTime.UtcNow.AddMonths(-3),
                GradeEntryDeadline = DateTime.UtcNow.AddDays(-10),
                Status = "Ended"
            };

            var semSU25 = new Semester
            {
                Code = "SU25",
                Name = "Summer 2025",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddMonths(3),
                TransferClassDeadline = DateTime.UtcNow.AddDays(14), // Còn 14 ngày để sinh viên xin chuyển lớp
                GradeEntryDeadline = DateTime.UtcNow.AddMonths(3).AddDays(-5),
                Status = "Active"
            };

            context.Semesters.AddRange(semFA24, semSP25, semSU25);
            context.SaveChanges();

            // 10. Mở lớp học phần cho kỳ hiện tại SU25
            var secPRN232_1 = new CourseSection
            {
                Code = "PRN232_SU25_SE1701",
                Name = "Lập trình Web ASP.NET Core - Lớp SE1701",
                SubjectId = subPRN232.Id,
                SemesterId = semSU25.Id,
                LecturerId = gv1.Id,
                MaxCapacity = 30,
                IsGradeLocked = false,
                Status = "Open"
            };

            var secSWP391_1 = new CourseSection
            {
                Code = "SWP391_SU25_SE1701",
                Name = "Dự án phát triển phần mềm - Lớp SE1701",
                SubjectId = subSWP391.Id,
                SemesterId = semSU25.Id,
                LecturerId = gv2.Id,
                MaxCapacity = 30,
                IsGradeLocked = false,
                Status = "Open"
            };

            context.CourseSections.AddRange(secPRN232_1, secSWP391_1);
            context.SaveChanges();

            // Lịch học
            context.CourseSectionSchedules.AddRange(
                new CourseSectionSchedule { CourseSectionId = secPRN232_1.Id, DayOfWeek = DayOfWeek.Monday, Slot = 2, Room = "BE-302" },
                new CourseSectionSchedule { CourseSectionId = secPRN232_1.Id, DayOfWeek = DayOfWeek.Thursday, Slot = 2, Room = "BE-302" },
                new CourseSectionSchedule { CourseSectionId = secSWP391_1.Id, DayOfWeek = DayOfWeek.Tuesday, Slot = 3, Room = "AL-204" },
                new CourseSectionSchedule { CourseSectionId = secSWP391_1.Id, DayOfWeek = DayOfWeek.Friday, Slot = 3, Room = "AL-204" }
            );

            // Phân bổ sinh viên st1, st2 vào secPRN232_1
            context.CourseSectionStudents.AddRange(
                new CourseSectionStudent { CourseSectionId = secPRN232_1.Id, StudentId = st1.Id, Status = "Enrolled" },
                new CourseSectionStudent { CourseSectionId = secPRN232_1.Id, StudentId = st2.Id, Status = "Enrolled" },
                new CourseSectionStudent { CourseSectionId = secSWP391_1.Id, StudentId = st1.Id, Status = "Enrolled" },
                new CourseSectionStudent { CourseSectionId = secSWP391_1.Id, StudentId = st2.Id, Status = "Enrolled" }
            );

            context.SaveChanges();

            // Điểm mẫu cho st1 và st2 ở secPRN232_1
            var compsPRN232 = context.SubjectGradeComponents.Where(c => c.SubjectId == subPRN232.Id).ToList();
            var compAss1 = compsPRN232.FirstOrDefault(c => c.Name == "Assignment 1");
            var compAss2 = compsPRN232.FirstOrDefault(c => c.Name == "Assignment 2");
            var compPT = compsPRN232.FirstOrDefault(c => c.Name == "Progress Test");

            if (compAss1 != null && compAss2 != null && compPT != null)
            {
                context.StudentGrades.AddRange(
                    new StudentGrade { CourseSectionId = secPRN232_1.Id, StudentId = st1.Id, SubjectGradeComponentId = compAss1.Id, Score = 8.5m, UpdatedBy = "gv_hungnv", UpdatedAt = DateTime.UtcNow },
                    new StudentGrade { CourseSectionId = secPRN232_1.Id, StudentId = st1.Id, SubjectGradeComponentId = compAss2.Id, Score = 9.0m, UpdatedBy = "gv_hungnv", UpdatedAt = DateTime.UtcNow },
                    new StudentGrade { CourseSectionId = secPRN232_1.Id, StudentId = st1.Id, SubjectGradeComponentId = compPT.Id, Score = 8.0m, UpdatedBy = "gv_hungnv", UpdatedAt = DateTime.UtcNow },

                    new StudentGrade { CourseSectionId = secPRN232_1.Id, StudentId = st2.Id, SubjectGradeComponentId = compAss1.Id, Score = 7.0m, UpdatedBy = "gv_hungnv", UpdatedAt = DateTime.UtcNow },
                    new StudentGrade { CourseSectionId = secPRN232_1.Id, StudentId = st2.Id, SubjectGradeComponentId = compAss2.Id, Score = 7.5m, UpdatedBy = "gv_hungnv", UpdatedAt = DateTime.UtcNow }
                );
            }

            // Đăng ký môn giảng dạy cho Giảng viên
            if (!context.LecturerSubjects.Any())
            {
                context.LecturerSubjects.AddRange(
                    new LecturerSubject { LecturerId = gv1.Id, SubjectId = subPRF192.Id, MajorId = majorSE.Id, SpecializationId = specSE_IS.Id, RegisteredAt = DateTime.UtcNow },
                    new LecturerSubject { LecturerId = gv1.Id, SubjectId = subPRO192.Id, MajorId = majorSE.Id, SpecializationId = specSE_IS.Id, RegisteredAt = DateTime.UtcNow },
                    new LecturerSubject { LecturerId = gv1.Id, SubjectId = subPRN211.Id, MajorId = majorSE.Id, SpecializationId = specSE_IS.Id, RegisteredAt = DateTime.UtcNow },
                    new LecturerSubject { LecturerId = gv1.Id, SubjectId = subPRN232.Id, MajorId = majorSE.Id, SpecializationId = specSE_IS.Id, RegisteredAt = DateTime.UtcNow },
                    new LecturerSubject { LecturerId = gv2.Id, SubjectId = subPRN232.Id, MajorId = majorSE.Id, SpecializationId = specSE_JS.Id, RegisteredAt = DateTime.UtcNow },
                    new LecturerSubject { LecturerId = gv2.Id, SubjectId = subSWP391.Id, MajorId = majorSE.Id, SpecializationId = specSE_JS.Id, RegisteredAt = DateTime.UtcNow }
                );
                context.SaveChanges();
            }

            // Ghi Audit Log ban đầu
            context.AuditLogs.Add(new AuditLog
            {
                Action = "SYSTEM_INITIALIZED",
                EntityName = "Database",
                EntityId = "ALL",
                PerformedBy = "System",
                PerformedAt = DateTime.UtcNow,
                Details = "Khởi tạo hệ thống SMMS và nạp dữ liệu chuẩn cho ngành Kỹ thuật phần mềm (K17, K18)"
            });

            context.SaveChanges();
        }
    }
}

