using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FinalScore).HasPrecision(18, 2);
        builder.Property(s => s.LatestAiScore).HasPrecision(18, 2);
        builder.Property(s => s.HumanScore).HasPrecision(18, 2);
        builder.HasIndex(s => new { s.ExamId, s.StudentId }).IsUnique();

        builder.HasOne(s => s.Exam)
            .WithMany()
            .HasForeignKey(s => s.ExamId);

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}