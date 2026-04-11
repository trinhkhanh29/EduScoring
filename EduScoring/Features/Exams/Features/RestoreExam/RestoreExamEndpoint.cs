using System.Security.Claims;
using EduScoring.Common.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EduScoring.Features.Exams.Features.RestoreExam;

public static class RestoreExamEndpoint
{
    public static void MapRestoreExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exams/{id:int}/restore", async (int id, ClaimsPrincipal user, RestoreExamCommandHandler handler) =>
        {
            var tag = $"[RestoreExam | ExamId={id}]";

            // 1. Parse UserId từ token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine($"{tag} THẤT BẠI — Không parse được UserId từ token. Raw value: '{userIdString ?? "null"}'");
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);
            var role = isAdmin ? AppRoles.Admin : AppRoles.Teacher;
            Console.WriteLine($"{tag} Yêu cầu phục hồi — UserId: {userId} | Role: {role}");

            var result = await handler.Handle(new RestoreExamCommand(id, userId, isAdmin));

            if (!result.IsSuccess)
            {
                var reason = result.StatusCode switch
                {
                    404 => "Đề thi không tồn tại",
                    403 => "Không có quyền phục hồi",
                    400 => "Đề thi đang active",
                    500 => "Lỗi hệ thống",
                    _ => "Lỗi không xác định"
                };
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {reason}. Chi tiết: {result.ErrorMessage}");

                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                if (result.StatusCode == 500) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 500);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            // BUG FIX: bản gốc dùng result.ErrorMessage (luôn rỗng khi thành công)
            // → phải dùng result.SuccessMessage
            Console.WriteLine($"{tag} THÀNH CÔNG — UserId: {userId} ({role})");
            return Results.Ok(new { Message = result.SuccessMessage });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}