namespace EduScoring.Features.Auth.Features.Register;

public record RegisterCommand(string Username, string Email, string Password, string FullName, string RoleName);

public record RegisterResponse(Guid UserId, string Message);