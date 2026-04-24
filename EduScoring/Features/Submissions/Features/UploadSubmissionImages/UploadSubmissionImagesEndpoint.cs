using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Submissions.Features.UploadSubmissionImages;

public static class UploadSubmissionImagesEndpoint
{
    public static void MapUploadSubmissionImagesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/{id:guid}/images", async (
            Guid id,
            [FromBody] UploadSubmissionImagesRequest request,
            ClaimsPrincipal user,
            UploadSubmissionImagesCommandHandler handler) =>
        {
            var tag = $"[UploadSubmissionImages | EntityId={id}]";

            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);

            var command = new UploadSubmissionImagesCommand(
                id,
                request.ImageUrls,
                userId,
                isAdmin
            );

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {result.ErrorMessage}");
                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(new UploadSubmissionImagesResponse(result.Data));
        })
        .RequireAuthorization()
        .WithTags("Submissions")
        .Produces<UploadSubmissionImagesResponse>(200);
    }
}