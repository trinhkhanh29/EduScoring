using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class AiEvaluationConfiguration : IEntityTypeConfiguration<AiEvaluation>
{
    public void Configure(EntityTypeBuilder<AiEvaluation> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TotalScore).HasPrecision(18, 2);
        builder.Property(a => a.ConfidenceScore).HasPrecision(18, 2);
        builder.Property(a => a.Status).HasMaxLength(50).HasDefaultValue("Pending");
        builder.Property(a => a.ModelName).HasMaxLength(100);
        builder.Property(a => a.PromptVersion).HasMaxLength(50);
        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Evaluations)
            .HasForeignKey(a => a.SubmissionId);
        builder.HasOne(a => a.Rubric)
            .WithMany()
            .HasForeignKey(a => a.RubricId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(a => a.Details)
            .WithOne(d => d.AiEvaluation);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Evaluations)
            .HasForeignKey(a => a.SubmissionId)
            .IsRequired(false);
    }
}
