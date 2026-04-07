using EduScoring.Common.Authentication;
using EduScoring.Data;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Auth;

// Request & Response DTOs
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Message);

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        // Chú ý: Cần inject thêm IJwtProvider để dùng hàm GenerateToken
        app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, IJwtProvider jwtProvider) =>
        {
            // 1. Tìm User theo Email (BẮT BUỘC Include UserRoles và Role để lấy tên Quyền)
            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Nếu không thấy User (Bảo mật: Báo lỗi chung chung, không nói rõ sai email hay pass)
            if (user == null)
            {
                return Results.BadRequest(new { Message = "Email hoặc mật khẩu không chính xác!" });
            }

            // 3. Dùng BCrypt để so sánh mật khẩu người nhập với mật khẩu băm trong DB
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Results.BadRequest(new { Message = "Email hoặc mật khẩu không chính xác!" });
            }

            // 4. Nếu chuẩn hết, gọi JwtProvider để "đúc" Token
            var token = jwtProvider.GenerateToken(user);

            // 5. Trả Token về cho Frontend
            return Results.Ok(new LoginResponse(token, "Đăng nhập thành công!"));

        }).WithTags("Auth");
    }
}