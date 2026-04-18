using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class AppealConfiguration : IEntityTypeConfiguration<Appeal>
{
    public void Configure(EntityTypeBuilder<Appeal> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Appeals)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.StudentReason).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.TeacherResponse).HasMaxLength(1000);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Open");
        builder.Property(a => a.ResolutionType).HasMaxLength(50);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}