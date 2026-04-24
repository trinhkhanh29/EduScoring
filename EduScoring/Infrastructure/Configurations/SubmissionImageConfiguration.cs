using EduScoring.Features.Submissions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class SubmissionImageConfiguration : IEntityTypeConfiguration<SubmissionImage>
{
    public void Configure(EntityTypeBuilder<SubmissionImage> builder)
    {
        builder.HasOne(x => x.Submission)
            .WithMany(s => s.Images)
            .HasForeignKey(x => x.SubmissionId)
            .IsRequired(false);
    }
}
