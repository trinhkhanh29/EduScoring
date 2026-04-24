using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Auth.Features.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register",
            [Authorize(Roles = "Admin")]
        async ([FromBody] RegisterCommand request, ClaimsPrincipal user, RegisterCommandHandler handler) =>
            {
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid adminUserId))
                {
                    return Results.Unauthorized();
                }

                var result = await handler.Handle(request, adminUserId);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(new { Message = result.ErrorMessage });
                }

                return Results.Ok(result.Data);

            }).WithTags("Auth");
    }
}