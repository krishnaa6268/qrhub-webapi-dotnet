using Microsoft.EntityFrameworkCore;
using QRHub.Infrastructure.Persistence;

namespace QRHub.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Applying database migrations to PostgreSQL...");
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}
