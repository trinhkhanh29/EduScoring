using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduScoring.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace EduScoring.Common.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtProvider> _logger;

    public JwtProvider(IConfiguration configuration, ILogger<JwtProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user)
    {
        // ── 1. Đọc & validate config
        var secretKey = _configuration["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogCritical("[JWT] JwtSettings:Secret chưa được cấu hình.");
            throw new InvalidOperationException("Thiếu Jwt Secret");
        }

        if (secretKey.Length < 32)
        {
            _logger.LogWarning("[JWT] Secret key quá ngắn ({Length} chars). Khuyến nghị >= 32 ký tự.", secretKey.Length);
        }

        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        if (string.IsNullOrWhiteSpace(issuer))
            _logger.LogWarning("[JWT] JwtSettings:Issuer chưa được cấu hình.");
        if (string.IsNullOrWhiteSpace(audience))
            _logger.LogWarning("[JWT] JwtSettings:Audience chưa được cấu hình.");

        // ── 2. Validate user object
        if (user is null)
        {
            _logger.LogError("[JWT] GenerateToken được gọi với user = null.");
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.Email))
            _logger.LogWarning("[JWT] User {UserId} không có Email – claim email sẽ rỗng.", user.Id);

        // ── 3. Build claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),   // unique token ID – hữu ích khi revoke
            new("FullName", user.FullName ?? string.Empty),
        };

        var roles = user.UserRoles?
            .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.Name))
            .Select(ur => ur.Role.Name!)
            .ToList() ?? [];

        if (roles.Count == 0)
            _logger.LogWarning("[JWT] User {UserId} không có Role nào được gán.", user.Id);

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        _logger.LogDebug("[JWT] Tạo token cho User {UserId} | Roles: [{Roles}]",
            user.Id, string.Join(", ", roles));

        // ── 4. Tạo token
        try
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.UtcNow.AddHours(
                _configuration.GetValue<int>("JwtSettings:ExpiryHours", 2));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var jwt = handler.WriteToken(token);

            _logger.LogInformation("[JWT] Token tạo thành công cho User {UserId}. Hết hạn lúc {Expiry:O}",
                user.Id, expiry);

            return jwt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JWT] Lỗi khi tạo token cho User {UserId}.", user.Id);
            throw new InvalidOperationException(
                $"Không thể tạo JWT token cho User '{user.Id}'.", ex);
        }
    }
}