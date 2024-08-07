using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data;

/// <summary>
/// Service for adding sample data to the database
/// </summary>
public class DbSeeder(
    ApiDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserService userService,
    RestaurantService restaurantService,
    VisitService visitService,
    OrderService orderService,
    ILogger<DbSeeder> logger,
    IOptions<FileUploadsOptions> fileUploadsOptions,
    GeometryFactory geometryFactory)
{
    private const string ExampleUploadsPath = "./example-uploads";
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    /// <summary>
    /// Add sample data to the database
    /// </summary>
    public async Task SeedDataAsync()
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Customer));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantOwner));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantEmployee));
        await roleManager.CreateAsync(new IdentityRole(Roles.CustomerSupportAgent));
        await roleManager.CreateAsync(new IdentityRole(Roles.CustomerSupportManager));

        context.AddRange(
            new WeatherForecast
            {
                Date = new DateOnly(2024, 1, 29),
                TemperatureC = -2,
                Summary = "Scorching"
            },
            new WeatherForecast
            {
                Date = new DateOnly(2024, 1, 30),
                TemperatureC = 19,
                Summary = "Hot"
            },
            new WeatherForecast
            {
                Date = new DateOnly(2024, 1, 31),
                TemperatureC = 45,
                Summary = "Chilly"
            },
            new WeatherForecast
            {
                Date = new DateOnly(2024, 1, 1),
                TemperatureC = -13,
                Summary = "Sweltering"
            },
            new WeatherForecast
            {
                Date = new DateOnly(2024, 1, 2),
                TemperatureC = 4,
                Summary = "Freezing"
            }
        );

        var bok1 = (await userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "support@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, "fced96c1-dad9-49ff-a598-05e1c5e433aa")).OrThrow();

        var bok2 = (await userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "manager@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kierownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            IsManager = true
        }, "3f97a9d9-21b5-40ae-b178-bfe071de723c")).OrThrow();

        var johnDoe = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Login = "JD",
            Email = "john@doe.pl",
            PhoneNumber = "+48123456789",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1990, 2, 3)
        }, "e5779baf-5c9b-4638-b9e7-ec285e57b367")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(johnDoe.Id);

        var anon = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Anon",
            LastName = "Ymus",
            Login = "AY",
            Email = "anon@ymus.pl",
            PhoneNumber = "+48987654321",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1989, 1, 2)
        }, "je4nd6f9-j4bn-9374-n4s3-j3nd85ht0a03")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(anon.Id);

        var walter = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Walter",
            LastName = "White",
            Login = "WW",
            Email = "walter@white.pl",
            PhoneNumber = "+48475927476",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1991, 3, 2)
        }, "meko2a3d-me2f-0394-me04-jend74t50sj3")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(walter.Id);

        var geralt = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Geralt",
            LastName = "Riv",
            Login = "GR",
            Email = "geralt@riv.pl",
            PhoneNumber = "+48049586273",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1986, 12, 12)
        }, "s3ka3ac0-m2ko-2137-ckw0-wmk32knap2ks")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(geralt.Id);

        var muadib = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Paul",
            LastName = "Atreides",
            Login = "PA",
            Email = "paul@atreides.pl",
            PhoneNumber = "+48423597532",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(1978, 4, 20)
        }, "emsko2a3-02ms-1376-m0z3-me2kzo2nsk20")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(muadib.Id);

        var kowalski = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Krzysztof",
            LastName = "Kowalski",
            Login = "KK",
            Email = "krzysztof.kowalski@gmail.com",
            PhoneNumber = "+48999999999",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(2002, 1, 1)
        }, "558614c5-ba9f-4c1a-ba1c-07b2b67c37e9")).OrThrow();
        await userService.MakeRestaurantOwnerAsync(kowalski.Id);

        var customer1 = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Customer",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, "e08ff043-f8d2-45d2-b89c-aec4eb6a1f29")).OrThrow();

        var customer2 = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer2",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Ewa",
            LastName = "Przykładowska",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, "86a24e58-cb06-4db0-a346-f75125722edd")).OrThrow();

        var customer3 = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer3",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kacper",
            LastName = "Testowy",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, "a79631a0-a3bf-43fa-8fbe-46e5ee697eeb")).OrThrow();

        await AddExampleUploads();

        var johnDoesGroup = new RestaurantGroup
        {
            Name = "John Doe's Restaurant Group",
            OwnerId = johnDoe.Id
        };
        context.RestaurantGroups.Add(johnDoesGroup);

        await CreateJohnDoesRestaurant(johnDoe, johnDoesGroup, bok1);
        await CreateJohnDoes2Restaurant(johnDoe, johnDoesGroup, bok2);

        var kowalskisGroup = new RestaurantGroup
        {
            Name = "Krzysztof Kowalski's Restaurant Group",
            OwnerId = kowalski.Id
        };

        context.RestaurantGroups.Add(kowalskisGroup);

        _ = await CreateKowalskisRestaurant(kowalski, kowalskisGroup, bok1);

        var anonGroup = new RestaurantGroup
        {
            Name = "Anon Ymus' Restaurant Group",
            OwnerId = anon.Id
        };

        context.RestaurantGroups.Add(anonGroup);

        await CreateAnonRestaurant(anon, anonGroup, bok1);
        await context.SaveChangesAsync();

        var geraltsGroup = new RestaurantGroup
        {
            Name = "Geralt's Restaurant Group",
            OwnerId = geralt.Id
        };

        context.RestaurantGroups.Add(geraltsGroup);

        await CreateWitcherRestaurant(geralt, geraltsGroup, bok1);

        var paulsGroup = new RestaurantGroup
        {
            Name = "Paul Muadib Atreides' Restaurant Group",
            OwnerId = muadib.Id
        };

        context.RestaurantGroups.Add(paulsGroup);

        await CreateAtreidesRestaurant(muadib, paulsGroup, bok1);

        var waltersGroup = new RestaurantGroup
        {
            Name = "Heisenberg's Restaurant Group",
            OwnerId = walter.Id
        };

        context.RestaurantGroups.Add(waltersGroup);

        await CreateBreakingBadRestaurant(walter, waltersGroup, bok1);

        await context.SaveChangesAsync();

        var visits = new List<Visit>
        {
            new Visit
            {
                Date = new DateTime(2024, 1, 1, 17, 0, 0),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 19, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = null,
                Takeaway = true,
                TableRestaurantId = 1,
                TableId = 1,
                ClientId = customer1.Id,
                Client = customer1,
                IsDeleted = false,
                Participants = [customer2, customer3],
                Restaurant = johnDoesGroup.Restaurants.ElementAt(0)
            },
            new Visit
            {
                Date = new DateTime(2024, 1, 4, 18, 0, 0),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 22, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = 10m,
                Takeaway = false,
                TableRestaurantId = 1,
                TableId = 2,
                ClientId = customer2.Id,
                Client = customer2,
                IsDeleted = false,
                Participants = [customer3],
                Restaurant = johnDoesGroup.Restaurants.ElementAt(0)
            },
            new Visit
            {
                Date = new DateTime(2024, 1, 5, 18, 0, 0),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 15, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = 25m,
                Takeaway = false,
                TableRestaurantId = 1,
                TableId = 1,
                ClientId = customer2.Id,
                Client = customer2,
                IsDeleted = false,
                Restaurant = johnDoesGroup.Restaurants.ElementAt(0)
            },
        };

        // Dodaj przykładowe wydarzenia dla restauracji John Doe
        context.Events.AddRange(
            new Event
            {
                CreatedAt = visits[0].Date.AddDays(-1),
                Description = "Event 1 Description",
                Time = visits[0].Date,
                MustJoinUntil = visits[0].Date.AddHours(-3),
                Creator = customer1,
                RestaurantId = 1,
                Visit = visits[0],
                Interested = [customer2, customer3]
            },
            new Event
            {
                CreatedAt = visits[1].Date.AddDays(-5),
                Description = "Event 2 Description",
                Time = visits[1].Date,
                MustJoinUntil = visits[1].Date.AddDays(-1),
                Creator = customer2,
                RestaurantId = 1,
                VisitId = null,
                Interested = [customer1]
            },
            new Event
            {
                CreatedAt = DateTime.UtcNow,
                Description = "Event 3 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(10),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(10).AddHours(-1),
                Creator = customer3,
                RestaurantId = 1,
                VisitId = null,
                Interested = [customer2]
            },
            new Event
            {
                CreatedAt = DateTime.UtcNow,
                Description = "Event 4 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(15),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(15).AddHours(-1),
                Creator = customer1,
                RestaurantId = 1,
                VisitId = null,
                Interested = []
            },
            new Event
            {
                CreatedAt = DateTime.UtcNow,
                Description = "Event 5 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(20),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(20).AddHours(-1),
                Creator = customer3,
                RestaurantId = 1,
                VisitId = null,
                Interested = [customer1, customer2]
            }
        );

        context.Visits.AddRange(visits);

        var orders = new List<Order>
        {
            new Order
            {
                VisitId = visits.First().Id,
                IsDeleted = false,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 3,
                        Status = OrderStatus.Taken,
                    }
                },
                Visit = visits.First()
            },
            new Order
            {
                VisitId = visits[1].Id,
                IsDeleted = false,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 1,
                        Status = OrderStatus.Cancelled,
                    }
                },
                Visit = visits[1]
            },
            new Order
            {
                VisitId = visits[2].Id,
                IsDeleted = false,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 1,
                        Status = OrderStatus.Taken,
                    },
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 2,
                        Status = OrderStatus.Taken,
                    }
                },
                Visit = visits[2]
            }
        };


        context.Orders.AddRange(orders);


        await context.SaveChangesAsync();
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

            if (userId is null)
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

                logger.LogInformation("Added example upload {} for user {} (Id: {})",
                    fileName, userLogin, userId);
            }
        }

        await context.SaveChangesAsync();
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

    private async Task CreateJohnDoesRestaurant(User johnDoe, RestaurantGroup johnDoesGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-jd.pdf", johnDoe);

        var johnDoes = new Restaurant
        {
            Name = "John Doe's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1231264550",
            Address = "ul. Marszałkowska 2",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.91364863552046,52.39625635)),
            Group = johnDoesGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo2.png", johnDoe),
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

        var visits = await context.Visits.ToListAsync();
        for (int i = 0; i< visits.Count; i++)
        {
            visits[i].Restaurant = johnDoes;
        }
        await context.SaveChangesAsync();


        johnDoes.Tables = new List<Table>
        {
            new()
            {
                Restaurant = johnDoes,
                Id = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes,
                Id = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes,
                Id = 4,
                Capacity = 6
            }
        };

        johnDoes.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = johnDoes,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", johnDoe)
            }
        };

        context.Restaurants.Add(johnDoes);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("pizza.png", johnDoe),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = johnDoes,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Pizza z mozzarellą",
                    Price = 39m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResPizza1.jpg", johnDoe)
                },
                new MenuItem
                {
                    Name = "Pizza z oliwami",
                    Price = 45m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResPizza2.jpg", johnDoe)
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("piwo.png", johnDoe),
            PhotoFileName = null!,
            MenuType = MenuType.Alcohol,
            Restaurant = johnDoes,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Piwo",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    Restaurant = johnDoes,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("piwo.png", johnDoe)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = johnDoes,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 1, 1),
                Contents = "Było bardzo smacznie, super obsługa, polecam",
            },
            new()
            {
                Restaurant = johnDoes,
                Author = customer1,
                Stars = 3,
                CreatedAt = new DateTime(2024, 5, 10),
                Contents = "Przeciętna ryba, średnia obsługa",
                RestaurantResponse = "Proponujemy następnym razem zamówić schabowego ;)"
            },
            new()
            {
                Restaurant = johnDoes,
                Author = customer3,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 22),
                Contents = "Genialnie!!!!! Wrócę na 100%!",
                RestaurantResponse = "Dziękujemy :)"
            },
        });

        await context.SaveChangesAsync();

        var hallEmployee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "hall",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Sali",
            LastName = "Przykładowski",
            BirthDate = DateOnly.Parse("2001-05-05"),
            PhoneNumber = "+48123456789"
        }, johnDoe, "22781e02-d83a-44ef-8cf4-735e95d9a0b2")).OrThrow();
        (await restaurantService.AddEmployeeAsync(
            new List<AddEmployeeRequest> {
                new AddEmployeeRequest
            {
                EmployeeId = hallEmployee.Id,
                IsBackdoorEmployee = false,
                IsHallEmployee = true
            }
            },
            johnDoes.Id,
            johnDoe.Id)).OrThrow();

        var backdoorEmployee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "backdoors",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Zaplecza",
            LastName = "Przykładowski",
            BirthDate = new DateOnly(2001, 5, 5),
            PhoneNumber = "+48123456789"
        }, johnDoe, "06c12721-e59e-402f-aafb-2b43a4dd23f2")).OrThrow();
        (await restaurantService.AddEmployeeAsync(
            new List<AddEmployeeRequest> {
                new AddEmployeeRequest
            {
                EmployeeId = backdoorEmployee.Id,
                IsBackdoorEmployee = true,
                IsHallEmployee = false
            }
            },
            johnDoes.Id,
            johnDoe.Id)).OrThrow();
    }

    private async Task CreateJohnDoes2Restaurant(User johnDoe, RestaurantGroup johnDoesGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-jd.pdf", johnDoe);

        var johnDoes2 = new Restaurant
        {
            Name = "John Doe's 2",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Koszykowa 10",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.022417021601285, 52.221019850000005)),
            Group = johnDoesGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo2.png", johnDoe),
            ProvideDelivery = false,
            Description = "Another example restaurant",
            Photos = [],
            Tags = context.RestaurantTags
                .Where(rt => rt.Name == "OnSite")
                .ToList(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };
        johnDoes2.Tables = new List<Table>
        {
            new()
            {
                Restaurant = johnDoes2,
                Id = 1,
                Capacity = 2
            },
            new()
            {
                Restaurant = johnDoes2,
                Id = 2,
                Capacity = 2
            },
            new()
            {
                Restaurant = johnDoes2,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes2,
                Id = 4,
                Capacity = 4
            }
        };
        context.Restaurants.Add(johnDoes2);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe 2",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("pierogi.png", johnDoe),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = johnDoes2,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Pierogi",
                    Price = 19m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes2,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("pierogi.png", johnDoe)
                },
                new MenuItem
                {
                    Name = "Sushi",
                    Price = 259m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes2,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResSushi1.jpg", johnDoe)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = johnDoes2,
                Author = customer3,
                Stars = 2,
                CreatedAt = new DateTime(2024, 5, 10),
                Contents = "Baaardzo średnio, myślałem, że będzie na poziomie restauracji numer 1 pana John Doe, a okazało się słabiutko",
            },
            new()
            {
                Restaurant = johnDoes2,
                Author = customer1,
                Stars = 1,
                CreatedAt = new DateTime(2024, 3, 11),
                Contents = "Kompletna porażka! Jedzenie zimne, kelner nieuprzejmy - fatalnie!",
            },
        });

        await context.SaveChangesAsync();

        var employee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "employee",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik 2",
            LastName = "Przykładowski",
            BirthDate = new DateOnly(2002, 1, 1),
            PhoneNumber = "+48123456789"
        }, johnDoe, "f1b1b494-85f2-4dc7-856d-d04d1ce50d65")).OrThrow();
        (await restaurantService.AddEmployeeAsync(
            new List<AddEmployeeRequest> {
                new AddEmployeeRequest
            {
                EmployeeId = employee.Id,
                IsBackdoorEmployee = true,
                IsHallEmployee = true
            } },
            johnDoes2.Id,
            johnDoe.Id)).OrThrow();
    }

    private async Task<Restaurant> CreateKowalskisRestaurant(User kowalski, RestaurantGroup kowalskisGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-kk.pdf", kowalski);

        var kowalskisRestaurant = new Restaurant
        {
            Name = "Kowalski's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Konstruktorska 5",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.99866252013997,  52.1853141)),
            Group = kowalskisGroup,
            RentalContractFileName = null,
            RentalContract = exampleDocument,
            AlcoholLicenseFileName = null,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo4.png", kowalski),
            ProvideDelivery = false,
            Description = "Fake restaurant",
            Photos = [],
            Tags = context.RestaurantTags
                .Where(rt => rt.Name == "Asian" || rt.Name == "Takeaway")
                .ToList(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };
        kowalskisRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = kowalskisRestaurant,
                Id = 1,
                Capacity = 3
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Id = 2,
                Capacity = 2
            },
        };

        context.Restaurants.Add(kowalskisRestaurant);
        context.Menus.Add(new Menu
        {
            Name = "Menu jedzenie",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,

            Photo = await RequireFileUpload("menu.png", kowalski),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = kowalskisRestaurant,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Pad thai",
                    Price = 29m,
                    AlcoholPercentage = null,
                    Restaurant = kowalskisRestaurant,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("padthai.png", kowalski)
                },
                new MenuItem
                {
                    Name = "Ryż smażony",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = kowalskisRestaurant,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("restaurantboss3.PNG", kowalski)
                },
                new MenuItem
                {
                    Name = "Udon",
                    Price = 35m,
                    AlcoholPercentage = null,
                    Restaurant = kowalskisRestaurant,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResSushi2.jpg", kowalski)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 5,
                CreatedAt = new DateTime(2024, 3, 11),
                Contents = "Fantastyczne jedzenie i wspaniała obsługa! Na pewno wrócę.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 5,
                CreatedAt = new DateTime(2024, 3, 20),
                Contents = "Najlepsza restauracja w mieście! Wszystko było perfekcyjne",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 5,
                CreatedAt = new DateTime(2024, 3, 25),
                Contents = "Rewelacyjne doświadczenie kulinarne. Na pewno tu wrócę!",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 4,
                CreatedAt = new DateTime(2024, 5, 18),
                Contents = "Bardzo dobre jedzenie, choć niektóre dania były zbyt przyprawione.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 3,
                CreatedAt = new DateTime(2024, 6, 1),
                Contents = "Atmosfera przyjemna, ale jedzenie mogłoby być lepsze.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 3,
                CreatedAt = new DateTime(2024, 6, 5),
                Contents = "Nic specjalnego, ale też nie było źle. Średnia restauracja.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 2,
                CreatedAt = new DateTime(2024, 6, 28),
                Contents = "Jedzenie poniżej oczekiwań i zbyt głośno. Nie polecam.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 1,
                CreatedAt = new DateTime(2024, 7, 11),
                Contents = "Totalna porażka! Wszystko było nie tak, jak powinno.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer1,
                Stars = 1,
                CreatedAt = new DateTime(2024, 7, 20),
                Contents = "Najgorsza restauracja, w jakiej byłem. Nic tu nie działało jak należy.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 2,
                CreatedAt = new DateTime(2024, 3, 15),
                Contents = "Słabe jedzenie i nieprzyjemna obsługa. Nie polecam.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 2,
                CreatedAt = new DateTime(2024, 4, 20),
                Contents = "Zbyt długi czas oczekiwania na jedzenie i nieuprzejma obsługa.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 3,
                CreatedAt = new DateTime(2024, 6, 1),
                Contents = "Nic specjalnego, ale też nie było źle. Średnia restauracja.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 1),
                Contents = "Przepyszne dania i fantastyczny wystrój. Warto odwiedzić!",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 5),
                Contents = "Rewelacyjne doświadczenie kulinarne. Na pewno tu wrócę!",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 15),
                Contents = "Najlepsza restauracja w mieście! Wszystko było perfekcyjne.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer3,
                Stars = 4,
                CreatedAt = new DateTime(2024, 5, 10),
                Contents = "Bardzo smaczne potrawy, choć trochę drogie. Atmosfera na plus.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer3,
                Stars = 4,
                CreatedAt = new DateTime(2024, 6, 10),
                Contents = "Bardzo dobre jedzenie, choć niektóre dania były zbyt przyprawione.",
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                Author = customer3,
                Stars = 4,
                CreatedAt = new DateTime(2024, 7, 10),
                Contents = "Smaczne dania i miła obsługa, choć wystrój mógłby być lepszy.",
            },
        });

        await context.SaveChangesAsync();

        return kowalskisRestaurant;
    }
    private async Task CreateAnonRestaurant(User anon, RestaurantGroup anonsGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-AY.pdf", anon);

        var anons = new Restaurant
        {
            Name = "Anon's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1544832204",
            Address = "ul. Nowogrodzka 47a",
            PostalIndex = "00-695",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.008140, 52.227730)),
            Group = anonsGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("sushi.png", anon),
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

        var visits = await context.Visits.ToListAsync();
        for (int i = 0; i < visits.Count; i++)
        {
            visits[i].Restaurant = anons;
        }
        await context.SaveChangesAsync();


        anons.Tables = new List<Table>
        {
            new()
            {
                Restaurant = anons,
                Id = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = anons,
                Id = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = anons,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = anons,
                Id = 4,
                Capacity = 6
            }
        };

        anons.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = anons,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("human4.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 2,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("human5.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 3,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("owner1.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 4,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResBurger1.jpg", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 5,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResBurger2.jpg", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 6,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside1.jpg", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 7,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside2.jpg", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 8,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResLogo1.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 9,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("sushi.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 10,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("wege.png", anon)
            },
            new()
            {
                Restaurant = anons,
                Order = 11,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("woda.png", anon)
            }
        };

        context.Restaurants.Add(anons);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("wege.png", anon),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = anons,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Burger",
                    Price = 20m,
                    AlcoholPercentage = null,
                    Restaurant = anons,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResBurger1.jpg", anon)
                },
                new MenuItem
                {
                    Name = "Cheeseburger",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = anons,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ResBurger2.jpg", anon)
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("woda.png", anon),
            PhotoFileName = null!,
            MenuType = MenuType.Alcohol,
            Restaurant = anons,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Piwo",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    Restaurant = anons,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("woda.png", anon)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = anons,
                Author = customer1,
                Stars = 4,
                CreatedAt = new DateTime(2024, 7, 1),
                Contents = "Bardzo smaczne potrawy, chociaż deser niczego nie urwał. Atmosfera super.",
            },
            new()
            {
                Restaurant = anons,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 17),
                Contents = "Wyśmienite jedzenie i obsługa na najwyższym poziomie. Absolutnie polecam!",
            },
            new()
            {
                Restaurant = anons,
                Author = customer3,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 20),
                Contents = "Perfekcyjna kolacja! Wszystko było smaczne, a obsługa wyjątkowo pomocna.",
            },
        });

        await context.SaveChangesAsync();
    }

    private async Task CreateWitcherRestaurant(User geralt, RestaurantGroup geraltsGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-GR.pdf", geralt);

        var geralts = new Restaurant
        {
            Name = "Witcher's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "7967049012",
            Address = "Al. Jerozolimskie 65/79",
            PostalIndex = "00-697",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.003630, 52.227690)),
            Group = geraltsGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo3.png", geralt),
            ProvideDelivery = true,
            Description = "The third example restaurant",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

        var visits = await context.Visits.ToListAsync();
        for (int i = 0; i < visits.Count; i++)
        {
            visits[i].Restaurant = geralts;
        }
        await context.SaveChangesAsync();


        geralts.Tables = new List<Table>
        {
            new()
            {
                Restaurant = geralts,
                Id = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = geralts,
                Id = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = geralts,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = geralts,
                Id = 4,
                Capacity = 6
            }
        };

        geralts.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = geralts,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("owner2.png", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 2,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("owner3.png", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 3,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("owner5.png", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 4,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ramen.png", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 5,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside3.jpg", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 6,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside4.jpg", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 7,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResKebab1.jpg", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 8,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResKebab2.jpg", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 9,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("saladki.png", geralt)
            },
            new()
            {
                Restaurant = geralts,
                Order = 10,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("stek.png", geralt)
            }
        };

        context.Restaurants.Add(geralts);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("ramen.png", geralt),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = geralts,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Ramen",
                    Price = 20m,
                    AlcoholPercentage = null,
                    Restaurant = geralts,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("ramen.png", geralt)
                },
                new MenuItem
                {
                    Name = "Stek",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = geralts,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("stek.png", geralt)
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("owner2.png", geralt),
            PhotoFileName = null!,
            MenuType = MenuType.Alcohol,
            Restaurant = geralts,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Piwo",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    Restaurant = geralts,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("owner2.png", geralt)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = geralts,
                Author = customer1,
                Stars = 3,
                CreatedAt = new DateTime(2024, 7, 19),
                Contents = "Średnie jedzenie, ale przyjemna atmosfera. Może wrócę spróbować innych dań.",
            },
            new()
            {
                Restaurant = geralts,
                Author = customer3,
                Stars = 4,
                CreatedAt = new DateTime(2024, 7, 22),
                Contents = "Dobre jedzenie, ale niektóre potrawy były trochę zbyt słone. Ogólnie pozytywnie.",
            },
        });

        await context.SaveChangesAsync();
    }

    private async Task CreateAtreidesRestaurant(User paul, RestaurantGroup atreidesGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-PA.pdf", paul);

        var atreides = new Restaurant
        {
            Name = "Dune's spices",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "9322527232",
            Address = "Świętokrzyska 18",
            PostalIndex = "00-052",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.011500, 52.236060)),
            Group = atreidesGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("human1.png", paul),
            ProvideDelivery = true,
            Description = "The fourth example restaurant. LISAN AL-GHAIB",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

        var visits = await context.Visits.ToListAsync();
        for (int i = 0; i < visits.Count; i++)
        {
            visits[i].Restaurant = atreides;
        }
        await context.SaveChangesAsync();


        atreides.Tables = new List<Table>
        {
            new()
            {
                Restaurant = atreides,
                Id = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = atreides,
                Id = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = atreides,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = atreides,
                Id = 4,
                Capacity = 6
            }
        };

        atreides.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = atreides,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("human1.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 2,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("human2.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 3,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("human3.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 4,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("kurczak.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 5,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("makarony.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 6,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("meksykanskie.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 7,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside7.jpg", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 8,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResLogo5.png", paul)
            },
            new()
            {
                Restaurant = atreides,
                Order = 9,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResVegan1.jpg", paul)
            }
        };

        context.Restaurants.Add(atreides);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("makarony.png", paul),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = atreides,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Chicken",
                    Price = 20m,
                    AlcoholPercentage = null,
                    Restaurant = atreides,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("kurczak.png", paul)
                },
                new MenuItem
                {
                    Name = "pasta",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = atreides,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("makarony.png", paul)
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("human2.png", paul),
            PhotoFileName = null!,
            MenuType = MenuType.Alcohol,
            Restaurant = atreides,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Piwo",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    Restaurant = atreides,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("human2.png", paul)
                }
            ]
        });

        await context.SaveChangesAsync();
    }

    private async Task CreateBreakingBadRestaurant(User walter, RestaurantGroup whitesGroup, User verifier)
    {
        var exampleDocument = await RequireFileUpload("test-WW.pdf", walter);

        var walters = new Restaurant
        {
            Name = "Heisenberg's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "3419686135",
            Address = "Al. Jerozolimskie 65/79",
            PostalIndex = "00-697",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.003630, 52.227690)),
            Group = whitesGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResVegan2.jpg", walter),
            ProvideDelivery = true,
            Description = "The last example restaurant. Got the purest meth on the market.",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

        var visits = await context.Visits.ToListAsync();
        for (int i = 0; i < visits.Count; i++)
        {
            visits[i].Restaurant = walters;
        }
        await context.SaveChangesAsync();


        walters.Tables = new List<Table>
        {
            new()
            {
                Restaurant = walters,
                Id = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = walters,
                Id = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = walters,
                Id = 3,
                Capacity = 4
            },
            new()
            {
                Restaurant = walters,
                Id = 4,
                Capacity = 6
            }
        };

        walters.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = walters,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("burger.png", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 2,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("drinki.png", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 3,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("kebab.png", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 4,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside8.jpg", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 5,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("restaurantboss4.PNG", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 6,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("restaurantBossUltimate.png", walter)
            },
            new()
            {
                Restaurant = walters,
                Order = 7,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResVegan2.jpg", walter)
            }
        };

        context.Restaurants.Add(walters);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("kebab.png", walter),
            PhotoFileName = null!,
            MenuType = MenuType.Food,
            Restaurant = walters,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "burger",
                    Price = 20m,
                    AlcoholPercentage = null,
                    Restaurant = walters,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("burger.png", walter)
                },
                new MenuItem
                {
                    Name = "kebab",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = walters,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("kebab.png", walter)
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            Photo = await RequireFileUpload("drinki.png", walter),
            PhotoFileName = null!,
            MenuType = MenuType.Alcohol,
            Restaurant = walters,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Drinki",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    Restaurant = walters,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload("drinki.png", walter)
                }
            ]
        });

        var customer1 = await context.Users.FirstAsync(u => u.UserName == "customer");
        var customer2 = await context.Users.FirstAsync(u => u.UserName == "customer2");
        var customer3 = await context.Users.FirstAsync(u => u.UserName == "customer3");

        context.Reviews.AddRange(new List<Review>
        {
            new()
            {
                Restaurant = walters,
                Author = customer1,
                Stars = 4,
                CreatedAt = new DateTime(2024, 7, 10),
                Contents = "Fajne jedzenie, ale z zaplecza wydobywał się dziwny niebieski dym... podejrzane",
            },
            new()
            {
                Restaurant = walters,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 10),
                Contents = "ŚWIETNIE!! Kucharz zaoferował też \"specjalny\", niebieski deser;))",
            },
            new()
            {
                Restaurant = walters,
                Author = customer2,
                Stars = 5,
                CreatedAt = new DateTime(2024, 7, 10),
                Contents = "Super restauracja, okazało się że właściciel uczył mnie chemii. Może zapytam czy ma jakiś pomysł na rozwinięcie biznesu..",
            },
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates visit in the future
    /// </summary>
    public async Task<VisitSummaryVM> AddFutureVisitAsync()
    {
        var exampleCustomer = await context.Users.FirstAsync(u => u.UserName == "customer");

        var visitResult = (await visitService.CreateVisitAsync(
            new CreateVisitRequest
            {
                Date = DateTime.UtcNow.AddDays(1),
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
