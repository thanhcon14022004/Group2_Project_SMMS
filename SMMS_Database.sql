/*
========================================================================================
  HỆ THỐNG QUẢN LÝ ĐIỂM SINH VIÊN (SMMS - Student Mark Management System)
  MÔN HỌC: PRN232 (ASP.NET Core & C#)
  DATABASE SCRIPT: TẠO CƠ SỞ DỮ LIỆU & NẠP DỮ LIỆU MẪU CHUẨN (DDL + DML)
========================================================================================
  * Hỗ trợ: Microsoft SQL Server 2017 / 2019 / 2022 / LocalDB / Azure SQL Database
  * Tên CSDL: SMMS_PRN232
  * Cách chạy:
    - Cách 1: Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio -> Nhấn F5 (Execute).
    - Cách 2: Chạy lệnh terminal:
        sqlcmd -S "(localdb)\mssqllocaldb" -E -i SMMS_Database.sql

  * DANH SÁCH TÀI KHOẢN MẪU KHỞI TẠO (Mật khẩu chung cho tất cả: Admin@123):
    1. Quản trị viên (Admin):
       - Username: admin         | Password: Admin@123 (Quản trị toàn hệ thống)
    2. Giảng viên (Lecturer):
       - Username: gv_hungnv     | Password: Admin@123 (ThS. Nguyễn Văn Hùng - Giảng viên PRN232)
       - Username: gv_huonglt    | Password: Admin@123 (TS. Lê Thị Hương - Giảng viên SWP391)
    3. Sinh viên (Student):
       - Username: he170001      | Password: Admin@123 (Trần Quang Minh - Lớp SE1701, Kỳ 5 SE-IS)
       - Username: he170002      | Password: Admin@123 (Lê Hoàng Nam    - Lớp SE1701, Kỳ 5 SE-JS)
       - Username: he170003      | Password: Admin@123 (Phạm Mai Anh   - Lớp SE1702, Kỳ 4 Đang chọn CN)

  * LƯU Ý LÀM MỚI (RESET DATABASE):
    Nếu bạn muốn xóa sạch cơ sở dữ liệu cũ để tạo lại từ đầu, hãy bỏ comment 2 dòng sau:
    -- USE master;
    -- IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SMMS_PRN232') DROP DATABASE [SMMS_PRN232];
========================================================================================
*/

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SMMS_PRN232')
BEGIN
    PRINT N'==> Tạo mới cơ sở dữ liệu SMMS_PRN232...';
    CREATE DATABASE [SMMS_PRN232];
END
GO

USE [SMMS_PRN232];
GO

PRINT N'==> Đang sử dụng cơ sở dữ liệu SMMS_PRN232...';
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [Action] nvarchar(max) NOT NULL,
        [EntityName] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NULL,
        [PerformedBy] nvarchar(max) NOT NULL,
        [PerformedAt] datetime2 NOT NULL,
        [Details] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [BackupRecords] (
        [Id] int NOT NULL IDENTITY,
        [BackupFileName] nvarchar(max) NOT NULL,
        [BackupPath] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [Notes] nvarchar(max) NULL,
        [IsAutomated] bit NOT NULL,
        [LastRestoredAt] datetime2 NULL,
        [LastRestoredBy] nvarchar(max) NULL,
        CONSTRAINT [PK_BackupRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Majors] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Majors] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Semesters] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TransferClassDeadline] datetime2 NOT NULL,
        [GradeEntryDeadline] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [IsClosedWithExceptions] bit NOT NULL,
        [CloseReason] nvarchar(max) NULL,
        CONSTRAINT [PK_Semesters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Subjects] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Credits] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [MajorId] int NULL,
        [SpecializationId] int NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Subjects] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(450) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Role] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Specializations] (
        [Id] int NOT NULL IDENTITY,
        [MajorId] int NULL,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Specializations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Specializations_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [PrerequisiteSubjects] (
        [Id] int NOT NULL IDENTITY,
        [SubjectId] int NOT NULL,
        [PrerequisiteSubjectId] int NOT NULL,
        [MinScore] decimal(4,2) NOT NULL,
        CONSTRAINT [PK_PrerequisiteSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrerequisiteSubjects_Subjects_PrerequisiteSubjectId] FOREIGN KEY ([PrerequisiteSubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PrerequisiteSubjects_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [SubjectGradeComponents] (
        [Id] int NOT NULL IDENTITY,
        [SubjectId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Weight] decimal(5,2) NOT NULL,
        [Scale] decimal(5,2) NOT NULL,
        [IsAdminOnly] bit NOT NULL,
        [OrderIndex] int NOT NULL,
        CONSTRAINT [PK_SubjectGradeComponents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubjectGradeComponents_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Lecturers] (
        [Id] int NOT NULL IDENTITY,
        [LecturerCode] nvarchar(450) NOT NULL,
        [UserId] int NOT NULL,
        [Department] nvarchar(max) NOT NULL,
        [AcademicTitle] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Lecturers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lecturers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [AdministrativeClasses] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [MajorId] int NULL,
        [Cohort] int NOT NULL,
        [AcademicYear] nvarchar(max) NOT NULL,
        [SpecializationId] int NULL,
        [CurrentSemester] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_AdministrativeClasses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdministrativeClasses_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdministrativeClasses_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [CurriculumConfigs] (
        [Id] int NOT NULL IDENTITY,
        [Version] nvarchar(max) NOT NULL,
        [MajorId] int NULL,
        [SemesterNumber] int NOT NULL,
        [SpecializationId] int NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CurriculumConfigs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CurriculumConfigs_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CurriculumConfigs_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [CourseSections] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SubjectId] int NOT NULL,
        [SemesterId] int NOT NULL,
        [LecturerId] int NULL,
        [AdministrativeClassId] int NULL,
        [MaxCapacity] int NOT NULL,
        [IsGradeLocked] bit NOT NULL,
        [GradeLockedAt] datetime2 NULL,
        [GradeLockedBy] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CourseSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSections_Lecturers_LecturerId] FOREIGN KEY ([LecturerId]) REFERENCES [Lecturers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_CourseSections_AdministrativeClasses_AdministrativeClassId] FOREIGN KEY ([AdministrativeClassId]) REFERENCES [AdministrativeClasses] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_CourseSections_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourseSections_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] int NOT NULL IDENTITY,
        [StudentCode] nvarchar(450) NOT NULL,
        [UserId] int NOT NULL,
        [MajorId] int NULL,
        [SpecializationId] int NULL,
        [AdministrativeClassId] int NULL,
        [Cohort] int NOT NULL,
        [CurrentSemester] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Students_AdministrativeClasses_AdministrativeClassId] FOREIGN KEY ([AdministrativeClassId]) REFERENCES [AdministrativeClasses] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Students_Majors_MajorId] FOREIGN KEY ([MajorId]) REFERENCES [Majors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Students_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Students_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [CurriculumSubjects] (
        [Id] int NOT NULL IDENTITY,
        [CurriculumConfigId] int NOT NULL,
        [SubjectId] int NOT NULL,
        [IsCompulsory] bit NOT NULL,
        CONSTRAINT [PK_CurriculumSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CurriculumSubjects_CurriculumConfigs_CurriculumConfigId] FOREIGN KEY ([CurriculumConfigId]) REFERENCES [CurriculumConfigs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CurriculumSubjects_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [CourseSectionSchedules] (
        [Id] int NOT NULL IDENTITY,
        [CourseSectionId] int NOT NULL,
        [DayOfWeek] int NOT NULL,
        [Slot] int NOT NULL,
        [Room] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CourseSectionSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSectionSchedules_CourseSections_CourseSectionId] FOREIGN KEY ([CourseSectionId]) REFERENCES [CourseSections] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [ClassTransferRequests] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [CurrentAdministrativeClassId] int NOT NULL,
        [TargetAdministrativeClassId] int NOT NULL,
        [SemesterId] int NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [RequestedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [ProcessedBy] nvarchar(max) NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_ClassTransferRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClassTransferRequests_AdministrativeClasses_CurrentAdministrativeClassId] FOREIGN KEY ([CurrentAdministrativeClassId]) REFERENCES [AdministrativeClasses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClassTransferRequests_AdministrativeClasses_TargetAdministrativeClassId] FOREIGN KEY ([TargetAdministrativeClassId]) REFERENCES [AdministrativeClasses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClassTransferRequests_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClassTransferRequests_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [CourseSectionStudents] (
        [Id] int NOT NULL IDENTITY,
        [CourseSectionId] int NOT NULL,
        [StudentId] int NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CourseSectionStudents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSectionStudents_CourseSections_CourseSectionId] FOREIGN KEY ([CourseSectionId]) REFERENCES [CourseSections] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourseSectionStudents_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [SpecializationSelections] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [SpecializationId] int NOT NULL,
        [SemesterId] int NOT NULL,
        [SelectedAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SpecializationSelections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SpecializationSelections_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SpecializationSelections_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SpecializationSelections_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE TABLE [StudentGrades] (
        [Id] int NOT NULL IDENTITY,
        [CourseSectionId] int NOT NULL,
        [StudentId] int NOT NULL,
        [SubjectGradeComponentId] int NOT NULL,
        [Score] decimal(4,2) NULL,
        [Notes] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_StudentGrades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentGrades_CourseSections_CourseSectionId] FOREIGN KEY ([CourseSectionId]) REFERENCES [CourseSections] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentGrades_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StudentGrades_SubjectGradeComponents_SubjectGradeComponentId] FOREIGN KEY ([SubjectGradeComponentId]) REFERENCES [SubjectGradeComponents] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AdministrativeClasses_Code] ON [AdministrativeClasses] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AdministrativeClasses_MajorId] ON [AdministrativeClasses] ([MajorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AdministrativeClasses_SpecializationId] ON [AdministrativeClasses] ([SpecializationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClassTransferRequests_CurrentAdministrativeClassId] ON [ClassTransferRequests] ([CurrentAdministrativeClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClassTransferRequests_SemesterId] ON [ClassTransferRequests] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClassTransferRequests_StudentId] ON [ClassTransferRequests] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClassTransferRequests_TargetAdministrativeClassId] ON [ClassTransferRequests] ([TargetAdministrativeClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CourseSections_Code] ON [CourseSections] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CourseSections_LecturerId] ON [CourseSections] ([LecturerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CourseSections_SemesterId] ON [CourseSections] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CourseSections_SubjectId] ON [CourseSections] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CourseSectionSchedules_CourseSectionId] ON [CourseSectionSchedules] ([CourseSectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CourseSectionStudents_CourseSectionId_StudentId] ON [CourseSectionStudents] ([CourseSectionId], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CourseSectionStudents_StudentId] ON [CourseSectionStudents] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurriculumConfigs_MajorId] ON [CurriculumConfigs] ([MajorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurriculumConfigs_SpecializationId] ON [CurriculumConfigs] ([SpecializationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurriculumSubjects_CurriculumConfigId] ON [CurriculumSubjects] ([CurriculumConfigId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurriculumSubjects_SubjectId] ON [CurriculumSubjects] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lecturers_LecturerCode] ON [Lecturers] ([LecturerCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lecturers_UserId] ON [Lecturers] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Majors_Code] ON [Majors] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrerequisiteSubjects_PrerequisiteSubjectId] ON [PrerequisiteSubjects] ([PrerequisiteSubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrerequisiteSubjects_SubjectId] ON [PrerequisiteSubjects] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Semesters_Code] ON [Semesters] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Specializations_Code] ON [Specializations] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Specializations_MajorId] ON [Specializations] ([MajorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SpecializationSelections_SemesterId] ON [SpecializationSelections] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SpecializationSelections_SpecializationId] ON [SpecializationSelections] ([SpecializationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SpecializationSelections_StudentId] ON [SpecializationSelections] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentGrades_CourseSectionId_StudentId_SubjectGradeComponentId] ON [StudentGrades] ([CourseSectionId], [StudentId], [SubjectGradeComponentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentGrades_StudentId] ON [StudentGrades] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentGrades_SubjectGradeComponentId] ON [StudentGrades] ([SubjectGradeComponentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_AdministrativeClassId] ON [Students] ([AdministrativeClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_MajorId] ON [Students] ([MajorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_SpecializationId] ON [Students] ([SpecializationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_StudentCode] ON [Students] ([StudentCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SubjectGradeComponents_SubjectId] ON [SubjectGradeComponents] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Subjects_Code] ON [Subjects] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261101190100_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261101190100_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO



-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Majors
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Majors] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Majors] ON;
    INSERT INTO [Majors] ([Id], [Code], [Name], [Description], [IsActive]) VALUES
(1, N'SE', N'Kỹ thuật phần mềm (Software Engineering)', N'Đào tạo kỹ sư phần mềm chất lượng cao', 1),
(2, N'IA', N'An toàn thông tin (Information Assurance)', N'Đào tạo chuyên gia bảo mật và phòng thủ không gian mạng', 1);
    SET IDENTITY_INSERT [Majors] OFF;
    PRINT N'==> Đã nạp 2 bản ghi mẫu vào bảng Majors';
END
ELSE
BEGIN
    PRINT N'==> Bảng Majors đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Specializations
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Specializations] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Specializations] ON;
    INSERT INTO [Specializations] ([Id], [MajorId], [Code], [Name], [Description], [IsActive]) VALUES
(1, 1, N'SE-IS', N'Hệ thống thông tin doanh nghiệp (Information Systems)', N'Chuyên sâu về kiến trúc hệ thống và ERP', 1),
(2, 1, N'SE-JS', N'Kỹ thuật phần mềm Nhật Bản (Japanese Software)', N'Chuyên sâu về cầu nối phát triển phần mềm cho thị trường Nhật', 1);
    SET IDENTITY_INSERT [Specializations] OFF;
    PRINT N'==> Đã nạp 2 bản ghi mẫu vào bảng Specializations';
END
ELSE
BEGIN
    PRINT N'==> Bảng Specializations đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Subjects
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Subjects] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Subjects] ON;
    INSERT INTO [Subjects] ([Id], [Code], [Name], [Credits], [Description], [IsActive]) VALUES
(1, N'PRF192', N'Nhập môn lập trình C', 3, NULL, 1),
(2, N'PRO192', N'Lập trình hướng đối tượng Java', 3, NULL, 1),
(3, N'PRN211', N'Lập trình C# cơ bản & Windows Apps', 3, NULL, 1),
(4, N'PRN232', N'Xây dựng ứng dụng Web với ASP.NET Core', 3, NULL, 1),
(5, N'SWP391', N'Dự án phát triển phần mềm thực chiến', 3, NULL, 1);
    SET IDENTITY_INSERT [Subjects] OFF;
    PRINT N'==> Đã nạp 5 bản ghi mẫu vào bảng Subjects';
END
ELSE
BEGIN
    PRINT N'==> Bảng Subjects đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: SubjectGradeComponents
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [SubjectGradeComponents] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [SubjectGradeComponents] ON;
    INSERT INTO [SubjectGradeComponents] ([Id], [SubjectId], [Name], [Weight], [Scale], [IsAdminOnly], [OrderIndex]) VALUES
(1, 1, N'Lab', 20.00, 10.00, 0, 1),
(2, 1, N'Assignment', 20.00, 10.00, 0, 2),
(3, 1, N'Progress Test', 20.00, 10.00, 0, 3),
(4, 1, N'Final Exam', 40.00, 10.00, 1, 4),
(5, 2, N'Quiz', 10.00, 10.00, 0, 1),
(6, 2, N'Lab', 20.00, 10.00, 0, 2),
(7, 2, N'Practical Exam', 30.00, 10.00, 0, 3),
(8, 2, N'Final Exam', 40.00, 10.00, 1, 4),
(9, 3, N'Lab Exercises', 20.00, 10.00, 0, 1),
(10, 3, N'Progress Test', 20.00, 10.00, 0, 2),
(11, 3, N'Practical Exam', 20.00, 10.00, 0, 3),
(12, 3, N'Final Exam', 40.00, 10.00, 1, 4),
(13, 4, N'Assignment 1', 15.00, 10.00, 0, 1),
(14, 4, N'Assignment 2', 15.00, 10.00, 0, 2),
(15, 4, N'Progress Test', 20.00, 10.00, 0, 3),
(16, 4, N'Final Project Defense', 50.00, 10.00, 1, 4),
(17, 5, N'Sprint 1', 20.00, 10.00, 0, 1),
(18, 5, N'Sprint 2', 20.00, 10.00, 0, 2),
(19, 5, N'Sprint 3', 20.00, 10.00, 0, 3),
(20, 5, N'Final Presentation', 40.00, 10.00, 1, 4);
    SET IDENTITY_INSERT [SubjectGradeComponents] OFF;
    PRINT N'==> Đã nạp 20 bản ghi mẫu vào bảng SubjectGradeComponents';
END
ELSE
BEGIN
    PRINT N'==> Bảng SubjectGradeComponents đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: PrerequisiteSubjects
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [PrerequisiteSubjects] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [PrerequisiteSubjects] ON;
    INSERT INTO [PrerequisiteSubjects] ([Id], [SubjectId], [PrerequisiteSubjectId], [MinScore]) VALUES
(1, 2, 1, 5.00),
(2, 3, 2, 5.00),
(3, 4, 3, 5.00),
(4, 5, 3, 5.00);
    SET IDENTITY_INSERT [PrerequisiteSubjects] OFF;
    PRINT N'==> Đã nạp 4 bản ghi mẫu vào bảng PrerequisiteSubjects';
END
ELSE
BEGIN
    PRINT N'==> Bảng PrerequisiteSubjects đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Users
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [FullName], [Email], [PhoneNumber], [Role], [IsActive], [CreatedAt], [LastLoginAt]) VALUES
(1, N'admin', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'Quản trị viên Hệ thống SMMS', N'admin@smms.edu.vn', N'0987654321', N'Admin', 1, '2026-11-01 19:24:36.297', '2026-11-01 19:24:42.472'),
(2, N'gv_hungnv', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'ThS. Nguyễn Văn Hùng', N'hungnv@smms.edu.vn', N'0912345678', N'Lecturer', 1, '2026-11-01 19:24:36.297', NULL),
(3, N'gv_huonglt', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'TS. Lê Thị Hương', N'huonglt@smms.edu.vn', N'0923456789', N'Lecturer', 1, '2026-11-01 19:24:36.297', NULL),
(4, N'he170001', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'Trần Quang Minh', N'minhtqhe170001@fpt.edu.vn', N'0934567890', N'Student', 1, '2026-11-01 19:24:36.297', NULL),
(5, N'he170002', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'Lê Hoàng Nam', N'namlhhe170002@fpt.edu.vn', N'0945678901', N'Student', 1, '2026-11-01 19:24:36.297', NULL),
(6, N'he170003', N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW', N'Phạm Mai Anh', N'anhpmhe170003@fpt.edu.vn', N'0956789012', N'Student', 1, '2026-11-01 19:24:36.297', NULL);
    SET IDENTITY_INSERT [Users] OFF;
    PRINT N'==> Đã nạp 6 bản ghi mẫu vào bảng Users';
END
ELSE
BEGIN
    PRINT N'==> Bảng Users đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Lecturers
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Lecturers] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Lecturers] ON;
    INSERT INTO [Lecturers] ([Id], [LecturerCode], [UserId], [Department], [AcademicTitle]) VALUES
