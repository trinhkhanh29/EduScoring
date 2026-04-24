using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduScoring.Common.Authentication;

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
            // 1. Lấy UserId của người đang thao tác hệ thống (từ Token)
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Results.Unauthorized();
            }

            // Nếu user là Admin và có truyền TeacherId trong request thì dùng TeacherId đó (tạo hộ).
            // Nếu không, thì lấy chính Id của người đang dùng (currentUserId).
            bool isAdmin = user.IsInRole(AppRoles.Admin);
            Guid finalTeacherId = (isAdmin && request.TeacherId.HasValue) 
                ? request.TeacherId.Value 
                : currentUserId;

            // 2. Gộp Request và TeacherId thành Command
            var command = new CreateExamCommand(
                request.Title, 
                request.Description, 
                finalTeacherId,
                request.AllowStudentSubmission,
                request.RequireTeacherReview,
                request.AllowAppeal);

            // 3. Đẩy xuống Handler xử lý
            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(result.Data);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Teacher" }) // Chỉ Admin,Teacher mới được tạo
        .WithTags("Exams");
    }
}