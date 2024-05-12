using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Services;

namespace Reservant.Api.Data;

public class DbSeeder(
    ApiDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserService userService,
    RestaurantService restaurantService)
{
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

        var johnDoesGroup = new RestaurantGroup
        {
            Name = "John Doe's Restaurant Group",
            OwnerId = johnDoe.Id
        };
        context.RestaurantGroups.Add(johnDoesGroup);

        await CreateJohnDoesRestaurant(johnDoe, johnDoesGroup, bok1);
        await CreateJohnDoes2Restaurant(johnDoe, johnDoesGroup, bok2);


        var kowalski = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            FirstName = "Krzysztof",
            LastName = "Kowalski",
            Login = "OK",
            Email = "krzysztof.kowalski@gmail.com",
            PhoneNumber = "+48999999999",
            Password = "Pa$$w0rd",
            BirthDate = new DateOnly(2002, 1, 1)
        })).OrThrow();

        await userService.MakeRestaurantOwnerAsync(kowalski.Id);

        var kowalskisGroup = new RestaurantGroup
        {
            Name = "Krzysztof Kowalski's Restaurant Group",
            OwnerId = kowalski.Id
        };

        context.RestaurantGroups.Add(kowalskisGroup);

        var kowalskisRestaurant = await CreateKowalskisRestaurant(kowalski, kowalskisGroup, bok1);

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

        var customer1 = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "cust1",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Ewa",
            LastName = "Przykładowska",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        })).OrThrow();

        var customer2 = (await userService.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = "cust2",
            Email = "customer@mail.com",
            Password = "Pa$$w0rd",
            FirstName = "Kacper",
            LastName = "Testowy",
            PhoneNumber = "+48123456789",
            BirthDate = new DateOnly(2000, 1, 1)
        })).OrThrow();

        var visits = new List<Visit>
        {
            new Visit
            {
                Date = new DateOnly(2024, 1, 1),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 19, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = null,
                Takeaway = true,
                TableRestaurantId = 3,
                TableId = 1,
                ClientId = customer1.Id,
                Client = customer1,
                IsDeleted = false,
            },
            new Visit
            {
                Date = new DateOnly(2024, 1, 1),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 22, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = 10m,
                Takeaway = false,
                TableRestaurantId = 3,
                TableId = 2,
                ClientId = customer2.Id,
                Client = customer2,
                IsDeleted = false,
            },
            new Visit
            {
                Date = new DateOnly(2024, 1, 1),
                NumberOfGuests = 1,
                PaymentTime = new DateTime(2024, 1, 1, 15, 32, 00),
                Deposit = null,
                ReservationDate = null,
                Tip = 25m,
                Takeaway = false,
                TableRestaurantId = 3,
                TableId = 1,
                ClientId = customer2.Id,
                Client = customer2,
                IsDeleted = false,
            },
        };

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
                        MenuItemId = 8,
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
                        MenuItemId = 6,
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
                        MenuItemId = 6,
                        Status = OrderStatus.Taken,
                    },
                    new OrderItem
                    {
                        Amount = 1,
                        MenuItemId = 7,
                        Status = OrderStatus.Taken,
                    }
                },
                Visit = visits[2]
            }
        };


        context.Orders.AddRange(orders);


        await context.SaveChangesAsync();
    }

    private async Task CreateJohnDoesRestaurant(User johnDoe, RestaurantGroup johnDoesGroup, User verifier)
    {
        var johnDoes = new Restaurant
        {
            Name = "John Doe's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1231264550",
            Address = "ul. Marszałkowska 2",
            PostalIndex = "00-000",
            City = "Warszawa",
            Group = johnDoesGroup,
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
            LogoFileName = null!,
            Logo = new FileUpload
            {
                UserId = johnDoe.Id,
                FileName = "logo-1.png",
                ContentType = "image/png"
            },
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Tags = await context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = verifier.Id,
            IsDeleted = false
        };

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
                Photo = new FileUpload
                {
                    UserId = johnDoe.Id,
                    FileName = "photo-1.png",
                    ContentType = "image/png"
                }
            },
            new()
            {
                Restaurant = johnDoes,
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
                Restaurant = johnDoes,
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
                Restaurant = johnDoes,
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
                Restaurant = johnDoes,
                Order = 5,
                PhotoFileName = null!,
                Photo = new FileUpload
                {
                    UserId = johnDoe.Id,
                    FileName = "photo-5.png",
                    ContentType = "image/png"
                }
            },
        };

        context.Restaurants.Add(johnDoes);

        context.Menus.Add(new Menu
        {
            Name = "Menu jedzeniowe",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            Restaurant = johnDoes,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Burger",
                    Price = 20m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes,
                },
                new MenuItem
                {
                    Name = "Cheeseburger",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes,
                }
            ]
        });

        context.Menus.Add(new Menu
        {
            Name = "Menu alkoholowe",
            DateFrom = new DateOnly(2024, 2, 1),
            DateUntil = null,
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
                }
            ]
        });

        await context.SaveChangesAsync();

        var hallEmployee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "hall",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Sali",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, johnDoe, "22781e02-d83a-44ef-8cf4-735e95d9a0b2")).OrThrow();
        await restaurantService.AddEmployeeAsync(
            new AddEmployeeRequest
            {
                Id = hallEmployee.Id,
                IsBackdoorEmployee = false,
                IsHallEmployee = true
            },
            johnDoes.Id,
            johnDoe.Id);

        var backdoorEmployee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "backdoors",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik Zaplecza",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, johnDoe, "06c12721-e59e-402f-aafb-2b43a4dd23f2")).OrThrow();
        await restaurantService.AddEmployeeAsync(
            new AddEmployeeRequest
            {
                Id = backdoorEmployee.Id,
                IsBackdoorEmployee = true,
                IsHallEmployee = false
            },
            johnDoes.Id,
            johnDoe.Id);
    }

    private async Task CreateJohnDoes2Restaurant(User johnDoe, RestaurantGroup johnDoesGroup, User verifier)
    {
        var johnDoes2 = new Restaurant
        {
            Name = "John Doe's 2",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Koszykowa 10",
            PostalIndex = "00-000",
            City = "Warszawa",
            Group = johnDoesGroup,
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
            MenuType = MenuType.Food,
            Restaurant = johnDoes2,
            MenuItems =
            [
                new MenuItem
                {
                    Name = "Kotlet schabowy",
                    Price = 19m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes2,
                },
                new MenuItem
                {
                    Name = "Zupa pomidorowa",
                    Price = 7m,
                    AlcoholPercentage = null,
                    Restaurant = johnDoes2,
                }
            ]
        });

        await context.SaveChangesAsync();

        var employee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = "employee",
            Password = "Pa$$w0rd",
            FirstName = "Pracownik 2",
            LastName = "Przykładowski",
            PhoneNumber = "+48123456789"
        }, johnDoe, "f1b1b494-85f2-4dc7-856d-d04d1ce50d65")).OrThrow();
        await restaurantService.AddEmployeeAsync(
            new AddEmployeeRequest
            {
                Id = employee.Id,
                IsBackdoorEmployee = true,
                IsHallEmployee = true
            },
            johnDoes2.Id,
            johnDoe.Id);
    }

    private async Task<Restaurant> CreateKowalskisRestaurant(User kowalski, RestaurantGroup kowalskisGroup, User verifier)
    {
        var kowalskisRestaurant = new Restaurant
        {
            Name = "Kowalski's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Konstruktorska 5",
            PostalIndex = "00-000",
            City = "Warszawa",
            Group = kowalskisGroup,
            RentalContractFileName = null!,
            RentalContract = new FileUpload
            {
                UserId = kowalski.Id,
                FileName = "kowalskis-rental-contract.pdf",
                ContentType = "application/pdf"
            },
            AlcoholLicenseFileName = null!,
            AlcoholLicense = new FileUpload
            {
                UserId = kowalski.Id,
                FileName = "kowalskis-alcohol-license.pdf",
                ContentType = "application/pdf"
            },
            BusinessPermissionFileName = null!,
            BusinessPermission = new FileUpload
            {
                UserId = kowalski.Id,
                FileName = "kowalskis-business-permission.pdf",
                ContentType = "application/pdf"
            },
            IdCardFileName = null!,
            IdCard = new FileUpload
            {
                UserId = kowalski.Id,
                FileName = "kowalskis-id-card.pdf",
                ContentType = "application/pdf"
            },
            LogoFileName = null!,
            Logo = new FileUpload
            {
                UserId = kowalski.Id,
                FileName = "kowalskis-logo.png",
                ContentType = "image/png"
            },
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
                },
                new MenuItem
                {
                    Name = "Ryż smażony",
                    Price = 25m,
                    AlcoholPercentage = null,
                    Restaurant = kowalskisRestaurant,
                },
                new MenuItem
                {
                    Name = "Udon",
                    Price = 35m,
                    AlcoholPercentage = null,
                    Restaurant = kowalskisRestaurant,
                }
            ]
        });

        await context.SaveChangesAsync();

        return kowalskisRestaurant;
    }
}
