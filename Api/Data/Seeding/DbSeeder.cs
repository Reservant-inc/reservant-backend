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
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;
using Reservant.Api.Services.OrderServices;
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
    MakeOrderService makeOrderService,
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

        var johnDoes = await context.Restaurants.SingleAsync(r => r.Name == "John Doe's");
        var visits = new List<Visit>
        {
            new()
            {
                Restaurant = johnDoes,
                NumberOfGuests = 1,
                Client = users.Customer1,
                Participants = [users.Customer2, users.Customer3],
                Reservation = new Reservation
                {
                    StartTime = new DateTime(2024, 1, 1, 17, 0, 0),
                    Deposit = null,
                    ReservationDate = new DateTime(2024, 1, 1, 22, 32, 00),
                },
                Takeaway = true,
                TableId = 1,
                Tip = null,
            },
            new()
            {
                Restaurant = johnDoes,
                NumberOfGuests = 1,
                Client = users.Customer2,
                Participants = [users.Customer3],
                Reservation = new Reservation
                {
                    StartTime = new DateTime(2024, 1, 4, 18, 0, 0),
                    ReservationDate = new DateTime(2024, 1, 1, 22, 32, 00),
                    Deposit = null,
                },
                Takeaway = false,
                TableId = 2,
                Tip = 10m,
            },
            new()
            {
                Restaurant = johnDoes,
                NumberOfGuests = 1,
                Client = users.Customer2,
                Reservation = new Reservation
                {
                    StartTime = new DateTime(2024, 1, 5, 18, 0, 0),
                    ReservationDate = new DateTime(2024, 1, 1, 15, 32, 00),
                    Deposit = null,
                },
                Takeaway = false,
                TableId = 1,
                Tip = 25m,
            },
        };

        var visit0Date = visits[0].Reservation!.ReservationDate!.Value;
        var visit1Date = visits[1].Reservation!.ReservationDate!.Value;
        context.Events.AddRange(
            new Event
            {
                Name="Posiadówa w John Doe's",
                CreatedAt = visit0Date.AddDays(-1),
                Description = "Event 1 Description",
                Time = visit0Date,
                MustJoinUntil = visit0Date.AddHours(-3),
                MaxPeople = 13,
                Creator = users.JohnDoe,
                RestaurantId = 1,
                Visit = visits[0],
                ParticipationRequests = [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = visit0Date.AddHours(-5),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = visit0Date.AddHours(-5),
                        DateAccepted = visit0Date.AddHours(-4),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer3,
                        DateSent = visit0Date.AddHours(-5),
                        DateDeleted = visit0Date.AddHours(-4),
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe)
            },
            new Event
            {
                Name="Posiadówa w John Doe's vol. 2",
                CreatedAt = visit1Date.AddDays(-5),
                Description = "Event 2 Description",
                Time = visit1Date,
                MustJoinUntil = visit1Date.AddDays(-1),
                MaxPeople = 10,
                Creator = users.JohnDoe,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = visit1Date.AddDays(-3),
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe)
            },
            new Event
            {
                Name="Przyszłe Wydarzenie",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Description = "Event 3 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(10),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(10).AddHours(-1),
                MaxPeople = 5,
                Creator = users.Customer3,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = [
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = DateTime.UtcNow,
                    },
                    new ParticipationRequest
                    {
                        User = users.JohnDoe,
                        DateSent = DateTime.UtcNow,
                        DateAccepted = DateTime.UtcNow,
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe)
            },
            new Event
            {
                Name="Event 4",
                CreatedAt = DateTime.UtcNow,
                Description = "Event 4 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(15),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(15).AddHours(-1),
                MaxPeople = 20,
                Creator = users.Customer1,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = [],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe)
            },

            new Event
            {
                Name="Wydarzenie 5",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Description = "Event 5 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(20),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(20).AddHours(-1),
                MaxPeople = 20,
                Creator = users.Customer3,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = DateTime.UtcNow.AddHours(-1),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = DateTime.UtcNow,
                    },
                    new ParticipationRequest
                    {
                        User = users.JohnDoe,
                        DateSent = DateTime.UtcNow,
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe)
            }
        );

        context.Visits.AddRange(visits);

        var orders = new List<Order>
        {
            new Order
            {
                VisitId = visits.First().VisitId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 3,
                        OneItemPrice = 8m,
                        Status = OrderStatus.Taken,
                    }
                },
                Visit = visits.First()
            },
            new Order
            {
                VisitId = visits[1].VisitId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 1,
                        OneItemPrice = 39m,
                        Status = OrderStatus.Cancelled,
                    }
                },
                Visit = visits[1]
            },
            new Order
            {
                VisitId = visits[2].VisitId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 1,
                        OneItemPrice = 39m,
                        Status = OrderStatus.Taken,
                    },
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 2,
                        OneItemPrice = 45m,
                        Status = OrderStatus.Taken,
                    }
                },
                Visit = visits[2]
            }
        };


        context.Orders.AddRange(orders);

        var exampleJDThread = new MessageThread
        {
            Title = "Example Thread 1",
            CreationDate = DateTime.UtcNow,
            CreatorId = users.JohnDoe.Id,
            Creator = users.JohnDoe,
            Participants = [users.JohnDoe, users.Customer3],
            Messages = [
                new Message
                {
                    Contents = "hi!",
                    DateSent = DateTime.UtcNow,
                    DateRead = DateTime.UtcNow.AddMinutes(1),
                    AuthorId = users.JohnDoe.Id,
                    Author = users.JohnDoe
                },
                new Message
                {
                    Contents = "sup!",
                    DateSent = DateTime.UtcNow.AddMinutes(1),
                    DateRead = DateTime.UtcNow.AddMinutes(2),
                    AuthorId = users.Customer3.Id,
                    Author = users.Customer3
                },
                new Message
                {
                    Contents = "Thanks for visiting my restaurant! Did you enjoy the visit?",
                    DateSent = DateTime.UtcNow.AddMinutes(3),
                    DateRead = DateTime.UtcNow.AddMinutes(4),
                    AuthorId = users.JohnDoe.Id,
                    Author = users.JohnDoe
                },
                new Message
                {
                    Contents = "You're welcome. Yes I had a blast.",
                    DateSent = DateTime.UtcNow.AddMinutes(5),
                    DateRead = DateTime.UtcNow.AddMinutes(6),
                    AuthorId = users.Customer3.Id,
                    Author = users.Customer3
                },
                new Message
                {
                    Contents = "Would you like to leave a review then?",
                    DateSent = DateTime.UtcNow.AddMinutes(7),
                    DateRead = DateTime.UtcNow.AddMinutes(8),
                    AuthorId = users.JohnDoe.Id,
                    Author = users.JohnDoe
                },
                new Message
                {
                    Contents = "Sure, thanks for a reminder!",
                    DateSent = DateTime.UtcNow.AddMinutes(9),
                    DateRead = DateTime.UtcNow.AddMinutes(10),
                    AuthorId = users.Customer3.Id,
                    Author = users.Customer3
                }
            ]
        };

        await context.MessageThreads.AddAsync(exampleJDThread);

        var exampleJDGroupThread = new MessageThread
        {
            Title = "Example Thread 2",
            CreationDate = DateTime.UtcNow,
            CreatorId = users.JohnDoe.Id,
            Creator = users.JohnDoe,
            Participants = [users.JohnDoe, users.Customer1, users.Customer2],
            Messages = [
                new Message
                {
                    Contents = "hi!",
                    DateSent = DateTime.UtcNow,
                    DateRead = DateTime.UtcNow.AddMinutes(1),
                    AuthorId = users.JohnDoe.Id,
                    Author = users.JohnDoe
                },
                new Message
                {
                    Contents = "sup!",
                    DateSent = DateTime.UtcNow.AddMinutes(1),
                    DateRead = DateTime.UtcNow.AddMinutes(2),
                    AuthorId = users.Customer1.Id,
                    Author = users.Customer1
                },
                new Message
                {
                    Contents = "yo!",
                    DateSent = DateTime.UtcNow.AddMinutes(2),
                    DateRead = DateTime.UtcNow.AddMinutes(3),
                    AuthorId = users.Customer2.Id,
                    Author = users.Customer2
                },
                new Message
                {
                    Contents = "Thanks for visiting my restaurant",
                    DateSent = DateTime.UtcNow.AddMinutes(4),
                    DateRead = DateTime.UtcNow.AddMinutes(5),
                    AuthorId = users.JohnDoe.Id,
                    Author = users.JohnDoe
                }
            ]
        };
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

    private const string DefaultFutureVisitCustomerUsername = "customer";

    /// <summary>
    /// Creates visit in the future
    /// </summary>
    public async Task<VisitSummaryVM> AddFutureVisitAsync(Guid? userId)
    {
        await context.Database.BeginTransactionAsync();

        var exampleCustomer = userId == null
            ? await context.Users.FirstAsync(u => u.UserName == DefaultFutureVisitCustomerUsername)
            : await context.Users.FirstAsync(u => u.Id == userId);

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

        var orderResult = (await makeOrderService.CreateOrderAsync(
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

    private async Task<FileUpload> RequireFileUpload(string fileName, User owner)
    {
        var upload = await context.FileUploads.FirstOrDefaultAsync(x => x.FileName == fileName) ??
                     throw new InvalidDataException($"Upload {fileName} not found");
        if (upload.UserId != owner.Id)
        {
            throw new InvalidDataException($"Upload {fileName} is not owned by {owner.UserName}");
        }

        return upload;
    }
}
