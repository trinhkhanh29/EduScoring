using EduScoring.Features.Auth.Features.Register;
using MediatR;

namespace EduScoring.Features.Auth.Features.Register.EventHandlers;

public class SendWelcomeEmailHandler : INotificationHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // SAU NÀY BỎ COMMENT CHỖ NÀY ĐỂ CODE CHẠY THẬT
        //var emailBody = $"Chào mừng {notification.FullName} gia nhập hệ thống!";//
        //await _emailProvider.SendEmailAsync(notification.Email, "Welcome", emailBody);//

        Console.WriteLine($"[EVENT BACKGROUND] Đã mô phỏng gửi Email chào mừng tới: {notification.Email}");

        return Task.CompletedTask;
    }
}