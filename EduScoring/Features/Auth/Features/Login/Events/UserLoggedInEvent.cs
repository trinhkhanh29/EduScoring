using MediatR;

namespace EduScoring.Features.Auth.Features.Login;

public record UserLoggedInEvent(Guid UserId, string Email, DateTime LoginTime) : INotification;