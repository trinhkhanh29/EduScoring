using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduScoring.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace EduScoring.Common.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Thiếu Jwt Secret");
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("FullName", user.FullName)
            // Sau này bạn có thể map Role của User vào đây
        };

        if (user.UserRoles != null && user.UserRoles.Any())
        {
            claims.AddRange(
                user.UserRoles
                    .Where(userRole => userRole.Role != null && !string.IsNullOrEmpty(userRole.Role.Name))
                    .Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name))
            );
        }
    
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }
}