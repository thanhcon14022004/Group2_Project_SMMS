/*
========================================================================================
  Há»† THá»NG QUáº¢N LĂ ÄIá»‚M SINH VIĂN (SMMS - Student Mark Management System)
  MÔN HỌC: PRN232 (ASP.NET Core & C#)
  DATABASE SCRIPT: Táº O CÆ  Sá» Dá»® LIá»†U & Náº P Dá»® LIá»†U MáºªU CHUáº¨N (DDL + DML)
========================================================================================
  * Hỗ trợ: Microsoft SQL Server 2017 / 2019 / 2022 / LocalDB / Azure SQL Database
  * Tên CSDL: SMMS_PRN232
  * Cách chạy:
    - Cách 1: Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio -> Nhấn F5 (Execute).
    - Cách 2: Chạy lệnh terminal:
        sqlcmd -S "(localdb)\mssqllocaldb" -E -i SMMS_Database.sql

  * DANH SĂCH TĂ€I KHOáº¢N MáºªU KHá»I Táº O (Máº­t kháº©u chung cho táº¥t cáº£: Admin@123):
    1. Quản trị viên (Admin):
       - Username: admin         | Password: Admin@123 (Quản trị toàn hệ thống)
    2. Giảng viên (Lecturer):
       - Username: gv_hungnv     | Password: Admin@123 (ThS. Nguyễn Văn Hùng - Giảng viên PRN232)
       - Username: gv_huonglt    | Password: Admin@123 (TS. Lê Thị Hương - Giảng viên SWP391)
    3. Sinh viên (Student):
       - Username: he170001      | Password: Admin@123 (Trần Quang Minh - Lớp SE1701, Kỳ 5 SE-IS)
       - Username: he170002      | Password: Admin@123 (Lê Hoàng Nam    - Lớp SE1701, Kỳ 5 SE-JS)
       - Username: he170003      | Password: Admin@123 (Phạm Mai Anh   - Lớp SE1702, Kỳ 4 Äang chá»n CN)

  * LÆ¯U Ă LĂ€M Má»I (RESET DATABASE):
    Náº¿u báº¡n muá»‘n xĂ³a sáº¡ch cÆ¡ sá»Ÿ dữ liệu cÅ© Ä‘á»ƒ táº¡o láº¡i tá»« Ä‘áº§u, hĂ£y bá» comment 2 dĂ²ng sau:
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

PRINT N'==> Äang sá»­ dá»¥ng cÆ¡ sá»Ÿ dữ liệu SMMS_PRN232...';
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
-- Náº¡p dữ liệu máº«u cho báº£ng: Majors
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Majors] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Majors] ON;
    INSERT INTO [Majors] ([Id], [Code], [Name], [Description], [IsActive]) VALUES
(1, N'SE', N'Ká»¹ thuáº­t pháº§n má»m (Software Engineering)', N'ÄĂ o táº¡o kỹ sư pháº§n má»m cháº¥t lÆ°á»£ng cao', 1),
(2, N'IA', N'An toàn thông tin (Information Assurance)', N'ÄĂ o táº¡o chuyên gia báº£o máº­t và phĂ²ng thá»§ khĂ´ng gian máº¡ng', 1);
    SET IDENTITY_INSERT [Majors] OFF;
    PRINT N'==> ÄĂ£ náº¡p 2 báº£n ghi máº«u vào báº£ng Majors';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Majors Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: Specializations
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [Specializations] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [Specializations] ON;
    INSERT INTO [Specializations] ([Id], [MajorId], [Code], [Name], [Description], [IsActive]) VALUES
(1, 1, N'SE-IS', N'Hệ thống thông tin doanh nghiệp (Information Systems)', N'ChuyĂªn sĂ¢u vá» kiáº¿n trĂºc hệ thống và ERP', 1),
(2, 1, N'SE-JS', N'Ká»¹ thuáº­t pháº§n má»m Nháº­t Báº£n (Japanese Software)', N'ChuyĂªn sĂ¢u vá» cáº§u ná»‘i phát triển pháº§n má»m cho thá»‹ trÆ°á»ng Nháº­t', 1);
    SET IDENTITY_INSERT [Specializations] OFF;
    PRINT N'==> ÄĂ£ náº¡p 2 báº£n ghi máº«u vào báº£ng Specializations';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Specializations Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
(5, N'SWP391', N'Dự án phát triển pháº§n má»m thực chiến', 3, NULL, 1);
    SET IDENTITY_INSERT [Subjects] OFF;
    PRINT N'==> ÄĂ£ náº¡p 5 báº£n ghi máº«u vào báº£ng Subjects';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Subjects Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 20 báº£n ghi máº«u vào báº£ng SubjectGradeComponents';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng SubjectGradeComponents Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 4 báº£n ghi máº«u vào báº£ng PrerequisiteSubjects';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng PrerequisiteSubjects Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 6 báº£n ghi máº«u vào báº£ng Users';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Users Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 2 báº£n ghi máº«u vào báº£ng Lecturers';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Lecturers Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: AdministrativeClasses
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [AdministrativeClasses] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [AdministrativeClasses] ON;
    INSERT INTO [AdministrativeClasses] ([Id], [Code], [Name], [MajorId], [Cohort], [AcademicYear], [SpecializationId], [CurrentSemester], [IsActive]) VALUES
(1, N'SE1701', N'Ká»¹ thuáº­t pháº§n má»m K17 Lớp 1', 1, 17, N'2023-2027', NULL, 5, 1),
(2, N'SE1702', N'Ká»¹ thuáº­t pháº§n má»m K17 Lớp 2', 1, 17, N'2023-2027', NULL, 5, 1);
    SET IDENTITY_INSERT [AdministrativeClasses] OFF;
    PRINT N'==> ÄĂ£ náº¡p 2 báº£n ghi máº«u vào báº£ng AdministrativeClasses';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng AdministrativeClasses Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 3 báº£n ghi máº«u vào báº£ng Students';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Students Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
END;
GO

-- ======================================================
-- Báº£ng: CurriculumConfigs & CurriculumSubjects (Äá»ƒ trá»‘ng cáº¥u hĂ¬nh Ä‘á»ƒ Quản trị viĂªn tá»± thiáº¿t láº­p)
-- ======================================================
PRINT N'==> Báº£ng CurriculumConfigs & CurriculumSubjects sáºµn sĂ ng (Ä‘á»ƒ trá»‘ng cáº¥u hĂ¬nh)';
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
    PRINT N'==> ÄĂ£ náº¡p 3 báº£n ghi máº«u vào báº£ng Semesters';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng Semesters Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
(2, N'SWP391_SU25_SE1701', N'Dự án phát triển pháº§n má»m - Lớp SE1701', 5, 3, 2, 30, 0, NULL, NULL, N'Open', '2026-11-01 19:24:36.728');
    SET IDENTITY_INSERT [CourseSections] OFF;
    PRINT N'==> ÄĂ£ náº¡p 2 báº£n ghi máº«u vào báº£ng CourseSections';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng CourseSections Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 4 báº£n ghi máº«u vào báº£ng CourseSectionSchedules';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng CourseSectionSchedules Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 4 báº£n ghi máº«u vào báº£ng CourseSectionStudents';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng CourseSectionStudents Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
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
    PRINT N'==> ÄĂ£ náº¡p 5 báº£n ghi máº«u vào báº£ng StudentGrades';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng StudentGrades Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
END;
GO

-- ======================================================
-- Nạp dữ liệu mẫu cho bảng: AuditLogs
-- ======================================================
IF NOT EXISTS (SELECT 1 FROM [AuditLogs] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [AuditLogs] ON;
    INSERT INTO [AuditLogs] ([Id], [Action], [EntityName], [EntityId], [PerformedBy], [PerformedAt], [Details], [IpAddress]) VALUES
(1, N'SYSTEM_INITIALIZED', N'Database', N'ALL', N'System', '2026-11-01 19:24:36.852', N'Khá»Ÿi táº¡o hệ thống SMMS và náº¡p dữ liệu chuáº©n cho ngành Ká»¹ thuáº­t pháº§n má»m (K17, K18)', NULL);
    SET IDENTITY_INSERT [AuditLogs] OFF;
    PRINT N'==> ÄĂ£ náº¡p 1 báº£n ghi máº«u vào báº£ng AuditLogs';
END
ELSE
BEGIN
    PRINT N'==> Báº£ng AuditLogs Ä‘Ă£ cĂ³ dữ liệu, bá» qua bÆ°á»›c náº¡p máº«u';
END;
GO






USE [SMMS_PRN232];
GO

SET NOCOUNT ON;

PRINT N'==> Báº®T Äáº¦U Náº P Dá»® LIá»†U Äáº¦Y Äá»¦ CHÆ¯Æ NG TRĂŒNH ÄĂ€O Táº O Äáº I Há»ŒC FPT...';

-- 1. NGĂ€NH ÄĂ€O Táº O (MAJORS)
IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'AI')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('AI', N'Trí tuệ nhân tạo (Artificial Intelligence)', N'ÄĂ o táº¡o chuyên gia AI, Machine Learning và Khoa há»c dữ liệu', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'DS')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('DS', N'Khoa há»c dữ liệu (Data Science)', N'ÄĂ o táº¡o kỹ sư phân tích dữ liệu lớn và trực quan hóa kinh doanh', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'IT')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('IT', N'Công nghệ thông tin (Information Technology)', N'ÄĂ o táº¡o kỹ sư CNTT tổng hợp, Cloud và Quản trị hệ thống', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'GD')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('GD', N'Thiết kế Mỹ thuật số (Digital Art & Graphic Design)', N'ÄĂ o táº¡o thiết kế Ä‘á»“ há»a 2D/3D, UI/UX và truyá»n thĂ´ng hình ảnh', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'BA')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('BA', N'Quản trị kinh doanh (Business Administration)', N'ÄĂ o táº¡o cử nhân QTKD, Digital Marketing và Kinh doanh quốc tế', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'MC')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('MC', N'Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n (Multimedia Communication)', N'ÄĂ o táº¡o chuyên viên truyá»n thĂ´ng sá»‘, báo chí và tổ chức sự kiện', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'EN')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('EN', N'Ngôn ngữ Anh (English Language)', N'ÄĂ o táº¡o chuyên gia ngôn ngữ Anh thương mại và biên phiên dịch', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'JP')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('JP', N'Ngôn ngữ Nhật (Japanese Language)', N'ÄĂ o táº¡o cử nhân tiếng Nhật CNTT và tiếng Nhật thương mại', 1);

IF NOT EXISTS (SELECT 1 FROM Majors WHERE Code = 'KR')
    INSERT INTO Majors (Code, Name, Description, IsActive) VALUES ('KR', N'Ngôn ngữ Hàn (Korean Language)', N'ÄĂ o táº¡o cử nhân tiếng Hàn thương mại và truyá»n thĂ´ng', 1);

PRINT N'==> ÄĂ£ cáº­p nháº­t 11 ngành Ä‘Ă o táº¡o FPT.';

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

-- 2. CHUYĂN NGĂ€NH Háº¸P (SPECIALIZATIONS)
-- SE
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'SE-ES')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idSE, 'SE-ES', N'Hệ thống nhúng & IoT (Embedded Systems & IoT)', N'Lập trình vi Ä‘iá»u khiá»ƒn, hệ thống nhúng và thiết bị thông minh', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'SE-AS')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idSE, 'SE-AS', N'Ká»¹ thuáº­t pháº§n má»m Ă´ tĂ´ (Automotive Software)', N'Phát triển pháº§n má»m Ä‘iá»u khiá»ƒn và xe tự hành', 1);

-- IA
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IA-NW')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIA, 'IA-NW', N'An ninh máº¡ng & Äiá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y (Cyber & Cloud Security)', N'Bảo mật hạ tầng mạng, phòng chống tấn công DDoS', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'IA-DF')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIA, 'IA-DF', N'Äiá»u tra sá»‘ & á»¨ng phĂ³ sá»± cá»‘ an ninh (Digital Forensics)', N'Khôi phục chứng cứ số và giám định bảo mật', 1);

-- AI
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'AI-ML')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idAI, 'AI-ML', N'Há»c mĂ¡y & Há»c sĂ¢u ứng dụng (Machine Learning & Deep Learning)', N'Xây dựng các mô hình AI dự báo và huấn luyện mạng nơ-ron', 1);
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
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idIT, 'IT-SA', N'Quản trị hệ thống & Äiá»‡n toĂ¡n Ä‘Ă¡m mĂ¢y (Cloud & Systems)', N'Triển khai AWS/Azure, CI/CD và DevOps', 1);

-- GD
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'GD-3D')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idGD, 'GD-3D', N'Thiết kế Ä‘á»“ há»a 3D & Kỹ xảo hình ảnh (3D Animation & VFX)', N'Dựng hình nhân vật 3D, làm hoạt hình và kỹ xảo điện ảnh', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'GD-UX')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idGD, 'GD-UX', N'Thiết kế giao diá»‡n & Tráº£i nghiá»‡m ngÆ°á»i dĂ¹ng (UI/UX Design)', N'NghiĂªn cá»©u hĂ nh vi ngÆ°á»i dĂ¹ng, thiết kế wireframe và prototype', 1);

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
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idMC, 'MC-PR', N'Quan hệ công chúng & Quản trị sự kiện (PR & Event)', N'Truyá»n thĂ´ng thÆ°Æ¡ng hiá»‡u, xá»­ lĂ½ khá»§ng hoáº£ng và tổ chức sự kiện', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'MC-NM')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idMC, 'MC-NM', N'Báo chí số & Sáng tạo nội dung (Digital Journalism)', N'Sáº£n xuáº¥t video, podcast và báo chí truyá»n thĂ´ng sá»‘', 1);

-- EN
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'EN-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idEN, 'EN-BC', N'Tiếng Anh thương mại (Business Communication)', N'Giao tiếp đàm phán quốc tế và hợp đồng thương mại', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'EN-TI')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idEN, 'EN-TI', N'Tiếng Anh Biên phiên dịch (Translation & Interpretation)', N'Biên dịch tài liệu chuyên ngành và phiên dịch hội thảo', 1);

-- JP
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'JP-IT')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idJP, 'JP-IT', N'Tiếng Nhật Công nghệ thông tin (IT Japanese)', N'Cáº§u ná»‘i phát triển pháº§n má»m Bridge SE cho doanh nghiá»‡p Nháº­t', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'JP-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idJP, 'JP-BC', N'Tiếng Nhật Thương mại (Business Japanese)', N'Giao tiếp văn phòng và thương mại với đối tác Nhật Bản', 1);

-- KR
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'KR-BC')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idKR, 'KR-BC', N'Tiếng Hàn Thương mại (Business Korean)', N'Làm việc tại các tập đoàn đa quốc gia Hàn Quốc (Samsung, LG...)', 1);
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE Code = 'KR-TI')
    INSERT INTO Specializations (MajorId, Code, Name, Description, IsActive) VALUES (@idKR, 'KR-TI', N'Tiếng Hàn Biên phiên dịch (Korean Translation)', N'Dịch thuật cabin, hội nghị và tài liệu kinh tế', 1);

PRINT N'==> ÄĂ£ cáº­p nháº­t 26 chuyên ngành háº¹p FPT.';

-- 3. Há»ŒC Ká»² (SEMESTERS)
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'FA23')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('FA23', N'Há»c ká»³ Fall 2023', '2023-09-05', '2023-12-25', '2023-09-20', '2023-12-30', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SP24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SP24', N'Há»c ká»³ Spring 2024', '2024-01-08', '2024-04-25', '2024-01-20', '2024-04-30', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SU24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SU24', N'Há»c ká»³ Summer 2024', '2024-05-06', '2024-08-20', '2024-05-18', '2024-08-25', 'Closed', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'FA24')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('FA24', N'Há»c ká»³ Fall 2024 (Hiá»‡n táº¡i)', '2024-09-03', '2024-12-28', '2024-09-18', '2024-12-30', 'Active', 0);

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SP25')
    INSERT INTO Semesters (Code, Name, StartDate, EndDate, TransferClassDeadline, GradeEntryDeadline, Status, IsClosedWithExceptions)
    VALUES ('SP25', N'Há»c ká»³ Spring 2025 (Sáº¯p tá»›i)', '2025-01-06', '2025-04-26', '2025-01-18', '2025-04-30', 'Upcoming', 0);

PRINT N'==> ÄĂ£ cáº­p nháº­t cĂ¡c há»c ká»³.';

-- 4. DANH SĂCH MĂ”N Há»ŒC CHUáº¨N FPT (SUBJECTS)
CREATE TABLE #TmpSubjects (Code NVARCHAR(50), Name NVARCHAR(255), Credits INT);
INSERT INTO #TmpSubjects VALUES
('MAE101', N'ToĂ¡n cao cáº¥p cho ká»¹ thuáº­t (Mathematics for Engineering)', 3),
('MAD101', N'ToĂ¡n rá»i ráº¡c (Discrete Mathematics)', 3),
('MAS291', N'XĂ¡c suáº¥t và Thá»‘ng kĂª (Probability & Statistics)', 3),
('SSL101c', N'Ká»¹ nÄƒng há»c táº­p Ä‘áº¡i há»c (Academic Skills)', 3),
('EXE101', N'Khá»Ÿi nghiá»‡p Ä‘á»•i má»›i sĂ¡ng táº¡o (Experiential Entrepreneurship 1)', 3),
('CSD201', N'Cấu trúc dữ liệu và giáº£i thuáº­t (Data Structures & Algorithms)', 3),
('DBI202', N'Giá»›i thiá»‡u Há»‡ quáº£n trá»‹ CSDL (Introduction to Databases)', 3),
('PRJ301', N'Lập trình ứng dụng Java Web (Java Web Application Development)', 3),
('SWE201c', N'Nháº­p mĂ´n Ká»¹ thuáº­t pháº§n má»m (Introduction to Software Engineering)', 3),
('SWT301', N'Kiá»ƒm thá»­ pháº§n má»m (Software Testing)', 3),
('SWR302', N'Ká»¹ thuáº­t yĂªu cáº§u pháº§n má»m (Software Requirements)', 3),
('SWD392', N'Kiáº¿n trĂºc và thiết kế pháº§n má»m (Software Architecture & Design)', 3),
('PRM392', N'Lập trình ứng dụng di Ä‘á»™ng (Mobile Programming)', 3),
('NWC203c', N'Máº¡ng mĂ¡y tĂ­nh cÄƒn báº£n (Computer Networking)', 3),
('OSG202', N'Há»‡ Ä‘iá»u hĂ nh (Operating Systems)', 3),
('IAE201', N'NguyĂªn lĂ½ An toĂ n thông tin (Principles of Information Assurance)', 3),
('IAS301', N'An ninh máº¡ng & PhĂ¢n tĂ­ch lá»— há»•ng (Network Security & Vulnerability)', 3),
('CRY301', N'Máº­t mĂ£ há»c và ứng dụng (Cryptography)', 3),
('AIL302m', N'TrĂ­ tuá»‡ nhĂ¢n táº¡o & Machine Learning (Machine Learning Fundamentals)', 3),
('DBS301', N'Hệ thống cơ sở dữ liệu nâng cao & NoSQL (Advanced Database Systems)', 3),
('DVP301', N'Trá»±c quan hĂ³a dữ liệu với Python & BI (Data Visualization)', 3),
('DES101', N'NguyĂªn lĂ½ thiết kế má»¹ thuáº­t sá»‘ (Principles of Visual Design)', 3),
('VFX201', N'Kỹ xảo hình ảnh & Hoáº¡t há»a ká»¹ thuáº­t sá»‘ (Visual Effects & 3D)', 3),
('UIX201', N'Thiết kế giao diá»‡n & Tráº£i nghiá»‡m ngÆ°á»i dĂ¹ng (UI/UX Design Studio)', 3),
('MKT101', N'NguyĂªn lĂ½ Marketing (Principles of Marketing)', 3),
('ECO111', N'Kinh táº¿ vi mĂ´ (Microeconomics)', 3),
('OBE102', N'HĂ nh vi tổ chức (Organizational Behavior)', 3),
('FIN301', N'Quản trị tĂ i chĂ­nh doanh nghiá»‡p (Corporate Finance)', 3),
('MMC101', N'Nháº­p mĂ´n Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n (Intro to Multimedia)', 3),
('PRS201', N'Ká»¹ nÄƒng viáº¿t và Quan há»‡ cĂ´ng chĂºng (Public Relations Writing)', 3),
('ENW492', N'Viáº¿t há»c thuáº­t và Tiáº¿ng Anh nghá» nghiá»‡p (Academic English)', 3),
('JPD113', N'Tiáº¿ng Nháº­t sÆ¡ cáº¥p 1 (Elementary Japanese 1)', 3),
('JPD123', N'Tiáº¿ng Nháº­t sÆ¡ cáº¥p 2 (Elementary Japanese 2)', 3),
('KRD113', N'Tiáº¿ng HĂ n sÆ¡ cáº¥p 1 (Elementary Korean 1)', 3),
('KRD123', N'Tiáº¿ng HĂ n sÆ¡ cáº¥p 2 (Elementary Korean 2)', 3);

DECLARE @subCode NVARCHAR(50), @subName NVARCHAR(255), @subCred INT;
DECLARE curSub CURSOR FOR SELECT Code, Name, Credits FROM #TmpSubjects;
OPEN curSub;
FETCH NEXT FROM curSub INTO @subCode, @subName, @subCred;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Subjects WHERE Code = @subCode)
    BEGIN
        INSERT INTO Subjects (Code, Name, Credits, Description, IsActive)
        VALUES (@subCode, @subName, @subCred, N'MĂ´n há»c chÆ°Æ¡ng trĂ¬nh chuáº©n Äáº¡i há»c FPT', 1);

        DECLARE @newSubId INT = SCOPE_IDENTITY();
        -- Cáº¥u hĂ¬nh Ä‘áº§u Ä‘iá»ƒm chuáº©n 100%: Quiz 10%, Lab 20%, Progress Test 30%, Final Exam 40%
        INSERT INTO SubjectGradeComponents (SubjectId, Name, Weight, Scale, IsAdminOnly, OrderIndex)
        VALUES 
        (@newSubId, N'Quiz / Äiá»ƒm danh', 10.0, 10.0, 0, 1),
        (@newSubId, N'BĂ i thá»±c hĂ nh (Lab)', 20.0, 10.0, 0, 2),
        (@newSubId, N'Kiá»ƒm tra tiến độ (Progress Test)', 30.0, 10.0, 0, 3),
        (@newSubId, N'Thi káº¿t thĂºc mĂ´n (Final Exam)', 40.0, 10.0, 1, 4);
    END
    FETCH NEXT FROM curSub INTO @subCode, @subName, @subCred;
END
CLOSE curSub;
DEALLOCATE curSub;
DROP TABLE #TmpSubjects;

PRINT N'==> ÄĂ£ cáº­p nháº­t toĂ n bá»™ 40 mĂ´n há»c FPT và cáº¥u hĂ¬nh Ä‘áº§u Ä‘iá»ƒm 100%.';

-- MĂ´n tiĂªn quyáº¿t
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

PRINT N'==> ÄĂ£ cáº­p nháº­t cĂ¢y Ä‘iá»u kiá»‡n mĂ´n tiĂªn quyáº¿t.';

-- 5. Lá»P HĂ€NH CHĂNH (ADMINISTRATIVE CLASSES)
CREATE TABLE #TmpClasses (Code NVARCHAR(50), Name NVARCHAR(255), MajorCode NVARCHAR(50), Cohort INT, Year NVARCHAR(50), Sem INT);
INSERT INTO #TmpClasses VALUES
('SE1703', N'Ká»¹ thuáº­t pháº§n má»m K17 Lớp 3', 'SE', 17, '2023-2027', 5),
('SE1801', N'Ká»¹ thuáº­t pháº§n má»m K18 Lớp 1', 'SE', 18, '2024-2028', 3),
('SE1802', N'Ká»¹ thuáº­t pháº§n má»m K18 Lớp 2', 'SE', 18, '2024-2028', 3),
('IA1701', N'An toĂ n thông tin K17 Lớp 1', 'IA', 17, '2023-2027', 5),
('IA1801', N'An toĂ n thông tin K18 Lớp 1', 'IA', 18, '2024-2028', 3),
('AI1701', N'TrĂ­ tuá»‡ nhĂ¢n táº¡o K17 Lớp 1', 'AI', 17, '2023-2027', 5),
('AI1801', N'TrĂ­ tuá»‡ nhĂ¢n táº¡o K18 Lớp 1', 'AI', 18, '2024-2028', 3),
('DS1701', N'Khoa há»c dữ liệu K17 Lớp 1', 'DS', 17, '2023-2027', 5),
('IT1701', N'CĂ´ng nghá»‡ thông tin K17 Lớp 1', 'IT', 17, '2023-2027', 5),
('GD1701', N'Thiết kế Ä‘á»“ há»a K17 Lớp 1', 'GD', 17, '2023-2027', 5),
('GD1801', N'Thiết kế Ä‘á»“ há»a K18 Lớp 1', 'GD', 18, '2024-2028', 3),
('BA1701', N'Quản trị kinh doanh K17 Lớp 1', 'BA', 17, '2023-2027', 5),
('BA1702', N'Quản trị kinh doanh K17 Lớp 2', 'BA', 17, '2023-2027', 5),
('BA1801', N'Quản trị kinh doanh K18 Lớp 1', 'BA', 18, '2024-2028', 3),
('MC1701', N'Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n K17 Lớp 1', 'MC', 17, '2023-2027', 5),
('MC1801', N'Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n K18 Lớp 1', 'MC', 18, '2024-2028', 3),
('EN1701', N'NgĂ´n ngá»¯ Anh K17 Lớp 1', 'EN', 17, '2023-2027', 5),
('JP1701', N'NgĂ´n ngá»¯ Nháº­t K17 Lớp 1', 'JP', 17, '2023-2027', 5),
('KR1701', N'NgĂ´n ngá»¯ HĂ n K17 Lớp 1', 'KR', 17, '2023-2027', 5),
('DS1801', N'Khoa há»c dữ liệu K18 Lớp 1', 'DS', 18, '2024-2028', 3),
('IT1801', N'CĂ´ng nghá»‡ thông tin K18 Lớp 1', 'IT', 18, '2024-2028', 3),
('EN1801', N'NgĂ´n ngá»¯ Anh K18 Lớp 1', 'EN', 18, '2024-2028', 3),
('JP1801', N'NgĂ´n ngá»¯ Nháº­t K18 Lớp 1', 'JP', 18, '2024-2028', 3),
('KR1801', N'NgĂ´n ngá»¯ HĂ n K18 Lớp 1', 'KR', 18, '2024-2028', 3);

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

PRINT N'==> ÄĂ£ cáº­p nháº­t 21 lá»›p hĂ nh chĂ­nh phá»§ kháº¯p cĂ¡c ngành.';

-- 6. TĂ€I KHOáº¢N GIáº¢NG VIĂN (LECTURERS)
DECLARE @pwdHash NVARCHAR(255) = N'$2a$11$MZ2SbSCMUf8iv/WabQ7B5.qrWIbNGMG3SgJQTcLrL3iXkJFl4BcaW';

CREATE TABLE #TmpLecturers (Username NVARCHAR(50), FullName NVARCHAR(100), Email NVARCHAR(100), Phone NVARCHAR(50), Code NVARCHAR(50), Dept NVARCHAR(100), Degree NVARCHAR(50));
INSERT INTO #TmpLecturers VALUES
('gv_cuongnk', N'TS. Nguyá»…n Kim CÆ°Æ¡ng', 'cuongnk@fpt.edu.vn', '0912111222', 'GV003', N'Ká»¹ thuáº­t pháº§n má»m', 'PhD'),
('gv_sondh', N'ThS. Äá»— HoĂ ng SÆ¡n', 'sondh@fpt.edu.vn', '0912222333', 'GV004', N'An toĂ n thông tin', 'Master'),
('gv_lanpt', N'TS. Pháº¡m Thá»‹ Lan', 'lanpt@fpt.edu.vn', '0912333444', 'GV005', N'TrĂ­ tuá»‡ nhĂ¢n táº¡o & Dá»¯ liá»‡u', 'PhD'),
('gv_namth', N'ThS. Tráº§n Háº£i Nam', 'namth@fpt.edu.vn', '0912444555', 'GV006', N'Há»‡ thá»‘ng thông tin & Máº¡ng', 'Master'),
('gv_trivm', N'ThS. VÅ© Minh TrĂ­', 'trivm@fpt.edu.vn', '0912555666', 'GV007', N'Thiết kế Má»¹ thuáº­t sá»‘', 'Master'),
('gv_huonghm', N'TS. HoĂ ng Mai HÆ°Æ¡ng', 'huonghm@fpt.edu.vn', '0912666777', 'GV008', N'Quản trị kinh doanh', 'PhD'),
('gv_kietdt', N'ThS. Äáº·ng Tuáº¥n Kiá»‡t', 'kietdt@fpt.edu.vn', '0912777888', 'GV009', N'Marketing & Kinh doanh sá»‘', 'Master'),
('gv_thaobp', N'ThS. BĂ¹i PhÆ°Æ¡ng Tháº£o', 'thaobp@fpt.edu.vn', '0912888999', 'GV010', N'Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n', 'Master'),
('gv_john', N'ThS. John Smith', 'johnsmith@fpt.edu.vn', '0912999000', 'GV011', N'NgĂ´n ngá»¯ Anh', 'Master'),
('gv_tanaka', N'ThS. Tanaka Hiroshi', 'tanaka@fpt.edu.vn', '0913000111', 'GV012', N'NgĂ´n ngá»¯ Nháº­t', 'Master'),
('gv_park', N'ThS. Park Min Woo', 'parkmw@fpt.edu.vn', '0913111222', 'GV013', N'NgĂ´n ngá»¯ HĂ n', 'Master');

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

PRINT N'==> ÄĂ£ cáº­p nháº­t 13 giảng viên cĂ¡c bá»™ mĂ´n.';

-- 7. THĂM THáº¬T NHIá»€U SINH VIĂN (STUDENTS) VĂ€O Táº¤T Cáº¢ CĂC NGĂ€NH
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
-- KHá»I CĂ”NG NGHá»† THĂ”NG TIN (MĂƒ HE)
-- SE - Ká»¹ thuáº­t pháº§n má»m
('he170010', N'Nguyá»…n HoĂ ng Long', 'longnhhe170010@fpt.edu.vn', '0981000010', 'HE170010', 'SE', 'SE-IS', 'SE1701', 17, 5),
('he170011', N'VÅ© ÄĂ¬nh Trá»ng', 'trongvdhe170011@fpt.edu.vn', '0981000011', 'HE170011', 'SE', 'SE-IS', 'SE1701', 17, 5),
('he170012', N'Äá»— Quá»‘c Báº£o', 'baodqhe170012@fpt.edu.vn', '0981000012', 'HE170012', 'SE', 'SE-JS', 'SE1701', 17, 5),
('he170013', N'BĂ¹i PhÆ°Æ¡ng Linh', 'linhbphe170013@fpt.edu.vn', '0981000013', 'HE170013', 'SE', 'SE-JS', 'SE1701', 17, 5),
('he170014', N'HoĂ ng Tuáº¥n DÅ©ng', 'dunghthe170014@fpt.edu.vn', '0981000014', 'HE170014', 'SE', 'SE-ES', 'SE1702', 17, 5),
('he170015', N'LĂª Thá»‹ Thu Trang', 'trangltthe170015@fpt.edu.vn', '0981000015', 'HE170015', 'SE', 'SE-ES', 'SE1702', 17, 5),
('he170016', N'Tráº§n Minh Äá»©c', 'ductmhe170016@fpt.edu.vn', '0981000016', 'HE170016', 'SE', 'SE-AS', 'SE1702', 17, 5),
('he170017', N'NgĂ´ Thanh TĂ¹ng', 'tungnthe170017@fpt.edu.vn', '0981000017', 'HE170017', 'SE', 'SE-AS', 'SE1702', 17, 5),
('he170018', N'Pháº¡m Gia Huy', 'huypghe170018@fpt.edu.vn', '0981000018', 'HE170018', 'SE', 'SE-IS', 'SE1703', 17, 5),
('he170019', N'Äinh Báº£o Ngá»c', 'ngocdbhe170019@fpt.edu.vn', '0981000019', 'HE170019', 'SE', 'SE-JS', 'SE1703', 17, 5),
('he180001', N'Nguyá»…n Quang Háº£i', 'hainqhe180001@fpt.edu.vn', '0981000101', 'HE180001', 'SE', NULL, 'SE1801', 18, 3),
('he180002', N'ÄoĂ n VÄƒn Háº­u', 'haudvhe180002@fpt.edu.vn', '0981000102', 'HE180002', 'SE', NULL, 'SE1801', 18, 3),
('he180003', N'Nguyá»…n Tiáº¿n Linh', 'linhnthe180003@fpt.edu.vn', '0981000103', 'HE180003', 'SE', NULL, 'SE1802', 18, 3),
('he180004', N'Phan VÄƒn Äá»©c', 'ducpvhe180004@fpt.edu.vn', '0981000104', 'HE180004', 'SE', NULL, 'SE1802', 18, 3),

-- IA - An toĂ n thông tin
('he170101', N'Trá»‹nh XuĂ¢n BĂ¡ch', 'bachtxhe170101@fpt.edu.vn', '0982000001', 'HE170101', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170102', N'DÆ°Æ¡ng ThĂ¹y Chi', 'chidtdhe170102@fpt.edu.vn', '0982000002', 'HE170102', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170103', N'Táº¡ Quang Tháº¯ng', 'thangtqhe170103@fpt.edu.vn', '0982000003', 'HE170103', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he170104', N'Cao HoĂ i Nam', 'namchhe170104@fpt.edu.vn', '0982000004', 'HE170104', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he180101', N'LÆ°u ÄĂ¬nh Phong', 'phongldhe180101@fpt.edu.vn', '0982000101', 'HE180101', 'IA', NULL, 'IA1801', 18, 3),
('he180102', N'Mai Há»“ng QuĂ¢n', 'quanmhhe180102@fpt.edu.vn', '0982000102', 'HE180102', 'IA', NULL, 'IA1801', 18, 3),

-- AI - TrĂ­ tuá»‡ nhĂ¢n táº¡o
('he170201', N'Pháº¡m Minh Tuáº¥n', 'tuanpmhe170201@fpt.edu.vn', '0983000001', 'HE170201', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170202', N'Nguyá»…n KhĂ¡nh Vy', 'vynkhe170202@fpt.edu.vn', '0983000002', 'HE170202', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170203', N'Tráº§n Quá»‘c Anh', 'anhtqhe170203@fpt.edu.vn', '0983000003', 'HE170203', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he170204', N'LĂª Minh ChĂ¢u', 'chaulmhe170204@fpt.edu.vn', '0983000004', 'HE170204', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he180201', N'VĂµ Táº¥n PhĂ¡t', 'phatvthe180201@fpt.edu.vn', '0983000101', 'HE180201', 'AI', NULL, 'AI1801', 18, 3),
('he180202', N'HoĂ ng Yáº¿n Nhi', 'nhiyhhe180202@fpt.edu.vn', '0983000102', 'HE180202', 'AI', NULL, 'AI1801', 18, 3),

-- DS - Khoa há»c dữ liệu
('he170301', N'Äá»— HoĂ ng PhĂºc', 'phucdhhe170301@fpt.edu.vn', '0984000001', 'HE170301', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170302', N'Nguyá»…n ThĂºy Háº±ng', 'hangnthe170302@fpt.edu.vn', '0984000002', 'HE170302', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170303', N'Tráº§n Äáº¡i NghÄ©a', 'nghiatdhe170303@fpt.edu.vn', '0984000003', 'HE170303', 'DS', 'DS-DE', 'DS1701', 17, 5),
('he170304', N'Phan Thanh BĂ¬nh', 'binhpthe170304@fpt.edu.vn', '0984000004', 'HE170304', 'DS', 'DS-DE', 'DS1701', 17, 5),

-- IT - CĂ´ng nghá»‡ thông tin
('he170401', N'LĂª VÄƒn Tháº¯ng', 'thanglvhe170401@fpt.edu.vn', '0985000001', 'HE170401', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170402', N'BĂ¹i Thá»‹ Ănh', 'anhbthe170402@fpt.edu.vn', '0985000002', 'HE170402', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170403', N'Nguyá»…n VÄƒn ToĂ n', 'toannvhe170403@fpt.edu.vn', '0985000003', 'HE170403', 'IT', 'IT-SA', 'IT1701', 17, 5),
('he170404', N'HoĂ ng Trung KiĂªn', 'kienhthe170404@fpt.edu.vn', '0985000004', 'HE170404', 'IT', 'IT-SA', 'IT1701', 17, 5),

-- GD - Thiết kế Ä‘á»“ há»a
('he170501', N'Nguyá»…n ThĂ¹y Dung', 'dungnthe170501@fpt.edu.vn', '0986000001', 'HE170501', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170502', N'TrÆ°Æ¡ng Má»¹ DuyĂªn', 'duyentmhe170502@fpt.edu.vn', '0986000002', 'HE170502', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170503', N'Pháº¡m ÄÄƒng KhĂ´i', 'khoipdhe170503@fpt.edu.vn', '0986000003', 'HE170503', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he170504', N'LĂ½ NhĂ£ Kỳ', 'kylnhe170504@fpt.edu.vn', '0986000004', 'HE170504', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he180501', N'HĂ  Anh Tuáº¥n', 'tuanhahe180501@fpt.edu.vn', '0986000101', 'HE180501', 'GD', NULL, 'GD1801', 18, 3),
('he180502', N'Trá»‹nh ThÄƒng BĂ¬nh', 'binhtthe180502@fpt.edu.vn', '0986000102', 'HE180502', 'GD', NULL, 'GD1801', 18, 3),

-- KHá»I MARKETING VĂ€ TRUYá»€N THĂ”NG (MĂƒ HS)
-- BA - Quản trị kinh doanh & Marketing
('hs170001', N'Nguyá»…n Trá»ng Äáº¡i', 'dainths170001@fpt.edu.vn', '0987000001', 'HS170001', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170002', N'LĂª Báº£o TrĂ¢m', 'tramlths170002@fpt.edu.vn', '0987000002', 'HS170002', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170003', N'Tráº§n Ngá»c HĂ¢n', 'hantnhs170003@fpt.edu.vn', '0987000003', 'HS170003', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170004', N'VÅ© Äá»©c Thá»‹nh', 'thinhvdhs170004@fpt.edu.vn', '0987000004', 'HS170004', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170005', N'HoĂ ng Gia CĂ¡t', 'cathghs170005@fpt.edu.vn', '0987000005', 'HS170005', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170006', N'Äáº·ng PhÆ°Æ¡ng Mai', 'maidphs170006@fpt.edu.vn', '0987000006', 'HS170006', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170007', N'BĂ¹i Anh Tuáº¥n', 'tuanbahs170007@fpt.edu.vn', '0987000007', 'HS170007', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs170008', N'NgĂ´ Kiá»u Oanh', 'oanhnkhs170008@fpt.edu.vn', '0987000008', 'HS170008', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs180001', N'Phan Thanh Háº­u', 'haupthhs180001@fpt.edu.vn', '0987000101', 'HS180001', 'BA', NULL, 'BA1801', 18, 3),
('hs180002', N'LÆ°Æ¡ng XuĂ¢n TrÆ°á»ng', 'truonglxhs180002@fpt.edu.vn', '0987000102', 'HS180002', 'BA', NULL, 'BA1801', 18, 3),

-- MC - Truyá»n thĂ´ng Ä‘a phÆ°Æ¡ng tiá»‡n
('hs170101', N'Tráº§n Minh TĂ¢m', 'tamtm_hs17@fpt.edu.vn', '0988000001', 'HS170101', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170102', N'VÅ© ThĂ¹y Linh', 'linhvt_hs17@fpt.edu.vn', '0988000002', 'HS170102', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170103', N'Äá»— Tuáº¥n Khang', 'khangdt_hs17@fpt.edu.vn', '0988000003', 'HS170103', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs170104', N'LĂª Diá»…m My', 'myld_hs17@fpt.edu.vn', '0988000004', 'HS170104', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs180101', N'Nguyá»…n Quang DÅ©ng', 'dungnq_hs18@fpt.edu.vn', '0988000101', 'HS180101', 'MC', NULL, 'MC1801', 18, 3),
('hs180102', N'Pháº¡m HÆ°Æ¡ng Ly', 'lyph_hs18@fpt.edu.vn', '0988000102', 'HS180102', 'MC', NULL, 'MC1801', 18, 3),

-- KHá»I NGĂ”N NGá»® (MĂƒ HA)
-- EN, JP, KR - NgĂ´n ngá»¯
('ha170001', N'Tráº§n Kim NgĂ¢n', 'ngantk_ha17@fpt.edu.vn', '0989000001', 'HA170001', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170002', N'Nguyá»…n HoĂ ng BĂ¡ch', 'bachnh_ha17@fpt.edu.vn', '0989000002', 'HA170002', 'EN', 'EN-TI', 'EN1701', 17, 5),
('ha170003', N'LĂª Minh Tuáº¥n', 'tuanlm_ha17@fpt.edu.vn', '0989000003', 'HA170003', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170101', N'HoĂ ng Mai Anh', 'anhhm_ha17@fpt.edu.vn', '0989000011', 'HA170101', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170102', N'Äá»— Nháº­t Quang', 'quangdn_ha17@fpt.edu.vn', '0989000012', 'HA170102', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170103', N'Pháº¡m Thu Tháº£o', 'thaopt_ha17@fpt.edu.vn', '0989000013', 'HA170103', 'JP', 'JP-BC', 'JP1701', 17, 5),
('ha170201', N'VÅ© Ngá»c Lan', 'lanvn_ha17@fpt.edu.vn', '0989000021', 'HA170201', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha170202', N'Tráº§n ÄÄƒng Khoa', 'khoatd_ha17@fpt.edu.vn', '0989000022', 'HA170202', 'KR', 'KR-TI', 'KR1701', 17, 5),
('ha170203', N'Nguyá»…n KhĂ¡nh Linh', 'linhnk_ha17@fpt.edu.vn', '0989000023', 'HA170203', 'KR', 'KR-BC', 'KR1701', 17, 5),

-- KHá»I CNTT Bá»” SUNG (MĂƒ HE)
('he180010', N'Nguyá»…n Háº£i ÄÄƒng', 'dangnhhe180010@fpt.edu.vn', '0981100010', 'HE180010', 'SE', NULL, 'SE1801', 18, 3),
('he180011', N'Tráº§n Quá»‘c Huy', 'huytqhe180011@fpt.edu.vn', '0981100011', 'HE180011', 'SE', NULL, 'SE1801', 18, 3),
('he180012', N'LĂª KhĂ¡nh ToĂ n', 'toanlkhe180012@fpt.edu.vn', '0981100012', 'HE180012', 'SE', NULL, 'SE1802', 18, 3),
('he180013', N'Pháº¡m Thanh Tháº£o', 'thaoptthhe180013@fpt.edu.vn', '0981100013', 'HE180013', 'SE', NULL, 'SE1802', 18, 3),
('he190001', N'Äáº·ng Quang Kháº£i', 'khaidqhe190001@fpt.edu.vn', '0981100014', 'HE190001', 'SE', NULL, 'SE1801', 18, 3),
('he170020', N'VĂµ HoĂ ng Yáº¿n', 'yenvhhe170020@fpt.edu.vn', '0981100020', 'HE170020', 'SE', 'SE-IS', 'SE1703', 17, 5),
('he170021', N'Nguyá»…n ThĂ nh Äáº¡t', 'datnthe170021@fpt.edu.vn', '0981100021', 'HE170021', 'SE', 'SE-JS', 'SE1703', 17, 5),
('he170105', N'BĂ¹i Minh TrĂ­', 'tribmhe170105@fpt.edu.vn', '0982100005', 'HE170105', 'IA', 'IA-NW', 'IA1701', 17, 5),
('he170106', N'Äá»— Thu HĂ ', 'hadthe170106@fpt.edu.vn', '0982100006', 'HE170106', 'IA', 'IA-DF', 'IA1701', 17, 5),
('he180103', N'TrÆ°Æ¡ng Gia BĂ¬nh', 'binhtghe180103@fpt.edu.vn', '0982100103', 'HE180103', 'IA', NULL, 'IA1801', 18, 3),
('he180104', N'HoĂ ng Trá»ng NghÄ©a', 'nghiahthe180104@fpt.edu.vn', '0982100104', 'HE180104', 'IA', NULL, 'IA1801', 18, 3),
('he180105', N'Nguyá»…n Tuáº¥n Anh', 'anhnthe180105@fpt.edu.vn', '0982100105', 'HE180105', 'IA', NULL, 'IA1801', 18, 3),
('he170205', N'VÅ© Anh Khoa', 'khoavakhe170205@fpt.edu.vn', '0983100005', 'HE170205', 'AI', 'AI-ML', 'AI1701', 17, 5),
('he170206', N'LĂª Tháº£o NguyĂªn', 'nguyenlthe170206@fpt.edu.vn', '0983100006', 'HE170206', 'AI', 'AI-CV', 'AI1701', 17, 5),
('he180203', N'Äinh Quang DÅ©ng', 'dungdqhe180203@fpt.edu.vn', '0983100103', 'HE180203', 'AI', NULL, 'AI1801', 18, 3),
('he180204', N'Tráº§n Ngá»c Lan', 'lantnhe180204@fpt.edu.vn', '0983100104', 'HE180204', 'AI', NULL, 'AI1801', 18, 3),
('he180205', N'Pháº¡m Quá»‘c CÆ°á»ng', 'cuongpqhe180205@fpt.edu.vn', '0983100105', 'HE180205', 'AI', NULL, 'AI1801', 18, 3),
('he170305', N'Nguyá»…n VÄƒn Hiáº¿u', 'hieunvhe170305@fpt.edu.vn', '0984100005', 'HE170305', 'DS', 'DS-BA', 'DS1701', 17, 5),
('he170306', N'BĂ¹i ThĂ¹y TiĂªn', 'tienbthe170306@fpt.edu.vn', '0984100006', 'HE170306', 'DS', 'DS-DE', 'DS1701', 17, 5),
('he180301', N'Phan Nháº­t Minh', 'minhpnhe180301@fpt.edu.vn', '0984100101', 'HE180301', 'DS', NULL, 'DS1801', 18, 3),
('he180302', N'ÄoĂ n Tuáº¥n Phong', 'phongdthe180302@fpt.edu.vn', '0984100102', 'HE180302', 'DS', NULL, 'DS1801', 18, 3),
('he170405', N'Tráº§n Äá»©c Duy', 'duytdhe170405@fpt.edu.vn', '0985100005', 'HE170405', 'IT', 'IT-WD', 'IT1701', 17, 5),
('he170406', N'VÅ© Thá»‹ Kim Oanh', 'oanhvtkhe170406@fpt.edu.vn', '0985100006', 'HE170406', 'IT', 'IT-SA', 'IT1701', 17, 5),
('he180401', N'Nguyá»…n Há»¯u PhÆ°á»›c', 'phuocnhhe180401@fpt.edu.vn', '0985100101', 'HE180401', 'IT', NULL, 'IT1801', 18, 3),
('he180402', N'Äáº·ng Há»¯u Tháº¯ng', 'thangdthe180402@fpt.edu.vn', '0985100102', 'HE180402', 'IT', NULL, 'IT1801', 18, 3),
('he170505', N'HoĂ ng Kim NgĂ¢n', 'nganhkhe170505@fpt.edu.vn', '0986100005', 'HE170505', 'GD', 'GD-3D', 'GD1701', 17, 5),
('he170506', N'NgĂ´ Gia Huy', 'huynghe170506@fpt.edu.vn', '0986100006', 'HE170506', 'GD', 'GD-UX', 'GD1701', 17, 5),
('he180503', N'Pháº¡m Mai PhÆ°Æ¡ng', 'phuongmphe180503@fpt.edu.vn', '0986100103', 'HE180503', 'GD', NULL, 'GD1801', 18, 3),
('he180504', N'Trá»‹nh Quang Huy', 'huytqhe180504@fpt.edu.vn', '0986100104', 'HE180504', 'GD', NULL, 'GD1801', 18, 3),
('he180505', N'Äá»— Háº£i Yáº¿n', 'yendhhe180505@fpt.edu.vn', '0986100105', 'HE180505', 'GD', NULL, 'GD1801', 18, 3),

-- KHá»I MARKETING & TRUYá»€N THĂ”NG Bá»” SUNG (MĂƒ HS)
('hs170009', N'Nguyá»…n Báº£o ChĂ¢u', 'chounbhs170009@fpt.edu.vn', '0987100009', 'HS170009', 'BA', 'BA-DM', 'BA1701', 17, 5),
('hs170010', N'LĂª HoĂ ng Kim', 'kimlhhs170010@fpt.edu.vn', '0987100010', 'HS170010', 'BA', 'BA-IB', 'BA1701', 17, 5),
('hs170011', N'Tráº§n Thá»¥c Anh', 'anhtths170011@fpt.edu.vn', '0987100011', 'HS170011', 'BA', 'BA-HM', 'BA1702', 17, 5),
('hs170012', N'Äáº·ng Tuáº¥n Khanh', 'khanhdths170012@fpt.edu.vn', '0987100012', 'HS170012', 'BA', 'BA-FT', 'BA1702', 17, 5),
('hs180003', N'VÅ© Quá»³nh NhÆ°', 'nhuvqhs180003@fpt.edu.vn', '0987100103', 'HS180003', 'BA', NULL, 'BA1801', 18, 3),
('hs180004', N'Phan Quá»‘c Äáº¡t', 'datpqhs180004@fpt.edu.vn', '0987100104', 'HS180004', 'BA', NULL, 'BA1801', 18, 3),
('hs180005', N'BĂ¹i Tháº¿ Anh', 'anhbths180005@fpt.edu.vn', '0987100105', 'HS180005', 'BA', NULL, 'BA1801', 18, 3),
('hs180006', N'Äá»— Thanh Háº±ng', 'hangdths180006@fpt.edu.vn', '0987100106', 'HS180006', 'BA', NULL, 'BA1801', 18, 3),
('hs180007', N'Nguyá»…n Tuáº¥n Kiá»‡t', 'kietnths180007@fpt.edu.vn', '0987100107', 'HS180007', 'BA', NULL, 'BA1801', 18, 3),
('hs180008', N'HoĂ ng Má»¹ TĂ¢m', 'tamhmhs180008@fpt.edu.vn', '0987100108', 'HS180008', 'BA', NULL, 'BA1801', 18, 3),
('hs180009', N'Trá»‹nh Báº£o Nam', 'namtbhs180009@fpt.edu.vn', '0987100109', 'HS180009', 'BA', NULL, 'BA1801', 18, 3),
('hs170105', N'Pháº¡m ThĂ¹y TiĂªn', 'tienpt_hs17@fpt.edu.vn', '0988100005', 'HS170105', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs170106', N'Tráº§n Thanh VĂ¢n', 'vantt_hs17@fpt.edu.vn', '0988100006', 'HS170106', 'MC', 'MC-NM', 'MC1701', 17, 5),
('hs170107', N'LĂª Duy Khoa', 'khoald_hs17@fpt.edu.vn', '0988100007', 'HS170107', 'MC', 'MC-PR', 'MC1701', 17, 5),
('hs180103', N'Nguyá»…n KhĂ¡nh Huyá»n', 'huyennk_hs18@fpt.edu.vn', '0988100103', 'HS180103', 'MC', NULL, 'MC1801', 18, 3),
('hs180104', N'VĂµ ThĂ nh Nam', 'namvt_hs18@fpt.edu.vn', '0988100104', 'HS180104', 'MC', NULL, 'MC1801', 18, 3),
('hs180105', N'Äáº·ng Diá»‡u Linh', 'linhdd_hs18@fpt.edu.vn', '0988100105', 'HS180105', 'MC', NULL, 'MC1801', 18, 3),
('hs180106', N'TrÆ°Æ¡ng TrĂ­ ThĂ´ng', 'thongtt_hs18@fpt.edu.vn', '0988100106', 'HS180106', 'MC', NULL, 'MC1801', 18, 3),
('hs180107', N'HoĂ ng Tháº£o My', 'myht_hs18@fpt.edu.vn', '0988100107', 'HS180107', 'MC', NULL, 'MC1801', 18, 3),
('hs180108', N'VÅ© ThĂ¡i SÆ¡n', 'sonvt_hs18@fpt.edu.vn', '0988100108', 'HS180108', 'MC', NULL, 'MC1801', 18, 3),
('hs180109', N'Nguyá»…n HĂ  PhÆ°Æ¡ng', 'phuongnh_hs18@fpt.edu.vn', '0988100109', 'HS180109', 'MC', NULL, 'MC1801', 18, 3),
('hs180110', N'ÄoĂ n Minh TrĂ­', 'tridm_hs18@fpt.edu.vn', '0988100110', 'HS180110', 'MC', NULL, 'MC1801', 18, 3),

-- KHá»I NGĂ”N NGá»® Bá»” SUNG (MĂƒ HA)
('ha170004', N'Nguyá»…n Thanh Háº±ng', 'hangnt_ha17@fpt.edu.vn', '0989100004', 'HA170004', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha170005', N'Äá»— Quang Minh', 'minhdq_ha17@fpt.edu.vn', '0989100005', 'HA170005', 'EN', 'EN-TI', 'EN1701', 17, 5),
('ha170006', N'LĂª Tháº£o Vy', 'vylt_ha17@fpt.edu.vn', '0989100006', 'HA170006', 'EN', 'EN-BC', 'EN1701', 17, 5),
('ha180001', N'Tráº§n Quá»‘c Báº£o', 'baotq_ha18@fpt.edu.vn', '0989100101', 'HA180001', 'EN', NULL, 'EN1801', 18, 3),
('ha180002', N'BĂ¹i HoĂ ng Oanh', 'oanhbh_ha18@fpt.edu.vn', '0989100102', 'HA180002', 'EN', NULL, 'EN1801', 18, 3),
('ha180003', N'VÅ© ÄĂ¬nh Khang', 'khangvd_ha18@fpt.edu.vn', '0989100103', 'HA180003', 'EN', NULL, 'EN1801', 18, 3),
('ha180004', N'Phan Thá»¥c QuyĂªn', 'quyenpt_ha18@fpt.edu.vn', '0989100104', 'HA180004', 'EN', NULL, 'EN1801', 18, 3),
('ha170104', N'Pháº¡m Gia Báº£o', 'baopg_ha17@fpt.edu.vn', '0989100014', 'HA170104', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha170105', N'Nguyá»…n PhÆ°Æ¡ng UyĂªn', 'uyennp_ha17@fpt.edu.vn', '0989100015', 'HA170105', 'JP', 'JP-BC', 'JP1701', 17, 5),
('ha170106', N'Äáº·ng Minh KhĂ´i', 'khoidm_ha17@fpt.edu.vn', '0989100016', 'HA170106', 'JP', 'JP-IT', 'JP1701', 17, 5),
('ha180101', N'Trá»‹nh ThĂ¹y Trang', 'trangtt_ha18@fpt.edu.vn', '0989100111', 'HA180101', 'JP', NULL, 'JP1801', 18, 3),
('ha180102', N'LĂª VÄƒn Quyáº¿t', 'quyetlv_ha18@fpt.edu.vn', '0989100112', 'HA180102', 'JP', NULL, 'JP1801', 18, 3),
('ha180103', N'HoĂ ng Ănh DÆ°Æ¡ng', 'duongha_ha18@fpt.edu.vn', '0989100113', 'HA180103', 'JP', NULL, 'JP1801', 18, 3),
('ha180104', N'VĂµ Mai Chi', 'chivm_ha18@fpt.edu.vn', '0989100114', 'HA180104', 'JP', NULL, 'JP1801', 18, 3),
('ha170204', N'Tráº§n Diá»‡u HÆ°Æ¡ng', 'huongtd_ha17@fpt.edu.vn', '0989100024', 'HA170204', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha170205', N'VÅ© Anh Tuáº¥n', 'tuanva_ha17@fpt.edu.vn', '0989100025', 'HA170205', 'KR', 'KR-TI', 'KR1701', 17, 5),
('ha170206', N'LĂª Báº£o HĂ¢n', 'hanlb_ha17@fpt.edu.vn', '0989100026', 'HA170206', 'KR', 'KR-BC', 'KR1701', 17, 5),
('ha180201', N'Nguyá»…n Minh Háº¡nh', 'hanhnm_ha18@fpt.edu.vn', '0989100121', 'HA180201', 'KR', NULL, 'KR1801', 18, 3),
('ha180202', N'Äá»— Tuáº¥n Kiá»‡t', 'kietdt_ha18@fpt.edu.vn', '0989100122', 'HA180202', 'KR', NULL, 'KR1801', 18, 3),
('ha180203', N'Phan Ngá»c Diá»‡p', 'dieppn_ha18@fpt.edu.vn', '0989100123', 'HA180203', 'KR', NULL, 'KR1801', 18, 3);

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

PRINT N'==> ÄĂ£ cáº­p nháº­t 60+ sinh viên Ä‘áº§y Ä‘á»§ há»“ sÆ¡ và tĂ i khoáº£n Ä‘Äƒng nháº­p (Admin@123).';

-- 8. Má» THĂM CĂC Lá»P Há»ŒC PHáº¦N (COURSE SECTIONS) & NHáº¬P ÄIá»‚M
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
    VALUES ('SWP391_FA24_01', N'Dự án phát triển pháº§n má»m - Lớp 01', @idSWP391_sub, @semFA24, @gvHuongId, 30, 0, 'Active', GETUTCDATE());
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

PRINT N'==> ÄĂ£ má»Ÿ cĂ¡c lá»›p há»c pháº§n ká»³ FA24.';

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

PRINT N'==> Náº P Dá»® LIá»†U Äáº I Há»ŒC FPT THĂ€NH CĂ”NG VĂ€ HOĂ€N Táº¤T!';
GO



-- ========================================================================================
-- KIá»‚M TRA Tá»”NG Káº¾T Báº¢NG & Sá» DĂ’NG Dá»® LIá»†U ÄĂƒ Náº P
-- ========================================================================================
PRINT N'';
PRINT N'=============================================================================';
PRINT N'  KHá»I Táº O CÆ  Sá» Dá»® LIá»†U SMMS_PRN232 THĂ€NH CĂ”NG!';
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
PRINT N'  DANH SĂCH TĂ€I KHOáº¢N ÄÄ‚NG NHáº¬P MáºªU TRONG Há»† THá»NG (Pass: Admin@123)';
PRINT N'=============================================================================';
SELECT [Id], [Username], [FullName], [Role], [Email], [PhoneNumber] FROM [Users] ORDER BY [Id];
GO
