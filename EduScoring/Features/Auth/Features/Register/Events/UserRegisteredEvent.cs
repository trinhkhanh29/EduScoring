using MediatR;

namespace EduScoring.Features.Auth.Features.Register;

public record UserRegisteredEvent(Guid UserId, string Email, string FullName) : INotification;