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
    VisitService visitService,
    OrderService orderService,
    UserService userService,
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
    /// Creates visit in the future
    /// </summary>
    public async Task<VisitSummaryVM> AddFutureVisitAsync()
    {
        var exampleCustomer = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "exampleCustomer",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Customer",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456769",
            BirthDate = new DateOnly(2000, 1, 1)
        }, "e08ff043-f8d2-45d2-b89c-aec4eb6a1f28")).OrThrow();
        
        
        
        
        var visitResult = (await visitService.CreateVisitAsync(
            new CreateVisitRequest
            {
                Date = new DateTime(2030, 3, 23, 14, 0, 0),
                NumberOfGuests = 1,
                Participants = [exampleCustomer.Id],
                RestaurantId = 1,
                TableId = 1,
                Takeaway = false,
                Tip = new decimal(1.50)
            },
            exampleCustomer
        )).OrThrow();
        
        var orderResult = (await orderService.CreateOrderAsync(
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
                        MenuItemId = 2,
                        Amount = 4
                    }
                ],
                Note = "This is a debug note",
                VisitId = visitResult.VisitId
            },
            exampleCustomer
        )).OrThrow();
        
        return new VisitSummaryVM
        {
            ClientId = exampleCustomer.Id,
            Date = visitResult.Date,
            Deposit = visitResult.Deposit,
            NumberOfPeople = visitResult.NumberOfPeople,
            RestaurantId = visitResult.RestaurantId,
            Takeaway = visitResult.Takeaway,
            VisitId = visitResult.VisitId
        };
    }
    
    
}
