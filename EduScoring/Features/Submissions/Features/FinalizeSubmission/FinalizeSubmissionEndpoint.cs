using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Submissions.Features.FinalizeSubmission;

public static class FinalizeSubmissionEndpoint
{
    public static void MapFinalizeSubmissionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/{id:guid}/finalize", async (
            Guid id,
            ClaimsPrincipal user,
            FinalizeSubmissionCommandHandler handler) =>
        {
            var tag = $"[FinalizeSubmission | EntityId={id}]";

            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);

            var command = new FinalizeSubmissionCommand(id, userId, isAdmin);

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {result.ErrorMessage}");
                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(new { Message = "Bài thi đã được đóng băng." });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AppRoles.Admin},{AppRoles.Teacher}" })
        .WithTags("Submissions")
        .Produces(200);
    }
}