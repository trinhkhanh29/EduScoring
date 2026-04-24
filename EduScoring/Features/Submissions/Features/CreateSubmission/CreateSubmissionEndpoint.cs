using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EduScoring.Common.Authentication;

namespace EduScoring.Features.Submissions.Features.CreateSubmission;

public static class CreateSubmissionEndpoint
{
    public static void MapCreateSubmissionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions", async (
            [FromBody] CreateSubmissionRequest request,
            ClaimsPrincipal user,
            CreateSubmissionCommandHandler handler) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            string role = AppRoles.Student;
            if (user.IsInRole(AppRoles.Admin)) role = AppRoles.Admin;
            else if (user.IsInRole(AppRoles.Teacher)) role = AppRoles.Teacher;

            Guid? targetStudentId = request.TargetStudentId;
            string createdSource = "";

            if (role == AppRoles.Student)
            {
                targetStudentId = userId;
                createdSource = "StudentSelfSubmit";
            }
            else if (role == AppRoles.Teacher || role == AppRoles.Admin)
            {
                targetStudentId = request.TargetStudentId;
                createdSource = "TeacherUpload";
            }

            var command = new CreateSubmissionCommand(
                request.ExamId,
                targetStudentId,
                createdSource,
                userId,
                role
            );

            var result = await handler.Handle(command);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return Results.NotFound(new { Message = result.ErrorMessage });
                if (result.StatusCode == 403) return Results.Json(new { Message = result.ErrorMessage }, statusCode: 403);
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Created($"/api/submissions/{result.Data?.SubmissionId}", result.Data);
        })
        .RequireAuthorization()
        .WithTags("Submissions")
        .Produces<CreateSubmissionResponse>(201);
    }
}