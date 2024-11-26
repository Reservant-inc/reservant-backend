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
public class DbSeeder
{
    private const string ExampleUploadsPath = "././example-uploads";
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();
    private readonly ApiDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly MakeReservationService _makeReservationService;
    private readonly OrderService _orderService;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DbSeeder> _logger;

    /// <summary>
    /// Konstuktor
    /// </summary>
    /// <param name="context"></param>
    /// <param name="roleManager"></param>
    /// <param name="userService"></param>
    /// <param name="restaurantService"></param>
    /// <param name="makeReservationService"></param>
    /// <param name="orderService"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="fileUploadsOptions"></param>
    /// <param name="geometryFactory"></param>
    public DbSeeder(
        ApiDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserService userService,
        RestaurantService restaurantService,
        MakeReservationService makeReservationService,
        OrderService orderService,
        ILoggerFactory loggerFactory,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory)
    {
        _context = context;
        _roleManager = roleManager;
        _userService = userService;
        _restaurantService = restaurantService;
        _makeReservationService = makeReservationService;
        _orderService = orderService;
        _loggerFactory = loggerFactory;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _logger = loggerFactory.CreateLogger<DbSeeder>();
    }

    /// <summary>
    /// Add sample data to the database
    /// </summary>
    public async Task SeedDataAsync()
    {
        // Create roles
        await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Customer));
        await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.RestaurantOwner));
        await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.RestaurantEmployee));
        await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.CustomerSupportAgent));
        await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.CustomerSupportManager));

        // Register users
        var bok1 = (await _userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "support@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, Guid.Parse("fced96c1-dad9-49ff-a598-05e1c5e433aa"))).OrThrow();

        var bok2 = (await _userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "manager@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kierownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            IsManager = true
        }, Guid.Parse("3f97a9d9-21b5-40ae-b178-bfe071de723c"))).OrThrow();

        var johnDoe = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Login = "JD",
            Email = "john@doe.pl",
            PhoneNumber = "+48123456789",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1990, 2, 3)
        }, Guid.Parse("e5779baf-5c9b-4638-b9e7-ec285e57b367"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(johnDoe.Id);

        var anon = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Anon",
            LastName = "Ymus",
            Login = "AY",
            Email = "anon@ymus.pl",
            PhoneNumber = "+48987654321",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1989, 1, 2)
        }, Guid.Parse("28b618d7-2f32-4f0c-823d-e63ffa56e47f"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(anon.Id);

        var walter = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Walter",
            LastName = "White",
            Login = "WW",
            Email = "walter@white.pl",
            PhoneNumber = "+48475927476",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1991, 3, 2)
        }, Guid.Parse("e20eeb3b-563c-480a-8b8c-85b3afac7c66"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(walter.Id);

        var geralt = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Geralt",
            LastName = "Riv",
            Login = "GR",
            Email = "geralt@riv.pl",
            PhoneNumber = "+48049586273",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1986, 12, 12)
        }, Guid.Parse("5ad4c90f-c52a-4b14-a8e5-e12eecfd4c8c"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(geralt.Id);

        var muadib = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Paul",
            LastName = "Atreides",
            Login = "PA",
            Email = "paul@atreides.pl",
            PhoneNumber = "+48423597532",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1978, 4, 20)
        }, Guid.Parse("f1e788f1-523c-4aa9-b26f-5eb43ce59573"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(muadib.Id);

        var kowalski = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Krzysztof",
            LastName = "Kowalski",
            Login = "KK",
            Email = "krzysztof.kowalski@gmail.com",
            PhoneNumber = "+48999999999",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(2002, 1, 1)
        }, Guid.Parse("558614c5-ba9f-4c1a-ba1c-07b2b67c37e9"))).OrThrow();
        await _userService.MakeRestaurantOwnerAsync(kowalski.Id);

        var customer1 = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer",
            Email = "customer1@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Customer",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, Guid.Parse("e08ff043-f8d2-45d2-b89c-aec4eb6a1f29"))).OrThrow();

        var customer2 = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer2",
            Email = "customer2@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Ewa",
            LastName = "Przykładowska",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, Guid.Parse("86a24e58-cb06-4db0-a346-f75125722edd"))).OrThrow();

        var customer3 = (await _userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer3",
            Email = "customer3@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kacper",
            LastName = "Testowy",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, Guid.Parse("a79631a0-a3bf-43fa-8fbe-46e5ee697eeb"))).OrThrow();

        // Add example uploads
        await AddExampleUploads();

        // Create restaurant groups
        var johnDoesGroup = new RestaurantGroup
        {
            Name = "John Doe's Restaurant Group",
            OwnerId = johnDoe.Id
        };
        _context.RestaurantGroups.Add(johnDoesGroup);

        var kowalskisGroup = new RestaurantGroup
        {
            Name = "Krzysztof Kowalski's Restaurant Group",
            OwnerId = kowalski.Id
        };
        _context.RestaurantGroups.Add(kowalskisGroup);

        var anonGroup = new RestaurantGroup
        {
            Name = "Anon Ymus' Restaurant Group",
            OwnerId = anon.Id
        };
        _context.RestaurantGroups.Add(anonGroup);

        var geraltsGroup = new RestaurantGroup
        {
            Name = "Geralt's Restaurant Group",
            OwnerId = geralt.Id
        };
        _context.RestaurantGroups.Add(geraltsGroup);

        var paulsGroup = new RestaurantGroup
        {
            Name = "Paul Muadib Atreides' Restaurant Group",
            OwnerId = muadib.Id
        };
        _context.RestaurantGroups.Add(paulsGroup);

        var waltersGroup = new RestaurantGroup
        {
            Name = "Heisenberg's Restaurant Group",
            OwnerId = walter.Id
        };
        _context.RestaurantGroups.Add(waltersGroup);

        await _context.SaveChangesAsync();

        // Seed restaurants
        var johnDoeLogger = _loggerFactory.CreateLogger<JohnDoesRestaurantSeeder>();
        var johnDoesRestaurantSeeder = new JohnDoesRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            johnDoeLogger,
            _fileUploadsOptions,
            _geometryFactory,
            johnDoe,
            johnDoesGroup,
            bok1);

        await johnDoesRestaurantSeeder.SeedAsync();

        var kowalskiLogger = _loggerFactory.CreateLogger<KowalskisRestaurantSeeder>();
        var kowalskisRestaurantSeeder = new KowalskisRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            kowalskiLogger,
            _fileUploadsOptions,
            _geometryFactory,
            kowalski,
            kowalskisGroup,
            bok1);

        await kowalskisRestaurantSeeder.SeedAsync();

        var anonsLogger = _loggerFactory.CreateLogger<AnonRestaurantSeeder>();
        var anonRestaurantSeeder = new AnonRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            anonsLogger,
            _fileUploadsOptions,
            _geometryFactory,
            anon,
            anonGroup,
            bok1);

        await anonRestaurantSeeder.SeedAsync();

        var witchersLogger = _loggerFactory.CreateLogger<WitcherRestaurantSeeder>();
        var witcherRestaurantSeeder = new WitcherRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            witchersLogger,
            _fileUploadsOptions,
            _geometryFactory,
            geralt,
            geraltsGroup,
            bok1);

        await witcherRestaurantSeeder.SeedAsync();

        var atreidesLogger = _loggerFactory.CreateLogger<AtreidesRestaurantSeeder>();
        var atreidesRestaurantSeeder = new AtreidesRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            atreidesLogger,
            _fileUploadsOptions,
            _geometryFactory,
            muadib,
            paulsGroup,
            bok1);

        await atreidesRestaurantSeeder.SeedAsync();

        var breakingBadLogger = _loggerFactory.CreateLogger<BreakingBadRestaurantSeeder>();
        var breakingBadRestaurantSeeder = new BreakingBadRestaurantSeeder(
            _context,
            _userService,
            _restaurantService,
            breakingBadLogger,
            _fileUploadsOptions,
            _geometryFactory,
            walter,
            waltersGroup,
            bok1);

        await breakingBadRestaurantSeeder.SeedAsync();

        // Save all changes
        await _context.SaveChangesAsync();
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
            var userId = await _context.Users
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
                File.Copy(filePath, Path.Combine(_fileUploadsOptions.Value.SavePath, fileName));
                _context.Add(new FileUpload
                {
                    UserId = userId,
                    FileName = fileName,
                    ContentType = contentType
                });

                _logger.ExampleUploadAdded(fileName, userLogin, userId);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates visit in the future
    /// </summary>
    public async Task<VisitSummaryVM> AddFutureVisitAsync()
    {
        await _context.Database.BeginTransactionAsync();

        var exampleCustomer = await _context.Users.FirstAsync(u => u.UserName == "customer");

        var visitDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var visitResult = (await _makeReservationService.MakeReservation(
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

        var orderResult = (await _orderService.CreateOrderAsync(
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

        await _context.Database.CommitTransactionAsync();

        return visitResult;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<FileUpload> RequireFileUpload(string fileName, User owner)
    {
        var upload = await _context.FileUploads.FirstOrDefaultAsync(x => x.FileName == fileName) ??
               throw new InvalidDataException($"Upload {fileName} not found");
        if (upload.UserId != owner.Id)
        {
            throw new InvalidDataException($"Upload {fileName} is not owned by {owner.UserName}");
        }

        return upload;
    }
}
