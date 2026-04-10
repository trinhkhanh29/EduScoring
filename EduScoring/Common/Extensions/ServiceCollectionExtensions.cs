using EduScoring.Common.Authentication;
using EduScoring.Common.Storage;
using EduScoring.Features.Auth.Features.Login;
using EduScoring.Features.Auth.Features.Register;
using EduScoring.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace EduScoring.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // ── 1. Database
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("[STARTUP][CRITICAL] Thiếu ConnectionStrings:DefaultConnection trong config.");
            throw new InvalidOperationException("Thiếu connection string 'DefaultConnection'.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        Console.WriteLine("[STARTUP][DB] Đã đăng ký AppDbContext với Npgsql.");

        // ── 2. Cloudinary
        services.Configure<CloudinarySettings>(config.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<ICloudinaryService, CloudinaryService>(); // ← chỉ đăng ký 1 lần qua interface
        Console.WriteLine("[STARTUP][CLOUDINARY] Đã đăng ký CloudinaryService.");

        // ── 3. JWT Settings
        var jwtSettings = new JwtSettings();
        config.Bind(JwtSettings.SectionName, jwtSettings);

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            Console.WriteLine("[STARTUP][CRITICAL] JwtSettings:Secret chưa được cấu hình.");
            throw new InvalidOperationException("JwtSettings:Secret không được để trống.");
        }
        if (jwtSettings.Secret.Length < 32)
            Console.WriteLine($"[STARTUP][WARNING] JwtSettings:Secret quá ngắn ({jwtSettings.Secret.Length} chars). Khuyến nghị >= 32 ký tự.");
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            Console.WriteLine("[STARTUP][WARNING] JwtSettings:Issuer chưa được cấu hình.");
        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            Console.WriteLine("[STARTUP][WARNING] JwtSettings:Audience chưa được cấu hình.");

        services.AddSingleton(jwtSettings);
        services.AddScoped<IJwtProvider, JwtProvider>();
        Console.WriteLine($"[STARTUP][JWT] Secret={new string('*', jwtSettings.Secret.Length)} | Issuer={jwtSettings.Issuer} | Audience={jwtSettings.Audience}");

        // ── 4. Authentication + JWT Bearer Events
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
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var msg = context.Exception switch
                        {
                            SecurityTokenExpiredException e => $"Token hết hạn lúc {e.Expires:O}",
                            SecurityTokenInvalidIssuerException => "Issuer không hợp lệ.",
                            SecurityTokenInvalidAudienceException => "Audience không hợp lệ.",
                            SecurityTokenSignatureKeyNotFoundException => "Không tìm thấy signing key.",
                            SecurityTokenInvalidSignatureException => "Chữ ký token không hợp lệ.",
                            _ => context.Exception.Message
                        };
                        Console.WriteLine($"[AUTH][401] {msg}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var userId = context.Principal?.FindFirst("sub")?.Value ?? "unknown";
                        Console.WriteLine($"[AUTH][OK] UserId={userId}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"[AUTH][401] Thiếu hoặc từ chối token | Path={context.Request.Path}");
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        Console.WriteLine($"[AUTH][403] Không đủ quyền | Path={context.Request.Path}");
                        return Task.CompletedTask;
                    }
                };
            });

        Console.WriteLine("[STARTUP][AUTH] Đã đăng ký JWT Bearer Authentication.");

        // ── 5. CORS
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        Console.WriteLine("[STARTUP][CORS] AllowAnyOrigin (chỉ dùng cho dev — restrict lại khi lên prod).");

        // ── 6. Misc
        services.AddControllers();
        services.AddAuthorization();
        services.AddOpenApi();

        // ── 7. Login-register
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RegisterCommandHandler>();
        Console.WriteLine("[STARTUP][HANDLERS] Đã đăng ký Login & Register Handlers.");

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
                Console.WriteLine("[STARTUP][HANDLERS] Đã đăng ký Handlers và MediatR.");

        Console.WriteLine("[STARTUP] AddApplicationServices hoàn tất.");
        return services;
    }
}