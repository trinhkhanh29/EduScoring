using Microsoft.EntityFrameworkCore;
using EduScoring.Common.Authentication;
using System.Security.Claims;
using EduScoring.Infrastructure;

namespace EduScoring.Features.Exams;

public static class GetExamDetailEndpoint
{
    public static void MapGetExamDetailEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exams")
                       .WithTags("Exams")
                       .RequireAuthorization();

        // 1. Lấy chi tiết đề thi (Tự động lọc IsDeleted = true nhờ Global Filter)
        group.MapGet("/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var exam = await db.Exams
                .Include(e => e.Teacher)
                .Include(e => e.Rubrics)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return Results.NotFound(new { message = "Đề thi không tồn tại hoặc đã bị xóa tạm thời." });
            }

            // Phân quyền
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAuthorized = user.IsInRole(AppRoles.Admin) ||
                               (user.IsInRole(AppRoles.Teacher) && exam.TeacherId.ToString() == userId);

            if (!isAuthorized)
            {
                return Results.Forbid();
            }

            return Results.Ok(new
            {
                exam.Id,
                exam.Title,
                exam.Description,
                TeacherName = exam.Teacher?.FullName,
                exam.CreatedAt,
                RubricsCount = exam.Rubrics.Count
            });
        });

        // 2. Phục hồi đề thi đã xóa (Sử dụng IgnoreQueryFilters để tìm bản ghi IsDeleted = true)
        group.MapPost("/{id:int}/restore", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            // Chỉ Admin hoặc Teacher sở hữu mới được phục hồi
            var exam = await db.Exams
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return Results.NotFound(new { message = "Không tìm thấy đề thi này trong hệ thống." });
            }

            if (!exam.IsDeleted)
            {
                return Results.BadRequest(new { message = "Đề thi này hiện đang hoạt động, không cần phục hồi." });
            }

            // Kiểm tra quyền (Admin hoặc chính chủ)
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!user.IsInRole(AppRoles.Admin) && exam.TeacherId.ToString() != userId)
            {
                return Results.Forbid();
            }

            // Đảo ngược trạng thái
            exam.IsDeleted = false;
            exam.DeletedAt = null;
            exam.DeletedBy = null;

            await db.SaveChangesAsync();

            return Results.Ok(new { message = $"Đã phục hồi đề thi '{exam.Title}' thành công!" });
        });
    }
}