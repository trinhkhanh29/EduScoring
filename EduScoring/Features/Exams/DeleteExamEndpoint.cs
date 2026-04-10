using System.Security.Claims;
using EduScoring.Common.Authentication;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Features.Exams;

public static class DeleteExamEndpoint
{
    public static void MapDeleteExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/exams/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            try
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Results.Unauthorized();

                var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == id);
                if (exam is null)
                    return Results.NotFound(new { Message = "Không tìm thấy đề thi để xóa!" });

                // Kiểm tra quyền
                bool isAuthorized = user.IsInRole(AppRoles.Admin) ||
                                   (user.IsInRole(AppRoles.Teacher) && exam.TeacherId == userId);

                if (!isAuthorized)
                {
                    var msg = $"Tài khoản {userId} không phải Admin và cũng không sở hữu đề thi {id}. Không được phép xóa!";
                    Console.WriteLine($"[CẢNH BÁO PHÂN QUYỀN] {msg}");

                    return Results.Json(new { Message = msg }, statusCode: 403);
                }

                db.Exams.Remove(exam);
                await db.SaveChangesAsync();

                Console.WriteLine($"[THÀNH CÔNG] User {userId} đã xóa đề thi {id}.");
                return Results.Ok(new { Message = "Đã xóa đề thi thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[LỖI DB - DeleteExam] Khóa ngoại: {dbEx.Message}");
                return Results.BadRequest(new { Message = "Không thể xóa đề này vì đã có dữ liệu sinh viên nộp bài liên quan!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI - DeleteExam] {ex.Message}");
                return Results.Problem("Lỗi hệ thống khi xóa đề thi.");
            }
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}