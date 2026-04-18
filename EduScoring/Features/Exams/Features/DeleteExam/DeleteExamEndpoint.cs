using System.Security.Claims;
using EduScoring.Common.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace EduScoring.Features.Exams.Features.DeleteExam;

public static class DeleteExamEndpoint
{
    public static void MapDeleteExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/exams/{id:int}", async (int id, ClaimsPrincipal user, DeleteExamCommandHandler handler) =>
        {
            var tag = $"[DeleteExam | EntityId={id}]";

            // 1. Parse UserId từ token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine($"{tag} THẤT BẠI — Không parse được UserId từ token. Raw value: '{userIdString ?? "null"}'");
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);
            var role = isAdmin ? AppRoles.Admin : AppRoles.Teacher;
            Console.WriteLine($"{tag} Yêu cầu xóa — UserId: {userId} | Role: {role}");

            // 2. Xử lý command
            var result = await handler.Handle(new DeleteExamCommand(id, userId, isAdmin));

            if (!result.IsSuccess)
            {
                var reason = result.StatusCode switch
                {
                    404 => "Đề thi không tồn tại",
                    403 => "Không có quyền xóa đề thi này",
                    _ => "Lỗi không xác định"
                };
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {reason}. Chi tiết: {result.ErrorMessage}");

                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            Console.WriteLine($"{tag} THÀNH CÔNG — ExamId {id} đã bị xóa bởi UserId: {userId} ({role})");
            return Results.Ok(new { Message = "Đã xóa đề thi thành công!" });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}