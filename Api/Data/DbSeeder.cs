using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Services;

namespace Reservant.Api.Data;

internal class DbSeeder(
    ApiDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserService userService)
{
    public async Task SeedDataAsync()
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Customer));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantOwner));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantEmployee));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantBackdoorsEmployee));
        await roleManager.CreateAsync(new IdentityRole(Roles.RestaurantHallEmployee));
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

        context.RestaurantGroups.Add(new RestaurantGroup
        {
            Id = 1,
            Name = "John Doe's Restaurant Group",
            OwnerId = johnDoe.Id
        });

        await CreateJohnDoesRestaurant(johnDoe);
        await CreateJohnDoes2Restaurant(johnDoe);

        (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "customer",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Customer",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        }, "e08ff043-f8d2-45d2-b89c-aec4eb6a1f29")).OrThrow();

        (await userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "support@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, "fced96c1-dad9-49ff-a598-05e1c5e433aa")).OrThrow();

        (await userService.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = "manager@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kierownik BOK",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            IsManager = true
        }, "3f97a9d9-21b5-40ae-b178-bfe071de723c")).OrThrow();

        await context.SaveChangesAsync();
    }

    private async Task CreateJohnDoesRestaurant(User johnDoe)
    {
        var johnDoes = new Restaurant
        {
            Id = 1,
            Name = "John Doe's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "000-00-00-000",
            Address = "ul. Marszałkowska 2",
            PostalIndex = "00-000",
            City = "Warszawa",
            GroupId = 1,
            RentalContractFileName = null!,
            RentalContract = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "rental-contract-1.pdf",
                ContentType = "application/pdf"
            },
            AlcoholLicenseFileName = null!,
            AlcoholLicense = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "alcohol-license-1.pdf",
                ContentType = "application/pdf"
            },
            BusinessPermissionFileName = null!,
            BusinessPermission = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "business-permission-1.pdf",
                ContentType = "application/pdf"
            },
            IdCardFileName = null!,
            IdCard = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "id-card-1.pdf",
                ContentType = "application/pdf"
            },
            Tables =
            [
                new Table
                {
                    RestaurantId = 1,
                    Id = 1,
                    Capacity = 4
                },
                new Table
                {
                    RestaurantId = 1,
                    Id = 2,
                    Capacity = 4
                },
                new Table
                {
                    RestaurantId = 1,
                    Id = 3,
                    Capacity = 4
                },
                new Table
                {
                    RestaurantId = 1,
                    Id = 4,
                    Capacity = 6
                }
            ],
            LogoFileName = null!,
            Logo = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "logo-1.png",
                ContentType = "image/png"
            },
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Photos = new List<RestaurantPhoto>
            {
                new()
                {
                    RestaurantId = 1,
                    Order = 1,
                    PhotoFileName = null!,
                    Photo = new FileUpload
                    {
                        UserId = johnDoe.Id,
                        FileName = "photo-1.png",
                        ContentType = "image/png"
                    }
                },
                new()
                {
                    RestaurantId = 1,
                    Order = 2,
                    PhotoFileName = null!,
                    Photo = new FileUpload
                    {
                        UserId = johnDoe.Id,
                        FileName = "photo-2.png",
                        ContentType = "image/png"
                    }
                },
                new()
                {
                    RestaurantId = 1,
                    Order = 3,
                    PhotoFileName = null!,
                    Photo = new FileUpload
                    {
                        UserId = johnDoe.Id,
                        FileName = "photo-3.png",
                        ContentType = "image/png"
                    }
                },
                new()
                {
                    RestaurantId = 1,
                    Order = 4,
                    PhotoFileName = null!,
                    Photo = new FileUpload
                    {
                        UserId = johnDoe.Id,
                        FileName = "photo-4.png",
                        ContentType = "image/png"
                    }
                },
                new()
                {
                    RestaurantId = 1,
                    Order = 5,
                    PhotoFileName = null!,
                    Photo = new FileUpload
                    {
                        UserId = johnDoe.Id,
                        FileName = "photo-5.png",
                        ContentType = "image/png"
                    }
                },
            },
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync()
        };
        context.Restaurants.Add(johnDoes);

        context.Menus.Add(new Menu
        {
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            RestaurantId = johnDoes.Id,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Burger",
                    Price = 20m,
                    AlcoholPercentage = null,
                    RestaurantId = johnDoes.Id,
                },
                new MenuItem
                {
                    Name = "Cheeseburger",
                    Price = 25m,
                    AlcoholPercentage = null,
                    RestaurantId = johnDoes.Id,
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            RestaurantId = johnDoes.Id,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Piwo",
                    Price = 8m,
                    AlcoholPercentage = 4.6m,
                    RestaurantId = johnDoes.Id,
                }
            ]
        });

        await context.SaveChangesAsync();

        (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "hall",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Sali",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            RestaurantId = johnDoes.Id,
            IsBackdoorEmployee = false,
            IsHallEmployee = true
        }, johnDoe, "22781e02-d83a-44ef-8cf4-735e95d9a0b2")).OrThrow();

        (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "backdoors",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Zaplecza",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            RestaurantId = johnDoes.Id,
            IsBackdoorEmployee = true,
            IsHallEmployee = false
        }, johnDoe, "06c12721-e59e-402f-aafb-2b43a4dd23f2")).OrThrow();
    }

    private async Task CreateJohnDoes2Restaurant(User johnDoe)
    {
        var johnDoes2 = new Restaurant
        {
            Id = 2,
            Name = "John Doe's 2",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "000-00-00-000",
            Address = "ul. Koszykowa 10",
            PostalIndex = "00-000",
            City = "Warszawa",
            GroupId = 1,
            RentalContractFileName = null!,
            RentalContract = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "rental-contract-2.pdf",
                ContentType = "application/pdf"
            },
            AlcoholLicenseFileName = null!,
            AlcoholLicense = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "alcohol-license-2.pdf",
                ContentType = "application/pdf"
            },
            BusinessPermissionFileName = null!,
            BusinessPermission = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "business-permission-2.pdf",
                ContentType = "application/pdf"
            },
            IdCardFileName = null!,
            IdCard = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "id-card-2.pdf",
                ContentType = "application/pdf"
            },
            Tables =
            [
                new Table
                {
                    RestaurantId = 2,
                    Id = 1,
                    Capacity = 2
                },
                new Table
                {
                    RestaurantId = 2,
                    Id = 2,
                    Capacity = 2
                },
                new Table
                {
                    RestaurantId = 2,
                    Id = 3,
                    Capacity = 4
                },
                new Table
                {
                    RestaurantId = 2,
                    Id = 4,
                    Capacity = 4
                }
            ],
            LogoFileName = null!,
            Logo = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "logo-2.png",
                ContentType = "image/png"
            },
            ProvideDelivery = false,
            Description = "Another example restaurant",
            Photos = [],
            Tags = context.RestaurantTags
                .Where(rt => rt.Name == "OnSite")
                .ToList()
        };
        context.Restaurants.Add(johnDoes2);

        context.Menus.Add(new Menu
        {
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            RestaurantId = johnDoes2.Id,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Kotlet schabowy",
                    Price = 19m,
                    AlcoholPercentage = null,
                    RestaurantId = johnDoes2.Id,
                },
                new MenuItem
                {
                    Name = "Zupa pomidorowa",
                    Price = 7m,
                    AlcoholPercentage = null,
                    RestaurantId = johnDoes2.Id,
                }
            ]
        });

        await context.SaveChangesAsync();

        (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "employee",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik 2",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789",
            RestaurantId = johnDoes2.Id,
            IsBackdoorEmployee = true,
            IsHallEmployee = true
        }, johnDoe, "f1b1b494-85f2-4dc7-856d-d04d1ce50d65")).OrThrow();
    }
}
