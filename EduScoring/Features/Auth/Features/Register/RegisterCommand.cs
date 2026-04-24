namespace EduScoring.Features.Auth.Features.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string Role
);

public record RegisterResponse(Guid UserId, string Message);