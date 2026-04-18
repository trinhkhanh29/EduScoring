using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Submissions.Features.CreateAppeal;

public static class CreateAppealEndpoint
{
    public static void MapCreateAppealEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/{id:guid}/appeals",
            [Authorize(Roles = "Student")]
            async (Guid id, [FromBody] CreateAppealRequest request, ClaimsPrincipal user, CreateAppealCommandHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Results.Unauthorized();
                }

                var command = new CreateAppealCommand(id, request.Reason, userId);
                var result = await handler.Handle(command);

                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                    return Results.BadRequest(new { Message = result.ErrorMessage });
                }

                return Results.Ok(new { Message = "Đã gửi đơn phúc khảo thành công." });
            })
            .WithTags("Submissions")
            .Produces(200);
    }
}

public record CreateAppealRequest(string Reason);
