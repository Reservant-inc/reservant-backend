using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Models;
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
    VisitService visitService,
    OrderService orderService,
    ValidationService validationService,
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
    public async Task<Result<VisitSummaryVM>> AddFutureVisitAsync(User user)
    {
        var visitResult = await visitService.CreateVisitAsync(
            new CreateVisitRequest
            {
                Date = new DateTime(2030, 3, 23, 14, 0, 0),
                NumberOfGuests = 1,
                Participants = [user.Id],
                RestaurantId = 1,
                TableId = 1,
                Takeaway = false,
                Tip = new decimal(1.50)
            },
            user
        );
        
        var orderResult = await orderService.CreateOrderAsync(
            new CreateOrderRequest
            {
                Items = [
                    new CreateOrderItemRequest
                    {
                        MenuItemId = 1,
                        Amount = 2
                    },
                    new CreateOrderItemRequest
                    {
                        MenuItemId = 0,
                        Amount = 4
                    }
                ],
                Note = "This is a debug note",
                VisitId = visitResult.Value.VisitId
            },
            user
        );
        
        return new VisitSummaryVM
        {
            ClientId = user.Id,
            Date = visitResult.Value.Date,
            Deposit = visitResult.Value.Deposit,
            NumberOfPeople = visitResult.Value.NumberOfPeople,
            RestaurantId = visitResult.Value.RestaurantId,
            Takeaway = visitResult.Value.Takeaway,
            VisitId = visitResult.Value.VisitId
        };
    }
    
    
}
