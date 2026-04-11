using EduScoring.Features.Auth.Features.Login;
using MediatR;

namespace EduScoring.Features.Auth.Features.Login;

public class LogUserActivityHandler : INotificationHandler<UserLoggedInEvent>
{
    public Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        // SAU NÀY BỎ COMMENT ĐỂ CODE CHẠY THẬT
        // 1. Tạo bản ghi ActivityLog
        // 2. Lưu vào DB: await _db.ActivityLogs.AddAsync(log); await _db.SaveChangesAsync();//

        Console.WriteLine($"[EVENT BACKGROUND] User {notification.Email} vừa đăng nhập lúc {notification.LoginTime}. Đã ghi Log.");

        return Task.CompletedTask;
    }
}