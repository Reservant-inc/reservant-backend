using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Data;

namespace Reservant.Api.Controllers;

/// <summary>
/// Debugging functions
/// </summary>
[ApiController, Route("/debug")]
public class DebugController(ApiDbContext context, DbSeeder seeder)
{
    /// <summary>
    /// Use this if you get a "no such column" error
    /// </summary>
    [HttpPost("recreate-database")]
    public async Task RecreateDatabase()
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await seeder.SeedDataAsync();
    }
}