(1, N'GV001', 2, N'Software Engineering', N'Master'),
(2, N'GV002', 3, N'Software Engineering', N'PhD');
    SET IDENTITY_INSERT [Lecturers] OFF;
    PRINT N'==> Đã nạp 2 bản ghi mẫu vào bảng Lecturers';
END
ELSE
BEGIN
    PRINT N'==> Bảng Lecturers đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: AdministrativeClasses
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [AdministrativeClasses] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [AdministrativeClasses] ON;
    INSERT INTO [AdministrativeClasses] ([Id], [Code], [Name], [MajorId], [Cohort], [AcademicYear], [SpecializationId], [CurrentSemester], [IsActive]) VALUES
(1, N'SE1701', N'Kỹ thuật phần mềm K17 Lớp 1', 1, 17, N'2023-2027', NULL, 5, 1),
(2, N'SE1702', N'Kỹ thuật phần mềm K17 Lớp 2', 1, 17, N'2023-2027', NULL, 5, 1);
    SET IDENTITY_INSERT [AdministrativeClasses] OFF;
    PRINT N'==> Đã nạp 2 bản ghi mẫu vào bảng AdministrativeClasses';
END
ELSE
BEGIN
    PRINT N'==> Bảng AdministrativeClasses đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Students
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Students] ON;
    INSERT INTO [Students] ([Id], [StudentCode], [UserId], [MajorId], [SpecializationId], [AdministrativeClassId], [Cohort], [CurrentSemester], [Status]) VALUES
(1, N'SE170001', 4, 1, 1, 1, 17, 5, N'Studying'),
(2, N'SE170002', 5, 1, 2, 1, 17, 5, N'Studying'),
(3, N'SE170003', 6, 1, NULL, 2, 17, 4, N'Studying');
    SET IDENTITY_INSERT [Students] OFF;
    PRINT N'==> Đã nạp 3 bản ghi mẫu vào bảng Students';
END
ELSE
BEGIN
    PRINT N'==> Bảng Students đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Bảng: CurriculumConfigs & CurriculumSubjects (Để trống cấu hình để Quản trị viên tự thiết lập)
-- ======================================================
PRINT N'==> Bảng CurriculumConfigs & CurriculumSubjects sẵn sàng (để trống cấu hình)';
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Semesters
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Semesters] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Semesters] ON;
    INSERT INTO [Semesters] ([Id], [Code], [Name], [StartDate], [EndDate], [TransferClassDeadline], [GradeEntryDeadline], [Status], [IsClosedWithExceptions], [CloseReason]) VALUES
