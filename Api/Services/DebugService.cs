using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Options;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for debug features
/// </summary>
public class DebugService(
    ApiDbContext context,
    VisitService visitService,
    OrderService orderService,
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

    /// <summary>
    /// Creates visit in a future
    /// </summary>
    public async Task AddFutureVisitAsync(User user)
    {
        var exampleVisit = visitService.CreateVisitAsync(
            new CreateVisitRequest
            {
                Date = new DateTime(2030, 3, 23, 14, 0, 0),
                NumberOfGuests = 2,
                Participants = [user.Id],
                RestaurantId = 1,
                TableId = 1,
                Takeaway = false,
                Tip = new decimal(1.50)
            }, 
            user
            );

        if (exampleVisit.Result.IsError)
        {
            throw new ValidationResult(
                
                )
        }




    }
    
    
}
