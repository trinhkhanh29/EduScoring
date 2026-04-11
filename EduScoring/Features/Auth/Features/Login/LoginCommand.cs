namespace EduScoring.Features.Auth.Features.Login;

public record LoginCommand(string Email, string Password);

public record LoginResponse(string Token, string Message);