(1, N'FA24', N'Fall 2024', '2026-03-01 19:24:36.704', '2026-07-01 19:24:36.704', '2026-04-01 19:24:36.704', '2026-06-26 19:24:36.704', N'Ended', 0, NULL),
(2, N'SP25', N'Spring 2025', '2026-07-01 19:24:36.704', '2026-10-17 19:24:36.704', '2026-08-01 19:24:36.704', '2026-10-22 19:24:36.704', N'Ended', 0, NULL),
(3, N'SU25', N'Summer 2025', '2026-10-22 19:24:36.704', '2027-02-01 19:24:36.704', '2026-11-15 19:24:36.704', '2027-01-27 19:24:36.704', N'Active', 0, NULL);
    SET IDENTITY_INSERT [Semesters] OFF;
    PRINT N'==> Đã nạp 3 bản ghi mẫu vào bảng Semesters';
END
ELSE
BEGIN
    PRINT N'==> Bảng Semesters đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: CourseSections
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [CourseSections] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [CourseSections] ON;
    INSERT INTO [CourseSections] ([Id], [Code], [Name], [SubjectId], [SemesterId], [LecturerId], [MaxCapacity], [IsGradeLocked], [GradeLockedAt], [GradeLockedBy], [Status], [CreatedAt]) VALUES
(1, N'PRN232_SU25_SE1701', N'Lập trình Web ASP.NET Core - Lớp SE1701', 4, 3, 1, 30, 0, NULL, NULL, N'Open', '2026-11-01 19:24:36.728'),
(2, N'SWP391_SU25_SE1701', N'Dự án phát triển phần mềm - Lớp SE1701', 5, 3, 2, 30, 0, NULL, NULL, N'Open', '2026-11-01 19:24:36.728');
    SET IDENTITY_INSERT [CourseSections] OFF;
    PRINT N'==> Đã nạp 2 bản ghi mẫu vào bảng CourseSections';
END
ELSE
BEGIN
    PRINT N'==> Bảng CourseSections đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: CourseSectionSchedules
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [CourseSectionSchedules] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [CourseSectionSchedules] ON;
    INSERT INTO [CourseSectionSchedules] ([Id], [CourseSectionId], [DayOfWeek], [Slot], [Room]) VALUES
(1, 1, 1, 2, N'BE-302'),
(2, 1, 4, 2, N'BE-302'),
(3, 2, 2, 3, N'AL-204'),
(4, 2, 5, 3, N'AL-204');
    SET IDENTITY_INSERT [CourseSectionSchedules] OFF;
    PRINT N'==> Đã nạp 4 bản ghi mẫu vào bảng CourseSectionSchedules';
END
ELSE
BEGIN
    PRINT N'==> Bảng CourseSectionSchedules đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: CourseSectionStudents
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [CourseSectionStudents] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [CourseSectionStudents] ON;
    INSERT INTO [CourseSectionStudents] ([Id], [CourseSectionId], [StudentId], [EnrolledAt], [Status]) VALUES
(1, 1, 1, '2026-11-01 19:24:36.773', N'Enrolled'),
(2, 1, 2, '2026-11-01 19:24:36.773', N'Enrolled'),
(3, 2, 1, '2026-11-01 19:24:36.773', N'Enrolled'),
(4, 2, 2, '2026-11-01 19:24:36.773', N'Enrolled');
    SET IDENTITY_INSERT [CourseSectionStudents] OFF;
    PRINT N'==> Đã nạp 4 bản ghi mẫu vào bảng CourseSectionStudents';
END
ELSE
BEGIN
    PRINT N'==> Bảng CourseSectionStudents đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: StudentGrades
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [StudentGrades] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [StudentGrades] ON;
    INSERT INTO [StudentGrades] ([Id], [CourseSectionId], [StudentId], [SubjectGradeComponentId], [Score], [Notes], [UpdatedAt], [UpdatedBy]) VALUES
(1, 1, 1, 13, 8.50, NULL, '2026-11-01 19:24:36.838', N'gv_hungnv'),
(2, 1, 1, 14, 9.00, NULL, '2026-11-01 19:24:36.839', N'gv_hungnv'),
(3, 1, 1, 15, 8.00, NULL, '2026-11-01 19:24:36.839', N'gv_hungnv'),
(4, 1, 2, 13, 7.00, NULL, '2026-11-01 19:24:36.839', N'gv_hungnv'),
(5, 1, 2, 14, 7.50, NULL, '2026-11-01 19:24:36.839', N'gv_hungnv');
    SET IDENTITY_INSERT [StudentGrades] OFF;
    PRINT N'==> Đã nạp 5 bản ghi mẫu vào bảng StudentGrades';
END
ELSE
BEGIN
    PRINT N'==> Bảng StudentGrades đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: AuditLogs
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [AuditLogs] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [AuditLogs] ON;
    INSERT INTO [AuditLogs] ([Id], [Action], [EntityName], [EntityId], [PerformedBy], [PerformedAt], [Details], [IpAddress]) VALUES
(1, N'SYSTEM_INITIALIZED', N'Database', N'ALL', N'System', '2026-11-01 19:24:36.852', N'Khởi tạo hệ thống SMMS và nạp dữ liệu chuẩn cho ngành Kỹ thuật phần mềm (K17, K18)', NULL);
    SET IDENTITY_INSERT [AuditLogs] OFF;
    PRINT N'==> Đã nạp 1 bản ghi mẫu vào bảng AuditLogs';
END
ELSE
BEGIN
    PRINT N'==> Bảng AuditLogs đã có dữ liệu, bỏ qua bước nạp mẫu';
END;
GO






USE [SMMS_PRN232];
GO

SET NOCOUNT ON;

PRINT N'==> BẮT ĐẦU NẠP DỮ LIỆU ĐẦY ĐỦ CHƯƠNG TRÌNH ĐÀO TẠO ĐẠI HỌC FPT...';

-- 1. NGÀNH ĐÀO TẠO (MAJORS)
IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'AI')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('AI', N'Trí tuệ nhân tạo (Artificial Intelligence)', N'Đào tạo chuyên gia AI, Machine Learning và Khoa học dữ liệu', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'DS')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('DS', N'Khoa học dữ liệu (Data Science)', N'Đào tạo kỹ sư phân tích dữ liệu lớn và trực quan hóa kinh doanh', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'IT')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('IT', N'Công nghệ thông tin (Information Technology)', N'Đào tạo kỹ sư CNTT tổng hợp, Cloud và Quản trị hệ thống', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'GD')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('GD', N'Thiết kế Mỹ thuật số (Digital Art & Graphic Design)', N'Đào tạo thiết kế đồ họa 2D/3D, UI/UX và truyền thông hình ảnh', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'BA')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('BA', N'Quản trị kinh doanh (Business Administration)', N'Đào tạo cử nhân QTKD, Digital Marketing và Kinh doanh quốc tế', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'MC')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('MC', N'Truyền thông đa phương tiện (Multimedia Communication)', N'Đào tạo chuyên viên truyền thông số, báo chí và tổ chức sự kiện', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'EN')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('EN', N'Ngôn ngữ Anh (English Language)', N'Đào tạo chuyên gia ngôn ngữ Anh thương mại và biên phiên dịch', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'JP')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('JP', N'Ngôn ngữ Nhật (Japanese Language)', N'Đào tạo cử nhân tiếng Nhật CNTT và tiếng Nhật thương mại', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'KR')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('KR', N'Ngôn ngữ Hàn (Korean Language)', N'Đào tạo cử nhân tiếng Hàn thương mại và truyền thông', 1);

PRINT N'==> Đã cập nhật 11 ngành đào tạo FPT.';

-- Lấy ID các ngành
DECLARE @idSE INT = (SELECT Id FROM Majors WHERE Code = 'SE');
DECLARE @idIA INT = (SELECT Id FROM Majors WHERE Code = 'IA');
DECLARE @idAI INT = (SELECT Id FROM Majors WHERE Code = 'AI');
DECLARE @idDS INT = (SELECT Id FROM Majors WHERE Code = 'DS');
DECLARE @idIT INT = (SELECT Id FROM Majors WHERE Code = 'IT');
DECLARE @idGD INT = (SELECT Id FROM Majors WHERE Code = 'GD');
DECLARE @idBA INT = (SELECT Id FROM Majors WHERE Code = 'BA');
DECLARE @idMC INT = (SELECT Id FROM Majors WHERE Code = 'MC');
DECLARE @idEN INT = (SELECT Id FROM Majors WHERE Code = 'EN');
DECLARE @idJP INT = (SELECT Id FROM Majors WHERE Code = 'JP');
DECLARE @idKR INT = (SELECT Id FROM Majors WHERE Code = 'KR');

-- 2. CHUYÊN NGÀNH HẸP (SPECIALIZATIONS)
-- SE
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'SE-ES')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idSE, 'SE-ES', N'Hệ thống nhúng & IoT (Embedded Systems & IoT)', N'Lập trình vi điều khiển, hệ thống nhúng và thiết bị thông minh', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'SE-AS')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idSE, 'SE-AS', N'Kỹ thuật phần mềm ô tô (Automotive Software)', N'Phát triển phần mềm điều khiển và xe tự hành', 1);

-- IA
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IA-NW')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIA, 'IA-NW', N'An ninh mạng & Điện toán đám mây (Cyber & Cloud Security)', N'Bảo mật hạ tầng mạng, phòng chống tấn công DDoS', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IA-DF')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIA, 'IA-DF', N'Điều tra số & Ứng phó sự cố an ninh (Digital Forensics)', N'Khôi phục chứng cứ số và giám định bảo mật', 1);

-- AI
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'AI-ML')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idAI, 'AI-ML', N'Học máy & Học sâu ứng dụng (Machine Learning & Deep Learning)', N'Xây dựng các mô hình AI dự báo và huấn luyện mạng nơ-ron', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'AI-CV')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idAI, 'AI-CV', N'Thị giác máy tính & Xử lý ngôn ngữ tự nhiên (Computer Vision & NLP)', N'Nhận diện khuôn mặt, chatbot và xử lý văn bản', 1);

-- DS
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'DS-BA')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idDS, 'DS-BA', N'Phân tích dữ liệu kinh doanh (Business Analytics & BI)', N'Phân tích insight khách hàng và tối ưu chiến lược kinh doanh', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'DS-DE')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idDS, 'DS-DE', N'Kỹ thuật dữ liệu lớn (Big Data Engineering)', N'Xây dựng Data Pipeline, Spark, Hadoop và hồ dữ liệu', 1);

-- IT
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IT-WD')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIT, 'IT-WD', N'Phát triển ứng dụng Web Full-stack (Full-stack Web Development)', N'Thiết kế và phát triển web hiện đại với Frontend & Backend', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IT-SA')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIT, 'IT-SA', N'Quản trị hệ thống & Điện toán đám mây (Cloud & Systems)', N'Triển khai AWS/Azure, CI/CD và DevOps', 1);

