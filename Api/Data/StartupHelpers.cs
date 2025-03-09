using Microsoft.Extensions.Options;
using Reservant.Api.Data.Seeding;
using Reservant.Api.Configuration;
using Reservant.Api.Services;

namespace Reservant.Api.Data;

/// <summary>
/// Extension methods to help with setting up the database
/// </summary>
public static class StartupHelpers
{
    /// <summary>
    /// Ensure the database is created and seeded, and the file upload folder exists
    /// </summary>
    public static async Task EnsureDatabaseCreatedAndSeeded(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var debugService = scope.ServiceProvider.GetRequiredService<DebugService>();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

        var fileUploadsOptions = scope.ServiceProvider.GetRequiredService<IOptions<FileUploadsOptions>>().Value;
        if (!Path.Exists(fileUploadsOptions.GetFullSavePath()))
        {
            Directory.CreateDirectory(fileUploadsOptions.GetFullSavePath());
        }

        if (app.Configuration.GetValue<bool>(ConfigurationKeys.RecreateDatabaseOnStart))
        {
            await debugService.RecreateDatabase();
        }
        else if (await context.Database.EnsureCreatedAsync())
        {
            await seeder.SeedDataAsync();
        }
    }

    /// <summary>
    /// Add database seeding services to the collection
    /// </summary>
    public static void AddDbSeedingServices(this IServiceCollection services)
    {
        var restaurantSeeders = typeof(DbSeeder).Assembly
            .DefinedTypes
            .Where(x => x.IsAssignableTo(typeof(RestaurantSeeder)) && x != typeof(RestaurantSeeder));
        foreach (var seederType in restaurantSeeders)
        {
            services.AddScoped(typeof(RestaurantSeeder), seederType);
        }

        services.AddScoped<DbSeeder>();
    }
}
