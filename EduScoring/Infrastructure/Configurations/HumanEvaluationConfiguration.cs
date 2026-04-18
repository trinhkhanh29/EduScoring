using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class HumanEvaluationConfiguration : IEntityTypeConfiguration<HumanEvaluation>
{
    public void Configure(EntityTypeBuilder<HumanEvaluation> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.TeacherScore).HasPrecision(18, 2);
        builder.Property(h => h.TeacherFeedback).HasMaxLength(1000);

        builder.HasOne(h => h.Submission)
            .WithMany(s => s.HumanEvaluations)
            .HasForeignKey(h => h.SubmissionId);

        builder.HasOne(h => h.Submission)
            .WithMany(s => s.HumanEvaluations)
            .HasForeignKey(h => h.SubmissionId)
            .IsRequired(false);

        // Filter trên chính entity — không qua navigation tránh conflict
    }
}