-- GD
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'GD-3D')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idGD, 'GD-3D', N'Thiết kế đồ họa 3D & Kỹ xảo hình ảnh (3D Animation & VFX)', N'Dựng hình nhân vật 3D, làm hoạt hình và kỹ xảo điện ảnh', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'GD-UX')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idGD, 'GD-UX', N'Thiết kế giao diện & Trải nghiệm người dùng (UI/UX Design)', N'Nghiên cứu hành vi người dùng, thiết kế wireframe và prototype', 1);

-- BA
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'BA-DM')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idBA, 'BA-DM', N'Digital Marketing (Marketing số)', N'SEO, Google/Facebook Ads, Content Strategy và Social Media', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'BA-IB')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idBA, 'BA-IB', N'Kinh doanh quốc tế (International Business)', N'Thương mại toàn cầu, xuất nhập khẩu và chuỗi cung ứng logistics', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'BA-HM')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idBA, 'BA-HM', N'Quản trị Khách sạn & Du lịch (Hospitality Management)', N'Vận hành khách sạn, resort và dịch vụ du lịch quốc tế', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'BA-FT')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idBA, 'BA-FT', N'Công nghệ tài chính (FinTech)', N'Ngân hàng số, thanh toán điện tử và phân tích đầu tư', 1);

-- MC
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'MC-PR')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idMC, 'MC-PR', N'Quan hệ công chúng & Quản trị sự kiện (PR & Event)', N'Truyền thông thương hiệu, xử lý khủng hoảng và tổ chức sự kiện', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'MC-NM')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idMC, 'MC-NM', N'Báo chí số & Sáng tạo nội dung (Digital Journalism)', N'Sản xuất video, podcast và báo chí truyền thông số', 1);

-- EN
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'EN-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idEN, 'EN-BC', N'Tiếng Anh thương mại (Business Communication)', N'Giao tiếp đàm phán quốc tế và hợp đồng thương mại', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'EN-TI')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idEN, 'EN-TI', N'Tiếng Anh Biên phiên dịch (Translation & Interpretation)', N'Biên dịch tài liệu chuyên ngành và phiên dịch hội thảo', 1);

-- JP
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'JP-IT')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idJP, 'JP-IT', N'Tiếng Nhật Công nghệ thông tin (IT Japanese)', N'Cầu nối phát triển phần mềm Bridge SE cho doanh nghiệp Nhật', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'JP-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idJP, 'JP-BC', N'Tiếng Nhật Thương mại (Business Japanese)', N'Giao tiếp văn phòng và thương mại với đối tác Nhật Bản', 1);

-- KR
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'KR-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idKR, 'KR-BC', N'Tiếng Hàn Thương mại (Business Korean)', N'Làm việc tại các tập đoàn đa quốc gia Hàn Quốc (Samsung, LG...)', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'KR-TI')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idKR, 'KR-TI', N'Tiếng Hàn Biên phiên dịch (Korean Translation)', N'Dịch thuật cabin, hội nghị và tài liệu kinh tế', 1);

PRINT N'==> Đã cập nhật 26 chuyên ngành hẹp FPT.';

-- 3. HỌC KỲ (SEMESTERS)
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'FA23')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('FA23', N'Học kỳ Fall 2023', '2023-09-05', '2023-12-25', '2023-09-20', '2023-12-30', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SP24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SP24', N'Học kỳ Spring 2024', '2024-01-08', '2024-04-25', '2024-01-20', '2024-04-30', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SU24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SU24', N'Học kỳ Summer 2024', '2024-05-06', '2024-08-20', '2024-05-18', '2024-08-25', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'FA24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('FA24', N'Học kỳ Fall 2024 (Hiện tại)', '2024-09-03', '2024-12-28', '2024-09-18', '2024-12-30', 'Active', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SP25')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SP25', N'Học kỳ Spring 2025 (Sắp tới)', '2025-01-06', '2025-04-26', '2025-01-18', '2025-04-30', 'Upcoming', 0);

PRINT N'==> Đã cập nhật các học kỳ.';

-- 4. DANH SÁCH MÔN HỌC CHUẨN FPT (SUBJECTS)
CREATE TABLE #TmpSubjects (Code NVARCHAR(50), Name NVARCHAR(255), Credits INT);
INSERT INTO #TmpSubjects VALUES
('MAE101', N'Toán cao cấp cho kỹ thuật (Mathematics for Engineering)', 3),
('MAD101', N'Toán rời rạc (Discrete Mathematics)', 3),
('MAS291', N'Xác suất và Thống kê (Probability & Statistics)', 3),
('SSL101c', N'Kỹ năng học tập đại học (Academic Skills)', 3),
('EXE101', N'Khởi nghiệp đổi mới sáng tạo (Experiential Entrepreneurship 1)', 3),
('CSD201', N'Cấu trúc dữ liệu và giải thuật (Data Structures & Algorithms)', 3),
('DBI202', N'Giới thiệu Hệ quản trị CSDL (Introduction to Databases)', 3),
('PRJ301', N'Lập trình ứng dụng Java Web (Java Web Application Development)', 3),
('SWE201c', N'Nhập môn Kỹ thuật phần mềm (Introduction to Software Engineering)', 3),
('SWT301', N'Kiểm thử phần mềm (Software Testing)', 3),
('SWR302', N'Kỹ thuật yêu cầu phần mềm (Software Requirements)', 3),
('SWD392', N'Kiến trúc và thiết kế phần mềm (Software Architecture & Design)', 3),
('PRM392', N'Lập trình ứng dụng di động (Mobile Programming)', 3),
('NWC203c', N'Mạng máy tính căn bản (Computer Networking)', 3),
('OSG202', N'Hệ điều hành (Operating Systems)', 3),
('IAE201', N'Nguyên lý An toàn thông tin (Principles of Information Assurance)', 3),
('IAS301', N'An ninh mạng & Phân tích lỗ hổng (Network Security & Vulnerability)', 3),
('CRY301', N'Mật mã học và ứng dụng (Cryptography)', 3),
('AIL302m', N'Trí tuệ nhân tạo & Machine Learning (Machine Learning Fundamentals)', 3),
('DBS301', N'Hệ thống cơ sở dữ liệu nâng cao & NoSQL (Advanced Database Systems)', 3),
('DVP301', N'Trực quan hóa dữ liệu với Python & BI (Data Visualization)', 3),
('DES101', N'Nguyên lý thiết kế mỹ thuật số (Principles of Visual Design)', 3),
('VFX201', N'Kỹ xảo hình ảnh & Hoạt họa kỹ thuật số (Visual Effects & 3D)', 3),
('UIX201', N'Thiết kế giao diện & Trải nghiệm người dùng (UI/UX Design Studio)', 3),
('MKT101', N'Nguyên lý Marketing (Principles of Marketing)', 3),
('ECO111', N'Kinh tế vi mô (Microeconomics)', 3),
('OBE102', N'Hành vi tổ chức (Organizational Behavior)', 3),
('FIN301', N'Quản trị tài chính doanh nghiệp (Corporate Finance)', 3),
('MMC101', N'Nhập môn Truyền thông đa phương tiện (Intro to Multimedia)', 3),
('PRS201', N'Kỹ năng viết và Quan hệ công chúng (Public Relations Writing)', 3),
('ENW492', N'Viết học thuật và Tiếng Anh nghề nghiệp (Academic English)', 3),
('JPD113', N'Tiếng Nhật sơ cấp 1 (Elementary Japanese 1)', 3),
('JPD123', N'Tiếng Nhật sơ cấp 2 (Elementary Japanese 2)', 3),
('KRD113', N'Tiếng Hàn sơ cấp 1 (Elementary Korean 1)', 3),
('KRD123', N'Tiếng Hàn sơ cấp 2 (Elementary Korean 2)', 3);

DECLARE @subCode NVARCHAR(50), @subName NVARCHAR(255), @subCred INT;
DECLARE curSub CURSOR FOR SELECT Code, Name, Credits FROM #TmpSubjects;
OPEN curSub;
FETCH NEXT FROM curSub INTO @subCode, @subName, @subCred;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Subjects WHERE Code = @subCode)
    BEGIN
        INSERT INTO Subjects (Code, Name, Credits, Description, IsActive)
        VALUES (@subCode, @subName, @subCred, N'Môn học chương trình chuẩn Đại học FPT', 1);

        DECLARE @newSubId INT = SCOPE_IDENTITY();
        -- Cấu hình đầu điểm chuẩn 100%: Quiz 10%, Lab 20%, Progress Test 30%, Final Exam 40%
        INSERT INTO SubjectGradeComponents (SubjectId, Name, Weight, Scale, IsAdminOnly, OrderIndex)
        VALUES 
        (@newSubId, N'Quiz / Điểm danh', 10.0, 10.0, 0, 1),
        (@newSubId, N'Bài thực hành (Lab)', 20.0, 10.0, 0, 2),
        (@newSubId, N'Kiểm tra tiến độ (Progress Test)', 30.0, 10.0, 0, 3),
        (@newSubId, N'Thi kết thúc môn (Final Exam)', 40.0, 10.0, 1, 4);
    END
    FETCH NEXT FROM curSub INTO @subCode, @subName, @subCred;
END
CLOSE curSub;
DEALLOCATE curSub;
DROP TABLE #TmpSubjects;

PRINT N'==> Đã cập nhật toàn bộ 40 môn học FPT và cấu hình đầu điểm 100%.';

-- Môn tiên quyết
DECLARE @idPRF INT = (SELECT Id FROM Subjects WHERE Code = 'PRF192');
DECLARE @idPRO INT = (SELECT Id FROM Subjects WHERE Code = 'PRO192');
DECLARE @idPRN211 INT = (SELECT Id FROM Subjects WHERE Code = 'PRN211');
DECLARE @idPRN232 INT = (SELECT Id FROM Subjects WHERE Code = 'PRN232');
DECLARE @idCSD INT = (SELECT Id FROM Subjects WHERE Code = 'CSD201');
DECLARE @idDBI INT = (SELECT Id FROM Subjects WHERE Code = 'DBI202');
DECLARE @idPRJ INT = (SELECT Id FROM Subjects WHERE Code = 'PRJ301');
DECLARE @idMAD INT = (SELECT Id FROM Subjects WHERE Code = 'MAD101');
DECLARE @idMAS INT = (SELECT Id FROM Subjects WHERE Code = 'MAS291');
DECLARE @idAIL INT = (SELECT Id FROM Subjects WHERE Code = 'AIL302m');
DECLARE @idJPD1 INT = (SELECT Id FROM Subjects WHERE Code = 'JPD113');
DECLARE @idJPD2 INT = (SELECT Id FROM Subjects WHERE Code = 'JPD123');
DECLARE @idKRD1 INT = (SELECT Id FROM Subjects WHERE Code = 'KRD113');
DECLARE @idKRD2 INT = (SELECT Id FROM Subjects WHERE Code = 'KRD123');

IF @idMAD IS NOT NULL AND @idCSD IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idCSD AND PrerequisiteSubjectId = @idMAD)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idCSD, @idMAD, 5.0);

IF @idPRF IS NOT NULL AND @idCSD IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idCSD AND PrerequisiteSubjectId = @idPRF)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idCSD, @idPRF, 5.0);

