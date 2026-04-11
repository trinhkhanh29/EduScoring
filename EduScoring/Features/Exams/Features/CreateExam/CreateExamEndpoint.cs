using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Exams.Features.CreateExam;

public static class CreateExamEndpoint
{
    public static void MapCreateExamEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exams", async (
            [FromBody] CreateExamRequest request,
            ClaimsPrincipal user,
            CreateExamCommandHandler handler) =>
        {
            // 1. Lấy TeacherId từ Token
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid teacherId))
            {
                return Results.Unauthorized();
            }

            // 2. Gộp Request và TeacherId thành Command
            var command = new CreateExamCommand(request.Title, request.Description, teacherId);

            // 3. Đẩy xuống Handler xử lý
            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(result.Data);
        })
        .RequireAuthorization() // Chỉ người có Token (đăng nhập) mới được tạo
        .WithTags("Exams");
    }
}