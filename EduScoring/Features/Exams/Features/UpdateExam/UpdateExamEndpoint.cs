using System.Security.Claims;
using EduScoring.Common.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Exams.Features.UpdateExam;

public static class UpdateExamEndpoint
{
    public static void MapUpdateExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/exams/{id:int}", async (int id, [FromBody] UpdateExamRequest request, ClaimsPrincipal user, UpdateExamCommandHandler handler) =>
        {
            var tag = $"[UpdateExam | EntityId={id}]";

            // 1. Parse UserId từ token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine($"{tag} THẤT BẠI — Không parse được UserId từ token. Raw value: '{userIdString ?? "null"}'");
                return Results.Unauthorized();
            }

            // 2. Validate request body
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                Console.WriteLine($"{tag} THẤT BẠI — Title rỗng. UserId: {userId}");
                return Results.BadRequest(new { Message = "Title không được để trống!" });
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);
            var role = isAdmin ? AppRoles.Admin : AppRoles.Teacher;
            Console.WriteLine($"{tag} Yêu cầu cập nhật — UserId: {userId} | Role: {role} | Title mới: '{request.Title}'");

            var command = new UpdateExamCommand(
                id, 
                request.Title, 
                request.Description, 
                request.TeacherId,
                userId, 
                isAdmin,
                request.AllowStudentSubmission,
                request.RequireTeacherReview,
                request.AllowAppeal);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                var reason = result.StatusCode switch
                {
                    404 => "Đề thi không tồn tại",
                    403 => "Không có quyền sửa đề thi này",
                    500 => "Lỗi máy chủ",
                    _ => "Lỗi không xác định"
                };
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {reason}. Chi tiết: {result.ErrorMessage}");

                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                if (result.StatusCode == 500) return Results.Problem(title: "Internal Error", detail: result.ErrorMessage, statusCode: 500);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            Console.WriteLine($"{tag} THÀNH CÔNG — Đã cập nhật đề thi bởi UserId: {userId} ({role})");
            return Results.Ok(new { Message = "Cập nhật đề thi thành công!" });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Exams");
    }
}