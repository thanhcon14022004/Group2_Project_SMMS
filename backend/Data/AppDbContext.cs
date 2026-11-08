using Microsoft.EntityFrameworkCore;
using project_PRN232.Models.Entities;

namespace project_PRN232.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Major> Majors => Set<Major>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<SubjectGradeComponent> SubjectGradeComponents => Set<SubjectGradeComponent>();
        public DbSet<PrerequisiteSubject> PrerequisiteSubjects => Set<PrerequisiteSubject>();
        public DbSet<AdministrativeClass> AdministrativeClasses => Set<AdministrativeClass>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Lecturer> Lecturers => Set<Lecturer>();
        public DbSet<LecturerSubject> LecturerSubjects => Set<LecturerSubject>();
        public DbSet<CurriculumConfig> CurriculumConfigs => Set<CurriculumConfig>();
        public DbSet<CurriculumSubject> CurriculumSubjects => Set<CurriculumSubject>();
        public DbSet<Semester> Semesters => Set<Semester>();
        public DbSet<CourseSection> CourseSections => Set<CourseSection>();
        public DbSet<CourseSectionSchedule> CourseSectionSchedules => Set<CourseSectionSchedule>();
        public DbSet<CourseSectionStudent> CourseSectionStudents => Set<CourseSectionStudent>();
        public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();
        public DbSet<ClassTransferRequest> ClassTransferRequests => Set<ClassTransferRequest>();
        public DbSet<SpecializationSelection> SpecializationSelections => Set<SpecializationSelection>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.StudentCode).IsUnique();

                entity.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Major)
                    .WithMany(m => m.Students)
                    .HasForeignKey(s => s.MajorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Specialization)
                    .WithMany(sp => sp.Students)
                    .HasForeignKey(s => s.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.AdministrativeClass)
                    .WithMany(ac => ac.Students)
                    .HasForeignKey(s => s.AdministrativeClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Lecturer
            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.HasIndex(l => l.LecturerCode).IsUnique();

                entity.HasOne(l => l.User)
                    .WithOne(u => u.Lecturer)
                    .HasForeignKey<Lecturer>(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Major & Specialization
            modelBuilder.Entity<Major>(entity =>
            {
                entity.HasIndex(m => m.Code).IsUnique();
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasIndex(s => s.Code).IsUnique();

                entity.HasOne(s => s.Major)
                    .WithMany(m => m.Specializations)
                    .HasForeignKey(s => s.MajorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Subject & Grade Components
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasIndex(s => s.Code).IsUnique();
                entity.HasOne(s => s.Major)
                    .WithMany()
                    .HasForeignKey(s => s.MajorId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(s => s.Specialization)
                    .WithMany()
                    .HasForeignKey(s => s.SpecializationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SubjectGradeComponent>(entity =>
            {
                entity.Property(c => c.Weight).HasPrecision(5, 2);
                entity.Property(c => c.Scale).HasPrecision(5, 2);

                entity.HasOne(c => c.Subject)
                    .WithMany(s => s.GradeComponents)
                    .HasForeignKey(c => c.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Prerequisite Subjects
            modelBuilder.Entity<PrerequisiteSubject>(entity =>
            {
                entity.Property(p => p.MinScore).HasPrecision(4, 2);

                entity.HasOne(p => p.Subject)
                    .WithMany(s => s.PrerequisiteRequirements)
                    .HasForeignKey(p => p.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Prerequisite)
                    .WithMany(s => s.PrerequisiteOf)
                    .HasForeignKey(p => p.PrerequisiteSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AdministrativeClass
            modelBuilder.Entity<AdministrativeClass>(entity =>
            {
                entity.HasIndex(c => c.Code).IsUnique();

                entity.HasOne(c => c.Major)
                    .WithMany(m => m.AdministrativeClasses)
                    .HasForeignKey(c => c.MajorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Specialization)
                    .WithMany()
                    .HasForeignKey(c => c.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Curriculum
            modelBuilder.Entity<CurriculumConfig>(entity =>
            {
                entity.HasIndex(c => new { c.MajorId, c.SemesterNumber });

                entity.HasOne(c => c.Major)
                    .WithMany(m => m.CurriculumConfigs)
                    .HasForeignKey(c => c.MajorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Specialization)
                    .WithMany(s => s.CurriculumConfigs)
                    .HasForeignKey(c => c.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CurriculumSubject>(entity =>
            {
                entity.HasOne(cs => cs.CurriculumConfig)
                    .WithMany(c => c.CurriculumSubjects)
                    .HasForeignKey(cs => cs.CurriculumConfigId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cs => cs.Subject)
                    .WithMany(s => s.CurriculumSubjects)
                    .HasForeignKey(cs => cs.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Semester
            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasIndex(s => s.Code).IsUnique();
            });

            // CourseSection
            modelBuilder.Entity<CourseSection>(entity =>
            {
                entity.HasIndex(cs => cs.Code).IsUnique();

                entity.HasOne(cs => cs.Subject)
                    .WithMany(s => s.CourseSections)
                    .HasForeignKey(cs => cs.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.Semester)
                    .WithMany(s => s.CourseSections)
                    .HasForeignKey(cs => cs.SemesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.Lecturer)
                    .WithMany(l => l.CourseSections)
                    .HasForeignKey(cs => cs.LecturerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(cs => cs.AdministrativeClass)
                    .WithMany(ac => ac.CourseSections)
                    .HasForeignKey(cs => cs.AdministrativeClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // CourseSectionSchedule
            modelBuilder.Entity<CourseSectionSchedule>(entity =>
            {
                entity.HasOne(css => css.CourseSection)
                    .WithMany(cs => cs.Schedules)
                    .HasForeignKey(css => css.CourseSectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CourseSectionStudent
            modelBuilder.Entity<CourseSectionStudent>(entity =>
            {
                entity.HasIndex(css => new { css.CourseSectionId, css.StudentId }).IsUnique();

                entity.HasOne(css => css.CourseSection)
                    .WithMany(cs => cs.Enrollments)
                    .HasForeignKey(css => css.CourseSectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(css => css.Student)
                    .WithMany(s => s.CourseSectionEnrollments)
                    .HasForeignKey(css => css.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StudentGrade
            modelBuilder.Entity<StudentGrade>(entity =>
            {
                entity.Property(g => g.Score).HasPrecision(4, 2);

                entity.HasIndex(g => new { g.CourseSectionId, g.StudentId, g.SubjectGradeComponentId }).IsUnique();

                entity.HasOne(g => g.CourseSection)
                    .WithMany(cs => cs.StudentGrades)
                    .HasForeignKey(g => g.CourseSectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(g => g.Student)
                    .WithMany(s => s.Grades)
                    .HasForeignKey(g => g.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.SubjectGradeComponent)
                    .WithMany(c => c.StudentGrades)
                    .HasForeignKey(g => g.SubjectGradeComponentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClassTransferRequest
            modelBuilder.Entity<ClassTransferRequest>(entity =>
            {
                entity.HasOne(ctr => ctr.Student)
                    .WithMany(s => s.TransferRequests)
                    .HasForeignKey(ctr => ctr.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ctr => ctr.CurrentClass)
                    .WithMany()
                    .HasForeignKey(ctr => ctr.CurrentAdministrativeClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ctr => ctr.TargetClass)
                    .WithMany()
                    .HasForeignKey(ctr => ctr.TargetAdministrativeClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ctr => ctr.Semester)
                    .WithMany(s => s.TransferRequests)
                    .HasForeignKey(ctr => ctr.SemesterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SpecializationSelection
            modelBuilder.Entity<SpecializationSelection>(entity =>
            {
                entity.HasOne(ss => ss.Student)
                    .WithMany(s => s.SpecializationSelections)
                    .HasForeignKey(ss => ss.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ss => ss.Specialization)
                    .WithMany()
                    .HasForeignKey(ss => ss.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ss => ss.Semester)
                    .WithMany()
                    .HasForeignKey(ss => ss.SemesterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LecturerSubject
            modelBuilder.Entity<LecturerSubject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.LecturerId, e.SubjectId }).IsUnique();

                entity.HasOne(e => e.Lecturer)
                    .WithMany(l => l.LecturerSubjects)
                    .HasForeignKey(e => e.LecturerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.LecturerSubjects)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Major)
                    .WithMany()
                    .HasForeignKey(e => e.MajorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Specialization)
                    .WithMany()
                    .HasForeignKey(e => e.SpecializationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

