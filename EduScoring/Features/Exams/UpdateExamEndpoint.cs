using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams;

public record UpdateExamRequest(string Title, string Description);

public static class UpdateExamEndpoint
{
    public static void MapUpdateExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/exams/{id:int}", async (int id, UpdateExamRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            try
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Results.Unauthorized();

                var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == id);
                if (exam is null)
                    return Results.NotFound(new { Message = "Không tìm thấy đề thi để cập nhật!" });

                // Kiểm tra quyền
                bool isAuthorized = user.IsInRole(AppRoles.Admin) ||
                                   (user.IsInRole(AppRoles.Teacher) && exam.TeacherId == userId);

                if (!isAuthorized)
                {
                    var msg = $"Tài khoản {userId} không có quyền sửa đề thi của giảng viên khác!";
                    Console.WriteLine($"[CẢNH BÁO PHÂN QUYỀN] {msg} (Target Exam ID: {id})");

                    // Trả về JSON kèm Message chi tiết và status code 403
                    return Results.Json(new { Message = msg }, statusCode: 403);
                }

                exam.Title = request.Title;
                exam.Description = request.Description;

                await db.SaveChangesAsync();

                Console.WriteLine($"[THÀNH CÔNG] User {userId} đã sửa đề thi {id}.");
                return Results.Ok(new { Message = "Cập nhật đề thi thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI - UpdateExam] {ex.Message}");
                return Results.Problem("Lỗi hệ thống khi sửa đề thi.");
            }
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}