using Microsoft.Extensions.DependencyInjection;
using QRHub.Application.Services;

namespace QRHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQrCodeService, QrCodeService>();

        return services;
    }
}
