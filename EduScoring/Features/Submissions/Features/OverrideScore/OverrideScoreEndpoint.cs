using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Submissions.Features.OverrideScore;

public static class OverrideScoreEndpoint
{
    public static void MapOverrideScoreEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/submissions/{id:guid}/override-score", async (
            Guid id,
            [FromBody] OverrideScoreRequest request,
            ClaimsPrincipal user,
            OverrideScoreCommandHandler handler) =>
        {
            var tag = $"[OverrideScore | EntityId={id}]";

            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Results.BadRequest(new { Message = "Phải cung cấp lý do ép điểm." });
            }

            var command = new OverrideScoreCommand(id, request.NewScore, request.Reason, userId, isAdmin);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {result.ErrorMessage}");
                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(new { Message = "Ép điểm thành công." });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Submissions")
        .Produces(200);
    }
}