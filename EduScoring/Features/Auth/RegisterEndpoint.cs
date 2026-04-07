using EduScoring.Common.Authentication;
using EduScoring.Data;
using EduScoring.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Auth;

// Request DTO nhận từ Postman
public record RegisterRequest(string Username, string Email, string Password, string FullName, string RoleName);

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, AppDbContext db) =>
        {
            // 1. Kiểm tra xem Email hoặc Username đã bị ai lấy chưa
            var exists = await db.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username);
            if (exists)
            {
                return Results.BadRequest(new { Message = "Email hoặc Username đã tồn tại!" });
            }

            // 2. Tìm ID của Quyền dựa trên tên (Admin, Teacher, Student)
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
            if (role == null)
            {
                return Results.BadRequest(new { Message = $"Quyền '{request.RoleName}' không hợp lệ. Vui lòng nhập: Admin, Teacher hoặc Student." });
            }

            // 3. Tạo User và BĂM MẬT KHẨU (Cực kỳ quan trọng)
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Băm mật khẩu ra chuỗi loằng ngoằng
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // 4. Nối User với Role (Bảng trung gian UserRoles)
            newUser.UserRoles.Add(new UserRole
            {
                Role = role
            });

            // 5. Lưu vào CSDL
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = $"Đăng ký tài khoản {request.RoleName} thành công!",
                UserId = newUser.Id
            });
        }).WithTags("Auth");
    }
}