IF @idDBI IS NOT NULL AND @idPRJ IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idPRJ AND PrerequisiteSubjectId = @idDBI)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idPRJ, @idDBI, 5.0);

IF @idMAS IS NOT NULL AND @idAIL IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idAIL AND PrerequisiteSubjectId = @idMAS)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idAIL, @idMAS, 5.0);

IF @idJPD1 IS NOT NULL AND @idJPD2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idJPD2 AND PrerequisiteSubjectId = @idJPD1)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idJPD2, @idJPD1, 5.0);

IF @idKRD1 IS NOT NULL AND @idKRD2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PrerequisiteSubjects WHERE SubjectId = @idKRD2 AND PrerequisiteSubjectId = @idKRD1)
    INSERT INTO PrerequisiteSubjects (SubjectId, PrerequisiteSubjectId, MinScore) VALUES (@idKRD2, @idKRD1, 5.0);

PRINT N'==> Đã cập nhật cây điều kiện môn tiên quyết.';

-- 5. LỚP HÀNH CHÍNH (ADMINISTRATIVE CLASSES)
CREATE TABLE #TmpClasses (Code NVARCHAR(50), Name NVARCHAR(255), MajorCode NVARCHAR(50), Cohort INT, Year NVARCHAR(50), Sem INT);
INSERT INTO #TmpClasses VALUES
('SE1703', N'Kỹ thuật phần mềm K17 Lớp 3', 'SE', 17, '2023-2027', 5),
('SE1801', N'Kỹ thuật phần mềm K18 Lớp 1', 'SE', 18, '2024-2028', 3),
('SE1802', N'Kỹ thuật phần mềm K18 Lớp 2', 'SE', 18, '2024-2028', 3),
('IA1701', N'An toàn thông tin K17 Lớp 1', 'IA', 17, '2023-2027', 5),
('IA1801', N'An toàn thông tin K18 Lớp 1', 'IA', 18, '2024-2028', 3),
('AI1701', N'Trí tuệ nhân tạo K17 Lớp 1', 'AI', 17, '2023-2027', 5),
('AI1801', N'Trí tuệ nhân tạo K18 Lớp 1', 'AI', 18, '2024-2028', 3),
('DS1701', N'Khoa học dữ liệu K17 Lớp 1', 'DS', 17, '2023-2027', 5),
('IT1701', N'Công nghệ thông tin K17 Lớp 1', 'IT', 17, '2023-2027', 5),
('GD1701', N'Thiết kế đồ họa K17 Lớp 1', 'GD', 17, '2023-2027', 5),
('GD1801', N'Thiết kế đồ họa K18 Lớp 1', 'GD', 18, '2024-2028', 3),
('BA1701', N'Quản trị kinh doanh K17 Lớp 1', 'BA', 17, '2023-2027', 5),
('BA1702', N'Quản trị kinh doanh K17 Lớp 2', 'BA', 17, '2023-2027', 5),
('BA1801', N'Quản trị kinh doanh K18 Lớp 1', 'BA', 18, '2024-2028', 3),
('MC1701', N'Truyền thông đa phương tiện K17 Lớp 1', 'MC', 17, '2023-2027', 5),
('MC1801', N'Truyền thông đa phương tiện K18 Lớp 1', 'MC', 18, '2024-2028', 3),
('EN1701', N'Ngôn ngữ Anh K17 Lớp 1', 'EN', 17, '2023-2027', 5),
('JP1701', N'Ngôn ngữ Nhật K17 Lớp 1', 'JP', 17, '2023-2027', 5),
('KR1701', N'Ngôn ngữ Hàn K17 Lớp 1', 'KR', 17, '2023-2027', 5),
('DS1801', N'Khoa học dữ liệu K18 Lớp 1', 'DS', 18, '2024-2028', 3),
('IT1801', N'Công nghệ thông tin K18 Lớp 1', 'IT', 18, '2024-2028', 3),
('EN1801', N'Ngôn ngữ Anh K18 Lớp 1', 'EN', 18, '2024-2028', 3),
('JP1801', N'Ngôn ngữ Nhật K18 Lớp 1', 'JP', 18, '2024-2028', 3),
('KR1801', N'Ngôn ngữ Hàn K18 Lớp 1', 'KR', 18, '2024-2028', 3);

DECLARE @cCode NVARCHAR(50), @cName NVARCHAR(255), @cMaj NVARCHAR(50), @cCoh INT, @cYear NVARCHAR(50), @cSem INT;
DECLARE curCls CURSOR FOR SELECT Code, Name, MajorCode, Cohort, Year, Sem FROM #TmpClasses;
OPEN curCls;
FETCH NEXT FROM curCls INTO @cCode, @cName, @cMaj, @cCoh, @cYear, @cSem;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AdministrativeClasses WHERE Code = @cCode)
    BEGIN
        DECLARE @mId INT = (SELECT Id FROM Majors WHERE Code = @cMaj);
        INSERT INTO AdministrativeClasses (Code, Name, MajorId, Cohort, AcademicYear, CurrentSemester, IsActive)
        VALUES (@cCode, @cName, @mId, @cCoh, @cYear, @cSem, 1);
    END
    FETCH NEXT FROM curCls INTO @cCode, @cName, @cMaj, @cCoh, @cYear, @cSem;
END
CLOSE curCls;
DEALLOCATE curCls;
DROP TABLE #TmpClasses;

PRINT N'==> Đã cập nhật 21 lớp hành chính phủ khắp các ngành.';

-- 6. TÀI KHOẢN GIẢNG VIÊN (LECTURERS)
DECLARE @pwdHash NVARCHAR(255) = N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW';

CREATE TABLE #TmpLecturers (Username NVARCHAR(50), FullName NVARCHAR(100), Email NVARCHAR(100), Phone NVARCHAR(50), Code NVARCHAR(50), Dept NVARCHAR(100), Degree NVARCHAR(50));
INSERT INTO #TmpLecturers VALUES
('gv_cuongnk', N'TS. Nguyễn Kim Cương', 'cuongnk@fpt.edu.vn', '0912111222', 'GV003', N'Kỹ thuật phần mềm', 'PhD'),
('gv_sondh', N'ThS. Đỗ Hoàng Sơn', 'sondh@fpt.edu.vn', '0912222333', 'GV004', N'An toàn thông tin', 'Master'),
('gv_lanpt', N'TS. Phạm Thị Lan', 'lanpt@fpt.edu.vn', '0912333444', 'GV005', N'Trí tuệ nhân tạo & Dữ liệu', 'PhD'),
('gv_namth', N'ThS. Trần Hải Nam', 'namth@fpt.edu.vn', '0912444555', 'GV006', N'Hệ thống thông tin & Mạng', 'Master'),
('gv_trivm', N'ThS. Vũ Minh Trí', 'trivm@fpt.edu.vn', '0912555666', 'GV007', N'Thiết kế Mỹ thuật số', 'Master'),
('gv_huonghm', N'TS. Hoàng Mai Hương', 'huonghm@fpt.edu.vn', '0912666777', 'GV008', N'Quản trị kinh doanh', 'PhD'),
('gv_kietdt', N'ThS. Đặng Tuấn Kiệt', 'kietdt@fpt.edu.vn', '0912777888', 'GV009', N'Marketing & Kinh doanh số', 'Master'),
('gv_thaobp', N'ThS. Bùi Phương Thảo', 'thaobp@fpt.edu.vn', '0912888999', 'GV010', N'Truyền thông đa phương tiện', 'Master'),
('gv_john', N'ThS. John Smith', 'johnsmith@fpt.edu.vn', '0912999000', 'GV011', N'Ngôn ngữ Anh', 'Master'),
('gv_tanaka', N'ThS. Tanaka Hiroshi', 'tanaka@fpt.edu.vn', '0913000111', 'GV012', N'Ngôn ngữ Nhật', 'Master'),
('gv_park', N'ThS. Park Min Woo', 'parkmw@fpt.edu.vn', '0913111222', 'GV013', N'Ngôn ngữ Hàn', 'Master');

DECLARE @gvUser NVARCHAR(50), @gvName NVARCHAR(100), @gvEmail NVARCHAR(100), @gvPhone NVARCHAR(50), @gvCode NVARCHAR(50), @gvDept NVARCHAR(100), @gvDeg NVARCHAR(50);
DECLARE curGv CURSOR FOR SELECT Username, FullName, Email, Phone, Code, Dept, Degree FROM #TmpLecturers;
OPEN curGv;
FETCH NEXT FROM curGv INTO @gvUser, @gvName, @gvEmail, @gvPhone, @gvCode, @gvDept, @gvDeg;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @gvUser)
    BEGIN
        INSERT INTO Users (Username, PasswordHash, FullName, Email, PhoneNumber, Role, IsActive, CreatedAt)
        VALUES (@gvUser, @pwdHash, @gvName, @gvEmail, @gvPhone, 'Lecturer', 1, GETUTCDATE());
        
        DECLARE @newGvUserId INT = SCOPE_IDENTITY();
        INSERT INTO Lecturers (UserId, LecturerCode, Department, AcademicTitle)
        VALUES (@newGvUserId, @gvCode, @gvDept, @gvDeg);
    END
    FETCH NEXT FROM curGv INTO @gvUser, @gvName, @gvEmail, @gvPhone, @gvCode, @gvDept, @gvDeg;
END
CLOSE curGv;
DEALLOCATE curGv;
DROP TABLE #TmpLecturers;

-- Seed mẫu LecturerSubjects nếu cần
IF NOT EXISTS (SELECT 1 FROM LecturerSubjects)
BEGIN
    INSERT INTO LecturerSubjects (LecturerId, SubjectId, MajorId, SpecializationId, RegisteredAt)
    SELECT l.Id, s.Id, s.MajorId, s.SpecializationId, GETUTCDATE()
    FROM Lecturers l
    CROSS JOIN Subjects s
    WHERE (l.LecturerCode IN ('GV001', 'GV002', 'GV003') AND s.Code IN ('PRF192', 'PRO192', 'PRN211', 'PRN232', 'SWP391', 'CSD201', 'DBI202'))
       OR (l.LecturerCode = 'GV004' AND s.Code IN ('NWC203', 'SWE201', 'SWP391'))
       OR (l.LecturerCode = 'GV005' AND s.Code IN ('AI101', 'MAS291', 'DBI202'))
       OR (l.LecturerCode = 'GV011' AND s.Code IN ('ENG101', 'ENG102', 'ENG201'))
       OR (l.LecturerCode = 'GV012' AND s.Code IN ('JPD113', 'JPD123'))
       OR (l.LecturerCode = 'GV013' AND s.Code IN ('KOR101', 'KOR102'));
END;

PRINT N'==> Đã cập nhật 13 giảng viên các bộ môn.';

-- 7. THÊM THẬT NHIỀU SINH VIÊN (STUDENTS) VÀO TẤT CẢ CÁC NGÀNH
CREATE TABLE #TmpStudents (
    Username NVARCHAR(50),
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(50),
    StudentCode NVARCHAR(50),
    MajorCode NVARCHAR(50),
    SpecCode NVARCHAR(50),
    ClassCode NVARCHAR(50),
    Cohort INT,
    CurrentSem INT
);

