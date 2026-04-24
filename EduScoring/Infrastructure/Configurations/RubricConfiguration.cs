using EduScoring.Features.Exams.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class RubricConfiguration : IEntityTypeConfiguration<Rubric>
{
    public void Configure(EntityTypeBuilder<Rubric> builder)
    {
        builder.Property(x => x.MaxScore).HasPrecision(5, 2);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}