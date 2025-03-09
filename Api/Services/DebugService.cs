using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Data.Seeding;
using Reservant.Api.Configuration;

namespace Reservant.Api.Services;

/// <summary>
/// Service for debug features
/// </summary>
public class DebugService(
    ApiDbContext context,
    DbSeeder dbSeeder,
    IOptions<FileUploadsOptions> uploadOptions
    )
{
    /// <summary>
    /// Remove all data including files on disk and seed the database
    /// </summary>
    public async Task RecreateDatabase()
    {
        foreach (var file in Directory.GetFiles(uploadOptions.Value.SavePath))
        {
            File.Delete(file);
        }

        await context.DropAllTablesAsync();
        await context.Database.EnsureCreatedAsync();
        await dbSeeder.SeedDataAsync();
    }
}
