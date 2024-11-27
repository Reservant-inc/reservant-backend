using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Dtos.OrderItems;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Options;
using Reservant.Api.Services;
using Reservant.Api.Services.VisitServices;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Service for adding sample data to the database
/// </summary>
public class DbSeeder(
    ApiDbContext context,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserService userService,
    MakeReservationService makeReservationService,
    OrderService orderService,
    IOptions<FileUploadsOptions> fileUploadsOptions,
    ILogger<DbSeeder> logger,
    IServiceProvider serviceProvider)
{
    private const string ExampleUploadsPath = "././example-uploads";
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    /// <summary>
    /// Add sample data to the database
    /// </summary>
    public async Task SeedDataAsync()
    {
        await CreateRoles();

        var users = await UserSeeder.CreateUsers(userService);
        await AddExampleUploads();
        await context.SaveChangesAsync();

        foreach (var restaurantSeeder in serviceProvider.GetServices<RestaurantSeeder>())
        {
            await restaurantSeeder.Seed(users);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Create roles
    /// </summary>
    private async Task CreateRoles()
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Customer));
        await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.RestaurantOwner));
        await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.RestaurantEmployee));
        await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.CustomerSupportAgent));
        await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.CustomerSupportManager));
    }

    /// <summary>
    /// Add file uploads for each file in <see cref="ExampleUploadsPath"/>
    /// </summary>
    /// <remarks>
    /// The directory is expected to contain folders named after user logins
    /// containing files to add for the respective user.
    /// The files are uploaded with the same name.
    /// </remarks>
    private async Task AddExampleUploads()
    {
        foreach (var userFolder in Directory.GetDirectories(ExampleUploadsPath))
        {
            var userLogin = Path.GetFileName(userFolder);
            var userId = await context.Users
                .Where(u => u.UserName == userLogin)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"Cannot add example uploads for the user {userLogin}: no such user");
            }

            foreach (var filePath in Directory.GetFiles(userFolder))
            {
                if (!_contentTypeProvider.TryGetContentType(filePath, out var contentType))
                {
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                File.Copy(filePath, Path.Combine(fileUploadsOptions.Value.SavePath, fileName));
                context.Add(new FileUpload
                {
                    UserId = userId,
                    FileName = fileName,
                    ContentType = contentType
                });

                logger.ExampleUploadAdded(fileName, userLogin, userId);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates visit in the future
    /// </summary>
    public async Task<VisitSummaryVM> AddFutureVisitAsync()
    {
        await context.Database.BeginTransactionAsync();

        var exampleCustomer = await context.Users.FirstAsync(u => u.UserName == "customer");

        var visitDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var visitResult = (await makeReservationService.MakeReservation(
            new MakeReservationRequest
            {
                Date = new DateTime(visitDay, new TimeOnly(18, 00)),
                EndTime = new DateTime(visitDay, new TimeOnly(18, 30)),
                NumberOfGuests = 1,
                ParticipantIds = [exampleCustomer.Id],
                RestaurantId = 1,
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

        await context.Database.CommitTransactionAsync();

        return visitResult;
    }
}
