using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EduScoring.Features.Submissions.Features.CompareAiVsHuman;

public static class CompareAiVsHumanEndpoint
{
    public static void MapCompareAiVsHumanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exams/{examId:int}/statistics",
            [Authorize(Roles = "Admin,Teacher")]
            async (int examId, ClaimsPrincipal user, CompareAiVsHumanQueryHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Results.Unauthorized();
                }
                bool isAdmin = user.IsInRole("Admin");
                var query = new CompareAiVsHumanQuery(examId, userId, isAdmin);
                var result = await handler.Handle(query);
                if (!result.IsSuccess)
                {
                    if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                    if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                    return Results.BadRequest(new { Message = result.ErrorMessage });
                }
                return Results.Ok(result.Data);
            })
            .WithTags("Statistics")
            .Produces<CompareResultDto>(200);
    }
}
