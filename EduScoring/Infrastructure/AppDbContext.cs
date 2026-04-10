using System.Linq.Expressions;
using EduScoring.Data;
using EduScoring.Data.Entities;
using EduScoring.Features.Auth.Models;
using EduScoring.Features.Exams.Models;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Infrastructure;

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
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<TestEntry> TestEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // 1. CHỈ GIỮ LẠI BỘ LỌC SOFT DELETE
        // ==========================================
        var baseEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
            .Select(e => e.ClrType);

        foreach (var clrType in baseEntityTypes)
        {
            var parameter = Expression.Parameter(clrType, "e");
            var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(null, propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
            var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(compareExpression, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }

        // ==========================================
        // 2. GIỮ NGUYÊN 100% MAPPING BẢN CŨ TRÁNH LỖI DB
        // ==========================================

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

        modelBuilder.Entity<Submission>()
            .HasIndex(x => new { x.ExamId, x.StudentId })
            .IsUnique();

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

        modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = EduScoring.Common.Authentication.AppRoles.Admin, Description = "Quản trị viên hệ thống" },
                new Role { Id = 2, Name = EduScoring.Common.Authentication.AppRoles.Teacher, Description = "Giảng viên (Tạo đề, xem điểm)" },
                new Role { Id = 3, Name = EduScoring.Common.Authentication.AppRoles.Student, Description = "Sinh viên (Nộp bài)" }
        );

        modelBuilder.Entity<ActivityLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    // ==========================================
    // 3. OVERRIDE CHO SOFT DELETE AUTO
    // ==========================================
    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleSoftDelete()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity is BaseEntity);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;

            var entity = (BaseEntity)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;

            Console.WriteLine($"[SOFT DELETE AUTO] Đã ẩn thực thể {entry.Entity.GetType().Name}");
        }
    }
}