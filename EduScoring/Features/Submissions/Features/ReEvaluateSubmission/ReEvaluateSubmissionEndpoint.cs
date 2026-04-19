using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EduScoring.Features.Submissions.Features.ReEvaluateSubmission;

public static class ReEvaluateSubmissionEndpoint
{
    public static void MapReEvaluateSubmissionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/{id:guid}/re-evaluate",
            [Authorize(Roles = "Admin,Teacher")]
            async (Guid id, ClaimsPrincipal user, ReEvaluateSubmissionCommandHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Results.Unauthorized();
                }
                bool isAdmin = user.IsInRole("Admin");
                var command = new ReEvaluateSubmissionCommand(id, userId, isAdmin);
                var result = await handler.Handle(command);
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                    if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                    return Results.BadRequest(new { Message = result.ErrorMessage });
                }
                return Results.Accepted($"/api/submissions/{id}", new { Message = result.ErrorMessage });
            })
            .WithTags("Submissions")
            .Produces(202);
    }
}
