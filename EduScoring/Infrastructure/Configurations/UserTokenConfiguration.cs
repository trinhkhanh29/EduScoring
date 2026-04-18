using EduScoring.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.LoginProvider, x.Name }).IsUnique();

        // UserToken không kế thừa BaseEntity nên phải filter qua navigation
        builder.HasQueryFilter(ut => !ut.User.IsDeleted);
    }
}
