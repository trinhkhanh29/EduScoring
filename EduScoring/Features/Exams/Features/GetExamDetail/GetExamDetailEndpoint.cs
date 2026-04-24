using System.Security.Claims;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Exams.Features.GetExamDetail;

public static class GetExamDetailEndpoint
{
    public static void MapGetExamDetailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exams/{id:int}", async (int id, ClaimsPrincipal user, GetExamDetailQueryHandler handler) =>
        {
            var tag = $"[GetExamDetail | EntityId={id}]";

            // 1. Parse UserId từ token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine($"{tag} THẤT BẠI — Không parse được UserId từ token. Raw value: '{userIdString ?? "null"}'");
                return Results.Unauthorized();
            }

            string role = AppRoles.Student;
            if (user.IsInRole(AppRoles.Admin)) role = AppRoles.Admin;
            else if (user.IsInRole(AppRoles.Teacher)) role = AppRoles.Teacher;

            Console.WriteLine($"{tag} Yêu cầu xem chi tiết — UserId: {userId} | Role: {role}");

            var result = await handler.Handle(new GetExamDetailQuery(id, userId, role));

            if (!result.IsSuccess)
            {
                var reason = result.StatusCode switch
                {
                    404 => "Đề thi không tồn tại",
                    403 => "Không có quyền xem đề thi này",
                    _ => "Lỗi không xác định"
                };
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {reason}. Chi tiết: {result.ErrorMessage}");

                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                return Results.Json(new { Message = result.ErrorMessage }, statusCode: result.StatusCode == 403 ? 403 : 400);
            }

            Console.WriteLine($"{tag} THÀNH CÔNG — Trả về chi tiết cho UserId: {userId} ({role})");
            return Results.Ok(result.Data);
        })
                        .RequireAuthorization()
                        .WithTags("Exams")
                        .Produces<ExamDetailDto>();
    }
}