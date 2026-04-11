using Microsoft.AspNetCore.Mvc;

namespace EduScoring.Features.Auth.Features.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async ([FromBody] LoginCommand request, LoginCommandHandler handler) =>
        {
            // Gọi Handler xử lý
            var result = await handler.Handle(request);

            // Tùy vào kết quả của Handler mà trả về Status Code tương ứng
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { Message = result.ErrorMessage });
            }

            return Results.Ok(result.Data);

        }).WithTags("Auth");
    }
}