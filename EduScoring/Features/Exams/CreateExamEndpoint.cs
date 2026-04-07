using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using EduScoring.Common.Authentication;
using EduScoring.Data;
using EduScoring.Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EduScoring.Features.Exams;

// 1. Định nghĩa Request ngay tại đây bằng Record (Cực kỳ gọn nhẹ, thay cho DTO)
public record CreateExamRequest(string Title, string Description);

public static class CreateExamEndpoint
{
    public static void MapCreateExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exams",
            [Authorize(Roles = AppRoles.Teacher)] // <--- Biển báo phân quyền
        async (CreateExamRequest request, AppDbContext db, HttpContext httpContext) =>
            {
                // 2. Lấy TeacherId trực tiếp từ JWT Token đang được gửi lên
                var teacherIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (!Guid.TryParse(teacherIdString, out Guid teacherId))
                {
                    return Results.Unauthorized();
                }

                // 3. Mapping dữ liệu từ Request sang Entity
                var newExam = new Exam
                {
                    Title = request.Title,
                    Description = request.Description,
                    TeacherId = teacherId,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // 4. Lưu vào Database
                db.Exams.Add(newExam);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "Đã tạo đề thi thành công!",
                    ExamId = newExam.Id
                });
            })
            .WithTags("Exams"); // Gom nhóm gọn gàng trên Swagger
    }
}