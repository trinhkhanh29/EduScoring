using EduScoring.Common.Authentication;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Users;

// Định nghĩa DTO phản hồi ngay tại đây: KHÔNG BAO GIỜ trả về PasswordHash
public record UserResponse(Guid Id, string Username, string Email, string FullName, bool IsActive, DateTimeOffset CreatedAt);

public static class GetUsersEndpoint
{
    public static void MapGetUsersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users",
            [Authorize(Roles = AppRoles.Admin)] // <--- Biển báo: Chỉ Admin mới được vào
        async (AppDbContext db) =>
            {
                // Lấy danh sách từ DB và map sang DTO an toàn
                var users = await db.Users
                    .AsNoTracking() // Tối ưu hiệu năng: Dùng khi chỉ đọc dữ liệu, không sửa
                    .Select(u => new UserResponse(
                        u.Id,
                        u.Username,
                        u.Email,
                        u.FullName,
                        u.IsActive,
                        u.CreatedAt
                    ))
                    .ToListAsync();

                return Results.Ok(users);
            })
            .WithTags("Users"); // Gom nhóm gọn gàng trên Swagger
    }
}