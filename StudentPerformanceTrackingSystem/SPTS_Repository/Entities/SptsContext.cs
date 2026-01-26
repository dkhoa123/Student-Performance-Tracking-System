using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SPTS_Repository.Entities;

public partial class SptsContext : DbContext
{
    public SptsContext()
    {
    }

    public SptsContext(DbContextOptions<SptsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<GpaScale> GpaScales { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeRule> GradeRules { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SectionSchedule> SectionSchedules { get; set; }

    public virtual DbSet<SectionStudent> SectionStudents { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Term> Terms { get; set; }

    public virtual DbSet<TermGpa> TermGpas { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DANGKHOA\\SQLEXPRESS;Initial Catalog=SPTS;Persist Security Info=True;User ID=sa;Password=khoa123;MultipleActiveResultSets=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.AcademicYearId).HasName("PK__Academic__11CFB9743377DA01");

            entity.HasIndex(e => e.IsCurrent, "IX_AcademicYears_Current").HasFilter("([is_current]=(1))");

            entity.HasIndex(e => new { e.StartYear, e.EndYear }, "IX_AcademicYears_Year");

            entity.HasIndex(e => e.YearName, "UQ__Academic__252258BE78C188E5").IsUnique();

            entity.Property(e => e.AcademicYearId).HasColumnName("academic_year_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EndYear).HasColumnName("end_year");
            entity.Property(e => e.IsCurrent).HasColumnName("is_current");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StartYear).HasColumnName("start_year");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.YearName)
                .HasMaxLength(20)
                .HasColumnName("year_name");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.Status, e.CreatedAt }, "IX_Alerts_Student");

            entity.Property(e => e.AlertId).HasColumnName("alert_id");
            entity.Property(e => e.ActualValue)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("actual_value");
            entity.Property(e => e.AlertType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("alert_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Severity)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("severity");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("NEW")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.ThresholdValue)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("threshold_value");

            entity.HasOne(d => d.Section).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_Alerts_Sections");

            entity.HasOne(d => d.Student).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alerts_Students");

            entity.HasOne(d => d.Term).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.TermId)
                .HasConstraintName("FK_Alerts_Terms");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.CourseCode, "UQ_Courses_Code").IsUnique();

            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("course_code");
            entity.Property(e => e.CourseName)
                .HasMaxLength(200)
                .HasColumnName("course_name");
            entity.Property(e => e.Credits).HasColumnName("credits");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C22324229968D3BF");

            entity.HasIndex(e => e.HeadTeacherId, "IX_Departments_HeadTeacher");

            entity.HasIndex(e => e.Status, "IX_Departments_Status");

            entity.HasIndex(e => e.DepartmentCode, "UQ__Departme__EBC3495E4D14A0FE").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("department_code");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(200)
                .HasColumnName("department_name");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HeadTeacherId).HasColumnName("head_teacher_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            entity.HasOne(d => d.HeadTeacher).WithMany(p => p.Departments)
                .HasForeignKey(d => d.HeadTeacherId)
                .HasConstraintName("FK_Departments_Teachers");
        });

        modelBuilder.Entity<GpaScale>(entity =>
        {
            entity.HasKey(e => e.ScaleId);

            entity.Property(e => e.ScaleId).HasColumnName("scale_id");
            entity.Property(e => e.GpaPoint)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("gpa_point");
            entity.Property(e => e.Letter)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("letter");
            entity.Property(e => e.MaxScore)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("max_score");
            entity.Property(e => e.MinScore)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("min_score");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.SectionId }, "IX_Grades_Student");

            entity.HasIndex(e => new { e.SectionId, e.StudentId }, "UQ_Grades_SectionStudent").IsUnique();

            entity.Property(e => e.GradeId).HasColumnName("grade_id");
            entity.Property(e => e.FinalScore)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("final_score");
            entity.Property(e => e.GpaPoint)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("gpa_point");
            entity.Property(e => e.ProcessScore)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("process_score");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalScore)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("total_score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Section).WithMany(p => p.Grades)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_Sections");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_Students");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Grades)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Grades_Teachers");

            entity.HasOne(d => d.SectionStudent).WithOne(p => p.Grade)
                .HasForeignKey<Grade>(d => new { d.SectionId, d.StudentId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_SectionStudents");
        });

        modelBuilder.Entity<GradeRule>(entity =>
        {
            entity.HasKey(e => e.RuleId);

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.FinalWeight)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("final_weight");
            entity.Property(e => e.ProcessWeight)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("process_weight");
            entity.Property(e => e.RoundingScale)
                .HasDefaultValue(1)
                .HasColumnName("rounding_scale");

            entity.HasOne(d => d.Course).WithMany(p => p.GradeRules)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GradeRules_Courses");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Content)
                .HasMaxLength(2000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.RelatedAlertId).HasColumnName("related_alert_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.RelatedAlert).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedAlertId)
                .HasConstraintName("FK_Notifications_Alerts");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.Property(e => e.ReminderId).HasColumnName("reminder_id");
            entity.Property(e => e.AdvisorId).HasColumnName("advisor_id");
            entity.Property(e => e.AlertId).HasColumnName("alert_id");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("sent_at");

            entity.HasOne(d => d.Alert).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.AlertId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reminders_Alerts");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasIndex(e => new { e.TeacherId, e.TermId }, "IX_Sections_Teacher");

            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SectionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("section_code");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("OPEN")
                .HasColumnName("status");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.TermId).HasColumnName("term_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Sections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sections_Courses");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Sections)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sections_Teachers");

            entity.HasOne(d => d.Term).WithMany(p => p.Sections)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sections_Terms");
        });

        modelBuilder.Entity<SectionSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__SectionS__C46A8A6FE9074464");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("day_of_week");
            entity.Property(e => e.EndPeriod).HasColumnName("end_period");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Room)
                .HasMaxLength(50)
                .HasColumnName("room");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StartPeriod).HasColumnName("start_period");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionSchedules)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SectionSchedules_Sections");
        });

        modelBuilder.Entity<SectionStudent>(entity =>
        {
            entity.HasKey(e => new { e.SectionId, e.StudentId });

            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("added_at");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.Source)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("EXCEL")
                .HasColumnName("source");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            entity.HasOne(d => d.AddedByNavigation).WithMany(p => p.SectionStudents)
                .HasForeignKey(d => d.AddedBy)
                .HasConstraintName("FK_SS_Users");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionStudents)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SS_Sections");

            entity.HasOne(d => d.Student).WithMany(p => p.SectionStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SS_Students");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_Students_Department");

            entity.HasIndex(e => e.StudentCode, "UQ_Students_Code").IsUnique();

            entity.Property(e => e.StudentId)
                .ValueGeneratedNever()
                .HasColumnName("student_id");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CohortYear).HasColumnName("cohort_year");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Major)
                .HasMaxLength(100)
                .HasColumnName("major");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.StudentCode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("student_code");

            entity.HasOne(d => d.Department).WithMany(p => p.Students)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Students_Departments");

            entity.HasOne(d => d.StudentNavigation).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(e => e.TeacherCode, "UQ_Teachers_Code").IsUnique();

            entity.Property(e => e.TeacherId)
                .ValueGeneratedNever()
                .HasColumnName("teacher_id");
            entity.Property(e => e.Degree).HasMaxLength(20);
            entity.Property(e => e.DemparmentName)
                .HasMaxLength(255)
                .HasColumnName("demparment_name");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.TeacherCode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("teacher_code");

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teachers_Users");
        });

        modelBuilder.Entity<Term>(entity =>
        {
            entity.HasIndex(e => e.AcademicYearId, "IX_Terms_AcademicYear");

            entity.HasIndex(e => e.TermName, "UQ_Terms_Name").IsUnique();

            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.AcademicYearId).HasColumnName("academic_year_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TermName)
                .HasMaxLength(50)
                .HasColumnName("term_name");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Terms)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK_Terms_AcademicYears");
        });

        modelBuilder.Entity<TermGpa>(entity =>
        {
            entity.HasKey(e => new { e.TermId, e.StudentId });

            entity.ToTable("TermGpa");

            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.CreditsAttempted).HasColumnName("credits_attempted");
            entity.Property(e => e.CreditsEarned).HasColumnName("credits_earned");
            entity.Property(e => e.GpaValue)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("gpa_value");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Student).WithMany(p => p.TermGpas)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TermGpa_Students");

            entity.HasOne(d => d.Term).WithMany(p => p.TermGpas)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TermGpa_Terms");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