INSERT INTO #TmpStudents VALUES
-- KHỐI CÔNG NGHỆ THÔNG TIN (MÃ HE)
-- SE - Kỹ thuật phần mềm
('he170010', N'Nguyễn Hoàng Long', 'longnhhe170010@fpt.edu.vn', '0981000010', 'HE170010', 'SE', 'SE-IS', 'SE1701', 17, 5),
('he170011', N'Vũ Đình Trọng', 'trongvdhe170011@fpt.edu.vn', '0981000011', 'HE170011', 'SE', 'SE-IS', 'SE1701', 17, 5),
('he170012', N'Đỗ Quốc Bảo', 'baodqhe170012@fpt.edu.vn', '0981000012', 'HE170012', 'SE', 'SE-JS', 'SE1701', 17, 5),
('he170013', N'Bùi Phương Linh', 'linhbphe170013@fpt.edu.vn', '0981000013', 'HE170013', 'SE', 'SE-JS', 'SE1701', 17, 5),
('he170014', N'Hoàng Tuấn Dũng', 'dunghthe170014@fpt.edu.vn', '0981000014', 'HE170014', 'SE', 'SE-ES', 'SE1702', 17, 5),
('he170015', N'Lê Thị Thu Trang', 'trangltthe170015@fpt.edu.vn', '0981000015', 'HE170015', 'SE', 'SE-ES', 'SE1702', 17, 5),
('he170016', N'Trần Minh Đức', 'ductmhe170016@fpt.edu.vn', '0981000016', 'HE170016', 'SE', 'SE-AS', 'SE1702', 17, 5),
('he170017', N'Ngô Thanh Tùng', 'tungnthe170017@fpt.edu.vn', '0981000017', 'HE170017', 'SE', 'SE-AS', 'SE1702', 17, 5),
('he170018', N'Phạm Gia Huy', 'huypghe170018@fpt.edu.vn', '0981000018', 'HE170018', 'SE', 'SE-IS', 'SE1703', 17, 5),
('he170019', N'Đinh Bảo Ngọc', 'ngocdbhe170019@fpt.edu.vn', '0981000019', 'HE170019', 'SE', 'SE-JS', 'SE1703', 17, 5),
('he180001', N'Nguyễn Quang Hải', 'hainqhe180001@fpt.edu.vn', '0981000101', 'HE180001', 'SE', NULL, 'SE1801', 18, 3),
('he180002', N'Đoàn Văn Hậu', 'haudvhe180002@fpt.edu.vn', '0981000102', 'HE180002', 'SE', NULL, 'SE1801', 18, 3),
('he180003', N'Nguyễn Tiến Linh', 'linhnthe180003@fpt.edu.vn', '0981000103', 'HE180003', 'SE', NULL, 'SE1802', 18, 3),
('he180004', N'Phan Văn Đức', 'ducpvhe180004@fpt.edu.vn', '0981000104', 'HE180004', 'SE', NULL, 'SE1802', 18, 3),

-- IA - An toàn thông tin
('he170101', N'Trịnh Xuân Bách', 'bachtxhe170101@fpt.edu.vn', '0982000001', 'HE170101', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170102', N'Dương Thùy Chi', 'chidtdhe170102@fpt.edu.vn', '0982000002', 'HE170102', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170103', N'Tạ Quang Thắng', 'thangtqhe170103@fpt.edu.vn', '0982000003', 'HE170103', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he170104', N'Cao Hoài Nam', 'namchhe170104@fpt.edu.vn', '0982000004', 'HE170104', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he180101', N'Lưu Đình Phong', 'phongldhe180101@fpt.edu.vn', '0982000101', 'HE180101', 'IA', NULL, 'IA1801', 18, 3),
('he180102', N'Mai Hồng Quân', 'quanmhhe180102@fpt.edu.vn', '0982000102', 'HE180102', 'IA', NULL, 'IA1801', 18, 3),

-- AI - Trí tuệ nhân tạo
('he170201', N'Phạm Minh Tuấn', 'tuanpmhe170201@fpt.edu.vn', '0983000001', 'HE170201', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170202', N'Nguyễn Khánh Vy', 'vynkhe170202@fpt.edu.vn', '0983000002', 'HE170202', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170203', N'Trần Quốc Anh', 'anhtqhe170203@fpt.edu.vn', '0983000003', 'HE170203', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he170204', N'Lê Minh Châu', 'chaulmhe170204@fpt.edu.vn', '0983000004', 'HE170204', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he180201', N'Võ Tấn Phát', 'phatvthe180201@fpt.edu.vn', '0983000101', 'HE180201', 'AI', NULL, 'AI1801', 18, 3),
('he180202', N'Hoàng Yến Nhi', 'nhiyhhe180202@fpt.edu.vn', '0983000102', 'HE180202', 'AI', NULL, 'AI1801', 18, 3),

-- DS - Khoa học dữ liệu
('he170301', N'Đỗ Hoàng Phúc', 'phucdhhe170301@fpt.edu.vn', '0984000001', 'HE170301', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170302', N'Nguyễn Thúy Hằng', 'hangnthe170302@fpt.edu.vn', '0984000002', 'HE170302', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170303', N'Trần Đại Nghĩa', 'nghiatdhe170303@fpt.edu.vn', '0984000003', 'HE170303', 'DS', 'DS-DE', 'DS1701', 17, 5),
('he170304', N'Phan Thanh Bình', 'binhpthe170304@fpt.edu.vn', '0984000004', 'HE170304', 'DS', 'DS-DE', 'DS1701', 17, 5),

-- IT - Công nghệ thông tin
('he170401', N'Lê Văn Thắng', 'thanglvhe170401@fpt.edu.vn', '0985000001', 'HE170401', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170402', N'Bùi Thị Ánh', 'anhbthe170402@fpt.edu.vn', '0985000002', 'HE170402', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170403', N'Nguyễn Văn Toàn', 'toannvhe170403@fpt.edu.vn', '0985000003', 'HE170403', 'IT', 'IT-SA', 'IT1701', 17, 5),
('he170404', N'Hoàng Trung Kiên', 'kienhthe170404@fpt.edu.vn', '0985000004', 'HE170404', 'IT', 'IT-SA', 'IT1701', 17, 5),

-- GD - Thiết kế đồ họa
('he170501', N'Nguyễn Thùy Dung', 'dungnthe170501@fpt.edu.vn', '0986000001', 'HE170501', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170502', N'Trương Mỹ Duyên', 'duyentmhe170502@fpt.edu.vn', '0986000002', 'HE170502', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170503', N'Phạm Đăng Khôi', 'khoipdhe170503@fpt.edu.vn', '0986000003', 'HE170503', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he170504', N'Lý Nhã Kỳ', 'kylnhe170504@fpt.edu.vn', '0986000004', 'HE170504', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he180501', N'Hà Anh Tuấn', 'tuanhahe180501@fpt.edu.vn', '0986000101', 'HE180501', 'GD', NULL, 'GD1801', 18, 3),
('he180502', N'Trịnh Thăng Bình', 'binhtthe180502@fpt.edu.vn', '0986000102', 'HE180502', 'GD', NULL, 'GD1801', 18, 3),

-- KHỐI MARKETING VÀ TRUYỀN THÔNG (MÃ HS)
-- BA - Quản trị kinh doanh & Marketing
('hs170001', N'Nguyễn Trọng Đại', 'dainths170001@fpt.edu.vn', '0987000001', 'HS170001', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170002', N'Lê Bảo Trâm', 'tramlths170002@fpt.edu.vn', '0987000002', 'HS170002', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170003', N'Trần Ngọc Hân', 'hantnhs170003@fpt.edu.vn', '0987000003', 'HS170003', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170004', N'Vũ Đức Thịnh', 'thinhvdhs170004@fpt.edu.vn', '0987000004', 'HS170004', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170005', N'Hoàng Gia Cát', 'cathghs170005@fpt.edu.vn', '0987000005', 'HS170005', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170006', N'Đặng Phương Mai', 'maidphs170006@fpt.edu.vn', '0987000006', 'HS170006', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170007', N'Bùi Anh Tuấn', 'tuanbahs170007@fpt.edu.vn', '0987000007', 'HS170007', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs170008', N'Ngô Kiều Oanh', 'oanhnkhs170008@fpt.edu.vn', '0987000008', 'HS170008', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs180001', N'Phan Thanh Hậu', 'haupthhs180001@fpt.edu.vn', '0987000101', 'HS180001', 'BA', NULL, 'BA1801', 18, 3),
('hs180002', N'Lương Xuân Trường', 'truonglxhs180002@fpt.edu.vn', '0987000102', 'HS180002', 'BA', NULL, 'BA1801', 18, 3),

-- MC - Truyền thông đa phương tiện
('hs170101', N'Trần Minh Tâm', 'tamtm_hs17@fpt.edu.vn', '0988000001', 'HS170101', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170102', N'Vũ Thùy Linh', 'linhvt_hs17@fpt.edu.vn', '0988000002', 'HS170102', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170103', N'Đỗ Tuấn Khang', 'khangdt_hs17@fpt.edu.vn', '0988000003', 'HS170103', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs170104', N'Lê Diễm My', 'myld_hs17@fpt.edu.vn', '0988000004', 'HS170104', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs180101', N'Nguyễn Quang Dũng', 'dungnq_hs18@fpt.edu.vn', '0988000101', 'HS180101', 'MC', NULL, 'MC1801', 18, 3),
('hs180102', N'Phạm Hương Ly', 'lyph_hs18@fpt.edu.vn', '0988000102', 'HS180102', 'MC', NULL, 'MC1801', 18, 3),

