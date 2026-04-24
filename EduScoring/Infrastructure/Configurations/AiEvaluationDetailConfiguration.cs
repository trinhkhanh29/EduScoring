using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class AiEvaluationDetailConfiguration : IEntityTypeConfiguration<AiEvaluationDetail>
{
    public void Configure(EntityTypeBuilder<AiEvaluationDetail> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Score).HasPrecision(18, 2);
        builder.Property(d => d.MaxScore).HasPrecision(18, 2);
        builder.Property(d => d.Weight).HasPrecision(18, 2);
        builder.Property(d => d.CriteriaKey).HasMaxLength(100);
        builder.Property(d => d.CriteriaName).HasMaxLength(200);
        builder.Property(d => d.CriteriaGroup).HasMaxLength(100);

        builder.HasOne(d => d.AiEvaluation)
            .WithMany(a => a.Details)
            .HasForeignKey(d => d.AiEvaluationId);

        builder.HasOne(d => d.Rubric)
            .WithMany()
            .HasForeignKey(d => d.RubricId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}