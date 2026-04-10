using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace EduScoring.Features.Auth.Features.Register;

public class RegisterCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public RegisterCommandHandler(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<(bool IsSuccess, RegisterResponse? Data, string ErrorMessage)> Handle(RegisterCommand command)
    {
        Console.WriteLine($"[Register] Bắt đầu xử lý đăng ký — Username: '{command.Username}' | Email: '{command.Email}' | Role: '{command.RoleName}'");

        // 1. Kiểm tra tồn tại
        var emailExists = await _db.Users.AnyAsync(u => u.Email == command.Email);
        var usernameExists = await _db.Users.AnyAsync(u => u.Username == command.Username);

        if (emailExists && usernameExists)
        {
            Console.WriteLine($"[Register] THẤT BẠI — Email '{command.Email}' và Username '{command.Username}' đều đã tồn tại trong hệ thống.");
            return (false, null, "Email và Username đã tồn tại!");
        }
        if (emailExists)
        {
            Console.WriteLine($"[Register] THẤT BẠI — Email '{command.Email}' đã được đăng ký bởi tài khoản khác.");
            return (false, null, "Email đã tồn tại!");
        }
        if (usernameExists)
        {
            Console.WriteLine($"[Register] THẤT BẠI — Username '{command.Username}' đã được sử dụng bởi tài khoản khác.");
            return (false, null, "Username đã tồn tại!");
        }

        // 2. Tìm Role
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == command.RoleName);
        if (role == null)
        {
            var availableRoles = await _db.Roles.Select(r => r.Name).ToListAsync();
            Console.WriteLine($"[Register] THẤT BẠI — Quyền '{command.RoleName}' không tồn tại trong hệ thống.");
            Console.WriteLine($"[Register] Các quyền hợp lệ hiện có: [{string.Join(", ", availableRoles)}]");
            return (false, null, $"Quyền '{command.RoleName}' không hợp lệ.");
        }

        // 3. Tạo User
        var newUser = new User
        {
            Username = command.Username,
            Email = command.Email,
            FullName = command.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        newUser.UserRoles.Add(new UserRole { Role = role });
        _db.Users.Add(newUser);

        try
        {
            await _mediator.Publish(new UserRegisteredEvent(newUser.Id, newUser.Email, newUser.FullName));
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"[Register] THẤT BẠI — Lỗi khi lưu database.");
            Console.WriteLine($"[Register] DbUpdateException: {ex.Message}");
            Console.WriteLine($"[Register] Inner Exception: {ex.InnerException?.Message}");
            return (false, null, "Lỗi hệ thống khi lưu tài khoản. Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Register] THẤT BẠI — Lỗi không xác định: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[Register] StackTrace: {ex.StackTrace}");
            return (false, null, "Lỗi hệ thống không xác định. Vui lòng liên hệ admin.");
        }

        Console.WriteLine($"[Register] THÀNH CÔNG — UserId: {newUser.Id} | Username: '{newUser.Username}' | Email: '{newUser.Email}' | Role: '{command.RoleName}'");
        return (true, new RegisterResponse(newUser.Id, $"Đăng ký tài khoản {command.RoleName} thành công!"), string.Empty);
    }
}