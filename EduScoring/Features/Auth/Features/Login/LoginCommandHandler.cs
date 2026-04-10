using EduScoring.Common.Authentication;
using EduScoring.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace EduScoring.Features.Auth.Features.Login;

public class LoginCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IJwtProvider _jwtProvider;
    private readonly IMediator _mediator;

    // Inject các công cụ cần thiết vào đây
    public LoginCommandHandler(AppDbContext db, IJwtProvider jwtProvider, IMediator mediator)
    {
        _db = db;
        _jwtProvider = jwtProvider;
        _mediator = mediator;
    }

    // Hàm thực thi chính
    public async Task<(bool IsSuccess, LoginResponse? Data, string ErrorMessage)> Handle(LoginCommand command)
    {
        // 1. Tìm User
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (user == null)
            return (false, null, "Email hoặc mật khẩu không chính xác!");

        // 2. Kiểm tra Pass
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash);
        if (!isPasswordValid)
            return (false, null, "Email hoặc mật khẩu không chính xác!");

        // 3. Đúc Token
        var token = _jwtProvider.GenerateToken(user);

        await _mediator.Publish(new UserLoggedInEvent(user.Id, user.Email, DateTime.UtcNow));

        return (true, new LoginResponse(token, "Đăng nhập thành công!"), string.Empty);
    }
}