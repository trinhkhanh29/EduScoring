using EduScoring.Data.Entities;
using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace EduScoring.Features.Auth.Features.Register;

public class RegisterCommandHandler
{
    private readonly AppDbContext _db;
    //private readonly IMediator _mediator;//

    public RegisterCommandHandler(AppDbContext db, IMediator mediator)
    {
        _db = db;
        //_mediator = mediator;//
    }

    public async Task<(bool IsSuccess, RegisterResponse? Data, string ErrorMessage)> Handle(RegisterCommand command, Guid adminUserId)
    {
        var tag = "[Register]";
        Console.WriteLine($"{tag} Yêu cầu tạo user mới bởi AdminId: {adminUserId} — Email: '{command.Email}' | Role: '{command.Role}'");

        // 1. Validate Role
        if (string.IsNullOrWhiteSpace(command.Role))
        {
            return (false, null, "Trường 'Role' là bắt buộc và không được để trống.");
        }
        if (command.Role != "Teacher" && command.Role != "Student")
        {
            return (false, null, "Role chỉ được phép là 'Teacher' hoặc 'Student'.");
        }

        // 2. Kiểm tra tồn tại
        var emailExists = await _db.Users.AnyAsync(u => u.Email == command.Email);
        if (emailExists)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Email '{command.Email}' đã tồn tại.");
            return (false, null, "Email đã tồn tại!");
        }

        // 3. Tìm Role
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == command.Role);
        if (role == null)
        {
            return (false, null, $"Role '{command.Role}' không hợp lệ.");
        }

        // 4. Tạo User (hash password)
        var newUser = new User
        {
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
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Lỗi khi lưu database: {ex.Message}");
            return (false, null, "Email đã tồn tại hoặc lỗi hệ thống khi lưu tài khoản.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{tag} THẤT BẠI — Lỗi không xác định: {ex.GetType().Name}: {ex.Message}");
            return (false, null, "Lỗi hệ thống không xác định. Vui lòng liên hệ admin.");
        }

        Console.WriteLine($"{tag} THÀNH CÔNG — UserId: {newUser.Id} | Email: '{newUser.Email}' | Role: '{command.Role}' | CreatedBy: {adminUserId}");
        return (true, new RegisterResponse(newUser.Id, $"Đăng ký tài khoản {command.Role} thành công!"), string.Empty);
    }
}