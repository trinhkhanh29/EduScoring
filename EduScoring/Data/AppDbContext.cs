using EduScoring.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ===== DbSets =====
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionImage> SubmissionImages { get; set; }
    public DbSet<AiEvaluation> AiEvaluations { get; set; }
    public DbSet<Appeal> Appeals { get; set; }

    // (optional test)
    public DbSet<TestEntry> TestEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== UserRole (Many-to-Many) =====
        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Unique Token =====
        modelBuilder.Entity<UserToken>()
            .HasIndex(x => new { x.UserId, x.LoginProvider, x.Name })
            .IsUnique();

        // ===== Decimal precision (PostgreSQL NUMERIC) =====
        modelBuilder.Entity<Rubric>()
            .Property(x => x.MaxScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Submission>()
            .Property(x => x.TotalScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<AiEvaluation>()
            .Property(x => x.AwardedScore)
            .HasPrecision(5, 2);

        // ===== Relationships =====

        modelBuilder.Entity<Exam>()
            .HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Submission>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Submission>()
            .HasOne(x => x.Exam)
            .WithMany()
            .HasForeignKey(x => x.ExamId);

        modelBuilder.Entity<SubmissionImage>()
            .HasOne(x => x.Submission)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.SubmissionId);

        modelBuilder.Entity<AiEvaluation>()
            .HasOne(x => x.Submission)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.SubmissionId);

        modelBuilder.Entity<AiEvaluation>()
            .HasOne(x => x.Rubric)
            .WithMany()
            .HasForeignKey(x => x.RubricId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appeal>()
            .HasOne(x => x.Submission)
            .WithMany()
            .HasForeignKey(x => x.SubmissionId);
    }
}