-- KHỐI NGÔN NGỮ (MÃ HA)
-- EN, JP, KR - Ngôn ngữ
('ha170001', N'Trần Kim Ngân', 'ngantk_ha17@fpt.edu.vn', '0989000001', 'HA170001', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170002', N'Nguyễn Hoàng Bách', 'bachnh_ha17@fpt.edu.vn', '0989000002', 'HA170002', 'EN', 'EN-TI', 'EN1701', 17, 5),
('ha170003', N'Lê Minh Tuấn', 'tuanlm_ha17@fpt.edu.vn', '0989000003', 'HA170003', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170101', N'Hoàng Mai Anh', 'anhhm_ha17@fpt.edu.vn', '0989000011', 'HA170101', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170102', N'Đỗ Nhật Quang', 'quangdn_ha17@fpt.edu.vn', '0989000012', 'HA170102', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170103', N'Phạm Thu Thảo', 'thaopt_ha17@fpt.edu.vn', '0989000013', 'HA170103', 'JP', 'JP-BC', 'JP1701', 17, 5),
('ha170201', N'Vũ Ngọc Lan', 'lanvn_ha17@fpt.edu.vn', '0989000021', 'HA170201', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha170202', N'Trần Đăng Khoa', 'khoatd_ha17@fpt.edu.vn', '0989000022', 'HA170202', 'KR', 'KR-TI', 'KR1701', 17, 5),
('ha170203', N'Nguyễn Khánh Linh', 'linhnk_ha17@fpt.edu.vn', '0989000023', 'HA170203', 'KR', 'KR-BC', 'KR1701', 17, 5),

-- KHỐI CNTT BỔ SUNG (MÃ HE)
('he180010', N'Nguyễn Hải Đăng', 'dangnhhe180010@fpt.edu.vn', '0981100010', 'HE180010', 'SE', NULL, 'SE1801', 18, 3),
('he180011', N'Trần Quốc Huy', 'huytqhe180011@fpt.edu.vn', '0981100011', 'HE180011', 'SE', NULL, 'SE1801', 18, 3),
('he180012', N'Lê Khánh Toàn', 'toanlkhe180012@fpt.edu.vn', '0981100012', 'HE180012', 'SE', NULL, 'SE1802', 18, 3),
('he180013', N'Phạm Thanh Thảo', 'thaoptthhe180013@fpt.edu.vn', '0981100013', 'HE180013', 'SE', NULL, 'SE1802', 18, 3),
('he190001', N'Đặng Quang Khải', 'khaidqhe190001@fpt.edu.vn', '0981100014', 'HE190001', 'SE', NULL, 'SE1801', 18, 3),
('he170020', N'Võ Hoàng Yến', 'yenvhhe170020@fpt.edu.vn', '0981100020', 'HE170020', 'SE', 'SE-IS', 'SE1703', 17, 5),
('he170021', N'Nguyễn Thành Đạt', 'datnthe170021@fpt.edu.vn', '0981100021', 'HE170021', 'SE', 'SE-JS', 'SE1703', 17, 5),
('he170105', N'Bùi Minh Trí', 'tribmhe170105@fpt.edu.vn', '0982100005', 'HE170105', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170106', N'Đỗ Thu Hà', 'hadthe170106@fpt.edu.vn', '0982100006', 'HE170106', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he180103', N'Trương Gia Bình', 'binhtghe180103@fpt.edu.vn', '0982100103', 'HE180103', 'IA', NULL, 'IA1801', 18, 3),
('he180104', N'Hoàng Trọng Nghĩa', 'nghiahthe180104@fpt.edu.vn', '0982100104', 'HE180104', 'IA', NULL, 'IA1801', 18, 3),
('he180105', N'Nguyễn Tuấn Anh', 'anhnthe180105@fpt.edu.vn', '0982100105', 'HE180105', 'IA', NULL, 'IA1801', 18, 3),
('he170205', N'Vũ Anh Khoa', 'khoavakhe170205@fpt.edu.vn', '0983100005', 'HE170205', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170206', N'Lê Thảo Nguyên', 'nguyenlthe170206@fpt.edu.vn', '0983100006', 'HE170206', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he180203', N'Đinh Quang Dũng', 'dungdqhe180203@fpt.edu.vn', '0983100103', 'HE180203', 'AI', NULL, 'AI1801', 18, 3),
('he180204', N'Trần Ngọc Lan', 'lantnhe180204@fpt.edu.vn', '0983100104', 'HE180204', 'AI', NULL, 'AI1801', 18, 3),
('he180205', N'Phạm Quốc Cường', 'cuongpqhe180205@fpt.edu.vn', '0983100105', 'HE180205', 'AI', NULL, 'AI1801', 18, 3),
('he170305', N'Nguyễn Văn Hiếu', 'hieunvhe170305@fpt.edu.vn', '0984100005', 'HE170305', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170306', N'Bùi Thùy Tiên', 'tienbthe170306@fpt.edu.vn', '0984100006', 'HE170306', 'DS', 'DS-DE', 'DS1701', 17, 5),
('he180301', N'Phan Nhật Minh', 'minhpnhe180301@fpt.edu.vn', '0984100101', 'HE180301', 'DS', NULL, 'DS1801', 18, 3),
('he180302', N'Đoàn Tuấn Phong', 'phongdthe180302@fpt.edu.vn', '0984100102', 'HE180302', 'DS', NULL, 'DS1801', 18, 3),
('he170405', N'Trần Đức Duy', 'duytdhe170405@fpt.edu.vn', '0985100005', 'HE170405', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170406', N'Vũ Thị Kim Oanh', 'oanhvtkhe170406@fpt.edu.vn', '0985100006', 'HE170406', 'IT', 'IT-SA', 'IT1701', 17, 5),
('he180401', N'Nguyễn Hữu Phước', 'phuocnhhe180401@fpt.edu.vn', '0985100101', 'HE180401', 'IT', NULL, 'IT1801', 18, 3),
('he180402', N'Đặng Hữu Thắng', 'thangdthe180402@fpt.edu.vn', '0985100102', 'HE180402', 'IT', NULL, 'IT1801', 18, 3),
('he170505', N'Hoàng Kim Ngân', 'nganhkhe170505@fpt.edu.vn', '0986100005', 'HE170505', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170506', N'Ngô Gia Huy', 'huynghe170506@fpt.edu.vn', '0986100006', 'HE170506', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he180503', N'Phạm Mai Phương', 'phuongmphe180503@fpt.edu.vn', '0986100103', 'HE180503', 'GD', NULL, 'GD1801', 18, 3),
('he180504', N'Trịnh Quang Huy', 'huytqhe180504@fpt.edu.vn', '0986100104', 'HE180504', 'GD', NULL, 'GD1801', 18, 3),
('he180505', N'Đỗ Hải Yến', 'yendhhe180505@fpt.edu.vn', '0986100105', 'HE180505', 'GD', NULL, 'GD1801', 18, 3),

-- KHỐI MARKETING & TRUYỀN THÔNG BỔ SUNG (MÃ HS)
('hs170009', N'Nguyễn Bảo Châu', 'chounbhs170009@fpt.edu.vn', '0987100009', 'HS170009', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170010', N'Lê Hoàng Kim', 'kimlhhs170010@fpt.edu.vn', '0987100010', 'HS170010', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170011', N'Trần Thục Anh', 'anhtths170011@fpt.edu.vn', '0987100011', 'HS170011', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170012', N'Đặng Tuấn Khanh', 'khanhdths170012@fpt.edu.vn', '0987100012', 'HS170012', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs180003', N'Vũ Quỳnh Như', 'nhuvqhs180003@fpt.edu.vn', '0987100103', 'HS180003', 'BA', NULL, 'BA1801', 18, 3),
('hs180004', N'Phan Quốc Đạt', 'datpqhs180004@fpt.edu.vn', '0987100104', 'HS180004', 'BA', NULL, 'BA1801', 18, 3),
('hs180005', N'Bùi Thế Anh', 'anhbths180005@fpt.edu.vn', '0987100105', 'HS180005', 'BA', NULL, 'BA1801', 18, 3),
('hs180006', N'Đỗ Thanh Hằng', 'hangdths180006@fpt.edu.vn', '0987100106', 'HS180006', 'BA', NULL, 'BA1801', 18, 3),
('hs180007', N'Nguyễn Tuấn Kiệt', 'kietnths180007@fpt.edu.vn', '0987100107', 'HS180007', 'BA', NULL, 'BA1801', 18, 3),
('hs180008', N'Hoàng Mỹ Tâm', 'tamhmhs180008@fpt.edu.vn', '0987100108', 'HS180008', 'BA', NULL, 'BA1801', 18, 3),
('hs180009', N'Trịnh Bảo Nam', 'namtbhs180009@fpt.edu.vn', '0987100109', 'HS180009', 'BA', NULL, 'BA1801', 18, 3),
('hs170105', N'Phạm Thùy Tiên', 'tienpt_hs17@fpt.edu.vn', '0988100005', 'HS170105', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170106', N'Trần Thanh Vân', 'vantt_hs17@fpt.edu.vn', '0988100006', 'HS170106', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs170107', N'Lê Duy Khoa', 'khoald_hs17@fpt.edu.vn', '0988100007', 'HS170107', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs180103', N'Nguyễn Khánh Huyền', 'huyennk_hs18@fpt.edu.vn', '0988100103', 'HS180103', 'MC', NULL, 'MC1801', 18, 3),
('hs180104', N'Võ Thành Nam', 'namvt_hs18@fpt.edu.vn', '0988100104', 'HS180104', 'MC', NULL, 'MC1801', 18, 3),
('hs180105', N'Đặng Diệu Linh', 'linhdd_hs18@fpt.edu.vn', '0988100105', 'HS180105', 'MC', NULL, 'MC1801', 18, 3),
('hs180106', N'Trương Trí Thông', 'thongtt_hs18@fpt.edu.vn', '0988100106', 'HS180106', 'MC', NULL, 'MC1801', 18, 3),
('hs180107', N'Hoàng Thảo My', 'myht_hs18@fpt.edu.vn', '0988100107', 'HS180107', 'MC', NULL, 'MC1801', 18, 3),
('hs180108', N'Vũ Thái Sơn', 'sonvt_hs18@fpt.edu.vn', '0988100108', 'HS180108', 'MC', NULL, 'MC1801', 18, 3),
('hs180109', N'Nguyễn Hà Phương', 'phuongnh_hs18@fpt.edu.vn', '0988100109', 'HS180109', 'MC', NULL, 'MC1801', 18, 3),
('hs180110', N'Đoàn Minh Trí', 'tridm_hs18@fpt.edu.vn', '0988100110', 'HS180110', 'MC', NULL, 'MC1801', 18, 3),

-- KHỐI NGÔN NGỮ BỔ SUNG (MÃ HA)
('ha170004', N'Nguyễn Thanh Hằng', 'hangnt_ha17@fpt.edu.vn', '0989100004', 'HA170004', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170005', N'Đỗ Quang Minh', 'minhdq_ha17@fpt.edu.vn', '0989100005', 'HA170005', 'EN', 'EN-TI', 'EN1701', 17, 5),
('ha170006', N'Lê Thảo Vy', 'vylt_ha17@fpt.edu.vn', '0989100006', 'HA170006', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha180001', N'Trần Quốc Bảo', 'baotq_ha18@fpt.edu.vn', '0989100101', 'HA180001', 'EN', NULL, 'EN1801', 18, 3),
('ha180002', N'Bùi Hoàng Oanh', 'oanhbh_ha18@fpt.edu.vn', '0989100102', 'HA180002', 'EN', NULL, 'EN1801', 18, 3),
('ha180003', N'Vũ Đình Khang', 'khangvd_ha18@fpt.edu.vn', '0989100103', 'HA180003', 'EN', NULL, 'EN1801', 18, 3),
('ha180004', N'Phan Thục Quyên', 'quyenpt_ha18@fpt.edu.vn', '0989100104', 'HA180004', 'EN', NULL, 'EN1801', 18, 3),
('ha170104', N'Phạm Gia Bảo', 'baopg_ha17@fpt.edu.vn', '0989100014', 'HA170104', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170105', N'Nguyễn Phương Uyên', 'uyennp_ha17@fpt.edu.vn', '0989100015', 'HA170105', 'JP', 'JP-BC', 'JP1701', 17, 5),
('ha170106', N'Đặng Minh Khôi', 'khoidm_ha17@fpt.edu.vn', '0989100016', 'HA170106', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha180101', N'Trịnh Thùy Trang', 'trangtt_ha18@fpt.edu.vn', '0989100111', 'HA180101', 'JP', NULL, 'JP1801', 18, 3),
('ha180102', N'Lê Văn Quyết', 'quyetlv_ha18@fpt.edu.vn', '0989100112', 'HA180102', 'JP', NULL, 'JP1801', 18, 3),
('ha180103', N'Hoàng Ánh Dương', 'duongha_ha18@fpt.edu.vn', '0989100113', 'HA180103', 'JP', NULL, 'JP1801', 18, 3),
('ha180104', N'Võ Mai Chi', 'chivm_ha18@fpt.edu.vn', '0989100114', 'HA180104', 'JP', NULL, 'JP1801', 18, 3),
('ha170204', N'Trần Diệu Hương', 'huongtd_ha17@fpt.edu.vn', '0989100024', 'HA170204', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha170205', N'Vũ Anh Tuấn', 'tuanva_ha17@fpt.edu.vn', '0989100025', 'HA170205', 'KR', 'KR-TI', 'KR1701', 17, 5),
('ha170206', N'Lê Bảo Hân', 'hanlb_ha17@fpt.edu.vn', '0989100026', 'HA170206', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha180201', N'Nguyễn Minh Hạnh', 'hanhnm_ha18@fpt.edu.vn', '0989100121', 'HA180201', 'KR', NULL, 'KR1801', 18, 3),
('ha180202', N'Đỗ Tuấn Kiệt', 'kietdt_ha18@fpt.edu.vn', '0989100122', 'HA180202', 'KR', NULL, 'KR1801', 18, 3),
('ha180203', N'Phan Ngọc Diệp', 'dieppn_ha18@fpt.edu.vn', '0989100123', 'HA180203', 'KR', NULL, 'KR1801', 18, 3);

DECLARE @stUser NVARCHAR(50), @stName NVARCHAR(100), @stEmail NVARCHAR(100), @stPhone NVARCHAR(50), @stCode NVARCHAR(50);
DECLARE @stMaj NVARCHAR(50), @stSpec NVARCHAR(50), @stCls NVARCHAR(50), @stCoh INT, @stSem INT;

DECLARE curSt CURSOR FOR 
SELECT Username, FullName, Email, Phone, StudentCode, MajorCode, SpecCode, ClassCode, Cohort, CurrentSem 
FROM #TmpStudents;

OPEN curSt;
FETCH NEXT FROM curSt INTO @stUser, @stName, @stEmail, @stPhone, @stCode, @stMaj, @stSpec, @stCls, @stCoh, @stSem;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @stUser)
    BEGIN
        INSERT INTO Users (Username, PasswordHash, FullName, Email, PhoneNumber, Role, IsActive, CreatedAt)
        VALUES (@stUser, @pwdHash, @stName, @stEmail, @stPhone, 'Student', 1, GETUTCDATE());
        
        DECLARE @newStUserId INT = SCOPE_IDENTITY();
        DECLARE @targetMajId INT = (SELECT Id FROM Majors WHERE Code = @stMaj);
        DECLARE @targetSpecId INT = (SELECT Id FROM Specializations WHERE Code = @stSpec);
        DECLARE @targetClassId INT = (SELECT Id FROM AdministrativeClasses WHERE Code = @stCls);

        INSERT INTO Students (UserId, StudentCode, MajorId, SpecializationId, AdministrativeClassId, Cohort, CurrentSemester, Status)
        VALUES (@newStUserId, @stCode, @targetMajId, @targetSpecId, @targetClassId, @stCoh, @stSem, 'Studying');
    END
    FETCH NEXT FROM curSt INTO @stUser, @stName, @stEmail, @stPhone, @stCode, @stMaj, @stSpec, @stCls, @stCoh, @stSem;
END

CLOSE curSt;
DEALLOCATE curSt;
DROP TABLE #TmpStudents;

PRINT N'==> Đã cập nhật 60+ sinh viên đầy đủ hồ sơ và tài khoản đăng nhập (Admin@123).';

-- 8. MỞ THÊM CÁC LỚP HỌC PHẦN (COURSE SECTIONS) & NHẬP ĐIỂM
DECLARE @semFA24 INT = (SELECT Id FROM Semesters WHERE Code = 'FA24');
DECLARE @idPRN232_sub INT = (SELECT Id FROM Subjects WHERE Code = 'PRN232');
DECLARE @idSWP391_sub INT = (SELECT Id FROM Subjects WHERE Code = 'SWP391');
DECLARE @idDBI202_sub INT = (SELECT Id FROM Subjects WHERE Code = 'DBI202');
DECLARE @idAIL_sub INT = (SELECT Id FROM Subjects WHERE Code = 'AIL302m');
DECLARE @idMKT_sub INT = (SELECT Id FROM Subjects WHERE Code = 'MKT101');
DECLARE @idUIX_sub INT = (SELECT Id FROM Subjects WHERE Code = 'UIX201');
DECLARE @idJPD_sub INT = (SELECT Id FROM Subjects WHERE Code = 'JPD113');

DECLARE @gvHungId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV001');
DECLARE @gvHuongId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV002');
DECLARE @gvLanId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV005');
DECLARE @gvKietId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV009');
DECLARE @gvTriId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV007');
DECLARE @gvTanakaId INT = (SELECT Id FROM Lecturers WHERE LecturerCode = 'GV012');

-- Lớp PRN232.02
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'PRN232_FA24_02')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('PRN232_FA24_02', N'Lập trình Web API với ASP.NET Core - Lớp 02', @idPRN232_sub, @semFA24, @gvHungId, 30, 0, 'Active', GETUTCDATE());
    DECLARE @csPRN232_02 INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csPRN232_02, 3, 2, 'BE-202');
END

-- Lớp SWP391.01
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'SWP391_FA24_01')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('SWP391_FA24_01', N'Dự án phát triển phần mềm - Lớp 01', @idSWP391_sub, @semFA24, @gvHuongId, 30, 0, 'Active', GETUTCDATE());
    DECLARE @csSWP INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csSWP, 2, 4, 'LAB-101');
END

-- Lớp AIL302m.01
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'AIL302m_FA24_01')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('AIL302m_FA24_01', N'Trí tuệ nhân tạo & Machine Learning - Lớp 01', @idAIL_sub, @semFA24, @gvLanId, 30, 0, 'Active', GETUTCDATE());
    DECLARE @csAIL INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csAIL, 4, 1, 'ALPHA-305');
END

-- Lớp MKT101.01
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'MKT101_FA24_01')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('MKT101_FA24_01', N'Nguyên lý Marketing - Lớp 01', @idMKT_sub, @semFA24, @gvKietId, 35, 0, 'Active', GETUTCDATE());
    DECLARE @csMKT INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csMKT, 5, 3, 'BETA-102');
END

-- Lớp UIX201.01
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'UIX201_FA24_01')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('UIX201_FA24_01', N'Thiết kế giao diện UI/UX - Lớp 01', @idUIX_sub, @semFA24, @gvTriId, 25, 0, 'Active', GETUTCDATE());
    DECLARE @csUIX INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csUIX, 1, 2, 'MAC-LAB-01');
END

-- Lớp JPD113.01
IF NOT EXISTS (SELECT 1 FROM CourseSections WHERE Code = 'JPD113_FA24_01')
BEGIN
    INSERT INTO CourseSections (Code, Name, SubjectId, SemesterId, LecturerId, MaxCapacity, IsGradeLocked, Status, CreatedAt)
    VALUES ('JPD113_FA24_01', N'Tiếng Nhật sơ cấp 1 - Lớp 01', @idJPD_sub, @semFA24, @gvTanakaId, 30, 0, 'Active', GETUTCDATE());
    DECLARE @csJPD INT = SCOPE_IDENTITY();
    INSERT INTO CourseSectionSchedules (CourseSectionId, DayOfWeek, Slot, Room) VALUES (@csJPD, 3, 4, 'DELTA-201');
END

PRINT N'==> Đã mở các lớp học phần kỳ FA24.';

-- Phân bổ tự động sinh viên vào các lớp HP mới mở và sinh điểm mẫu
DECLARE @csPRN_Id INT = (SELECT Id FROM CourseSections WHERE Code = 'PRN232_FA24_02');
IF @csPRN_Id IS NOT NULL
BEGIN
    -- Lấy các đầu điểm của PRN232
    DECLARE @c1 INT = (SELECT TOP 1 Id FROM SubjectGradeComponents WHERE SubjectId = @idPRN232_sub ORDER BY OrderIndex ASC);
    DECLARE @c2 INT = (SELECT TOP 1 Id FROM SubjectGradeComponents WHERE SubjectId = @idPRN232_sub AND Id <> @c1 ORDER BY OrderIndex ASC);
    
    INSERT INTO StudentGrades (StudentId, CourseSectionId, SubjectGradeComponentId, Score, UpdatedBy, UpdatedAt)
    SELECT s.Id, @csPRN_Id, @c1, 8.5, 'gv_hungnv', GETUTCDATE()
    FROM Students s 
    WHERE s.MajorId = @idSE AND s.Id NOT IN (SELECT StudentId FROM StudentGrades WHERE CourseSectionId = @csPRN_Id);

    INSERT INTO StudentGrades (StudentId, CourseSectionId, SubjectGradeComponentId, Score, UpdatedBy, UpdatedAt)
    SELECT s.Id, @csPRN_Id, @c2, 8.0, 'gv_hungnv', GETUTCDATE()
    FROM Students s 
    WHERE s.MajorId = @idSE AND s.Id IN (SELECT StudentId FROM StudentGrades WHERE CourseSectionId = @csPRN_Id AND SubjectGradeComponentId = @c1);
END

PRINT N'==> NẠP DỮ LIỆU ĐẠI HỌC FPT THÀNH CÔNG VÀ HOÀN TẤT!';
GO



-- ========================================================================================
-- KIỂM TRA TỔNG KẾT BẢNG & SỐ DÒNG DỮ LIỆU ĐÃ NẠP
-- ========================================================================================
PRINT N'';
PRINT N'=============================================================================';
PRINT N'  KHỞI TẠO CƠ SỞ DỮ LIỆU SMMS_PRN232 THÀNH CÔNG!';
PRINT N'=============================================================================';
SELECT 
    t.name AS [Bảng Dữ Liệu],
    i.rows AS [Số Bản Ghi Mẫu]
FROM sys.tables t
INNER JOIN sys.sysindexes i ON t.object_id = i.id AND i.indid < 2
ORDER BY t.name;
GO

PRINT N'';
PRINT N'=============================================================================';
PRINT N'  DANH SÁCH TÀI KHOẢN ĐĂNG NHẬP MẪU TRONG HỆ THỐNG (Pass: Admin@123)';
PRINT N'=============================================================================';
SELECT [Id], [Username], [FullName], [Role], [Email], [PhoneNumber] FROM [Users] ORDER BY [Id];
GO
