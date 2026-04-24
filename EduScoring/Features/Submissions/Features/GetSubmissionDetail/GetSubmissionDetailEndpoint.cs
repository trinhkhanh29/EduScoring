using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Submissions.Features.GetSubmissionDetail;

public static class GetSubmissionDetailEndpoint
{
    public static void MapGetSubmissionDetailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/submissions/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            GetSubmissionDetailQueryHandler handler) =>
        {
            var tag = $"[GetSubmissionDetail | EntityId={id}]";

            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            bool isAdmin = user.IsInRole(AppRoles.Admin);
            string role = AppRoles.Student;
            if (isAdmin) role = AppRoles.Admin;
            else if (user.IsInRole(AppRoles.Teacher)) role = AppRoles.Teacher;

            var query = new GetSubmissionDetailQuery(id, userId, role, isAdmin);

            var result = await handler.Handle(query);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{tag} THẤT BẠI [{result.StatusCode}] — {result.ErrorMessage}");
                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(result.Data);
        })
        .RequireAuthorization() // mọi role đã đăng nhập
        .WithTags("Submissions")
        .Produces<SubmissionDetailDto>(200);
    }
}