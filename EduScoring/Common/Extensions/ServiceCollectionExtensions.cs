using EduScoring.Common.Authentication;
using EduScoring.Common.Storage;
using EduScoring.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduScoring.Common.Extensions;

public static class ServiceCollectionExtensions
{
    // Hàm này sẽ gom toàn bộ cấu hình Database, Cloudinary, JWT...
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // 1. Database & Storage
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<CloudinaryService>();

        // 2. Cấu hình JWT Settings
        var jwtSettings = new JwtSettings();
        config.Bind(JwtSettings.SectionName, jwtSettings);
        services.AddSingleton(jwtSettings);
        services.AddScoped<IJwtProvider, JwtProvider>();

        // 3. Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        // 4. CORS
        services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

        // 5. Các cấu hình khác
        services.AddControllers();
        services.AddAuthorization();
        services.AddOpenApi();

        return services;
    }
}