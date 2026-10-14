using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QRHub.Application.Common.Interfaces;
using QRHub.Infrastructure.Authentication;
using QRHub.Infrastructure.Persistence;
using QRHub.Infrastructure.Services;

namespace QRHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // HTTP Context Accessor
        services.AddHttpContextAccessor();

        // Infrastructure Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IQrCodeGenerator, QrCodeGeneratorService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // JWT Authentication setup
        var secretKey = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(jwtSettings.Secret) 
            ? "default_super_secret_key_at_least_32_chars_long_12345!" 
            : jwtSettings.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // set true in strict prod with SSL certificate
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
