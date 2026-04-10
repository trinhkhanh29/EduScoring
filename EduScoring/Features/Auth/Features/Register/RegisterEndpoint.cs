using EduScoring.Common.Authentication;
using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EduScoring.Features.Auth.Features.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async ([FromBody] RegisterCommand request, RegisterCommandHandler handler) =>
        {
            var result = await handler.Handle(request);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(result.Data);

        }).WithTags("Auth");
    }
}