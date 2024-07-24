using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Options;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

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
