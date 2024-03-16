using Microsoft.AspNetCore.Identity;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
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

        var johnDoe = (await userService.RegisterRestaurantOwnerAsync(new RegisterRestaurantOwnerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.pl",
            PhoneNumber = "+48123456789",
            Password = "Pa$$w0rd"
        })).OrThrow();

        context.Restaurants.Add(new Restaurant
        {
            Id = 1,
            Name = "John Doe's",
            Address = "Warszawa, ul. Marszałkowska 2",
            OwnerId = johnDoe.Id,
            Tables = [
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
            ]
        });

        await context.SaveChangesAsync();
    }
}