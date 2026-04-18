using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Submissions.Features.ResolveAppeal;

public static class ResolveAppealEndpoint
{
    public static void MapResolveAppealEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/appeals/{appealId:int}/resolve",
            [Authorize(Roles = "Admin,Teacher")]
            async (int appealId, [FromBody] ResolveAppealRequest request, ClaimsPrincipal user, ResolveAppealCommandHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Results.Unauthorized();
                }
                bool isAdmin = user.IsInRole("Admin");
                var command = new ResolveAppealCommand(
                    appealId,
                    request.IsAccepted,
                    request.NewScore,
                    request.TeacherFeedback,
                    userId,
                    isAdmin
                );
                var result = await handler.Handle(command);
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                    if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                    return Results.BadRequest(new { Message = result.ErrorMessage });
                }
                return Results.Ok(new { Message = result.ErrorMessage });
            })
            .WithTags("Appeals")
            .Produces(200);
    }
}

public record ResolveAppealRequest(bool IsAccepted, decimal? NewScore, string TeacherFeedback);
