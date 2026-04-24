using EduScoring.Data;
using EduScoring.Data.Entities;
using EduScoring.Features.Auth.Models;
using EduScoring.Features.Exams.Models;
using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionImage> SubmissionImages { get; set; }
    public DbSet<AiEvaluation> AiEvaluations { get; set; }
    public DbSet<AiEvaluationDetail> AiEvaluationDetails { get; set; }
    public DbSet<HumanEvaluation> HumanEvaluations { get; set; }
    public DbSet<Appeal> Appeals { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<TestEntry> TestEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); //moi entity co 1 file config rieng, ko can cau hinh trong onmodelcreating nua
    }

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
            Console.WriteLine($"[SOFT DELETE] {entry.Entity.GetType().Name} đã bị ẩn.");
        }
    }
}