using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EduScoring.Features.Submissions.Features.GetStudentResults;

public static class GetStudentResultsEndpoint
{
    public static void MapGetStudentResultsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/submissions/my-results",
            [Authorize(Roles = "Student")]
            async (ClaimsPrincipal user, GetStudentResultsQueryHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid studentId))
                {
                    return Results.Unauthorized();
                }
                var query = new GetStudentResultsQuery(studentId);
                var result = await handler.Handle(query);
                return Results.Ok(result);
            })
            .WithTags("Submissions")
            .Produces<StudentResultDto[]>(200);
    }
}
