using EduScoring.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduScoring.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role { Id = 1, Name = EduScoring.Common.Authentication.AppRoles.Admin, Description = "Quản trị viên hệ thống" },
            new Role { Id = 2, Name = EduScoring.Common.Authentication.AppRoles.Teacher, Description = "Giảng viên (Tạo đề, xem điểm)" },
            new Role { Id = 3, Name = EduScoring.Common.Authentication.AppRoles.Student, Description = "Sinh viên (Nộp bài)" }
        );
    }
}