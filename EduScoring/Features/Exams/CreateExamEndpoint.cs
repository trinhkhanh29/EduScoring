using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Authorization; // BẮT BUỘC: Thêm thư viện này
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Exams;

// 1. Định nghĩa Request ngay tại đây bằng Record
public record CreateExamRequest(string Title, string Description);

public static class CreateExamEndpoint
{
    public static void MapCreateExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exams", async (
            CreateExamRequest request,
            AppDbContext db,
            ClaimsPrincipal user) => // Dùng ClaimsPrincipal trực tiếp thay vì HttpContext
        {
            try
            {
                // 1. Rút ID người đang thao tác (Có thể là Admin hoặc Teacher)
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Results.Unauthorized();
                }

                // 2. Mapping dữ liệu từ Request sang Entity
                // Dù là Admin hay Teacher tạo, thì ID của người đó sẽ được lưu làm chủ tọa (TeacherId)
                var newExam = new Exam
                {
                    Title = request.Title,
                    Description = request.Description,
                    TeacherId = userId,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // 3. Lưu vào Database
                db.Exams.Add(newExam);
                await db.SaveChangesAsync();

                // Ghi log thành công
                Console.WriteLine($"[THÀNH CÔNG] User {userId} (Role: {user.FindFirstValue(ClaimTypes.Role)}) đã tạo đề thi {newExam.Id}.");

                return Results.Ok(new
                {
                    Message = "Đã tạo đề thi thành công!",
                    ExamId = newExam.Id
                });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra Console
                Console.WriteLine($"[LỖI NGHIÊM TRỌNG - CreateExam] Chi tiết: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // Dò lỗi dễ dàng hơn

                return Results.Problem("Xảy ra lỗi hệ thống khi tạo đề thi. Vui lòng thử lại sau.");
            }
        })
        // 4. Phân quyền: Mở cửa cho cả Admin và Teacher
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}