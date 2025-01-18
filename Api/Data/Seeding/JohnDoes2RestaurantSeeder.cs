using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <inheritdoc />
public class JohnDoes2RestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 2;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() =>
    [
        "pierogi.png", "piwo.png", "pizza.png", "ResPizza1.jpg", "ResPizza2.jpg", "ResSushi1.jpg",
    ];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.JohnDoe;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-jd.pdf");
        var verifier = users.CustomerSupportAgent;

        return new Restaurant
        {
            Name = "John Doe's 2",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Koszykowa 10",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.022417021601285, 52.221019850000005)),
            Group = await CreateOrReuseRestaurantGroup(owner, "New group"),
            RentalContractFileName = null,
            AlcoholLicenseFileName = null,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            MaxReservationDurationMinutes = 120,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo2.png"),
            ProvideDelivery = false,
            Description = "Another example restaurant",
            Photos = [],
            Tags = await RequireRestaurantTags("OnSite"),
            VerifierId = verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(20, 00),
                new TimeOnly(10, 00), new TimeOnly(18, 00)),
            Tables = new List<Table>
            {
                new() { Number = 1, Capacity = 2, },
                new() { Number = 2, Capacity = 2, },
                new() { Number = 3, Capacity = 4, },
                new() { Number = 4, Capacity = 4, },
            },
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
    [
        new Ingredient { PublicName = "Dough", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 0
        new Ingredient { PublicName = "Tomato Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 1
        new Ingredient { PublicName = "Mozzarella", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
        new Ingredient { PublicName = "Pepperoni", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
        new Ingredient { PublicName = "Basil", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 4
        new Ingredient { PublicName = "Olives", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
        new Ingredient { PublicName = "Mushrooms", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 6
        new Ingredient { PublicName = "Onions", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
        new Ingredient { PublicName = "Bell Peppers", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 8
        new Ingredient { PublicName = "Pineapple", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 9
        new Ingredient { PublicName = "Ham", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 10
        new Ingredient { PublicName = "Chicken", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
        new Ingredient { PublicName = "Beef", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 12
        new Ingredient { PublicName = "BBQ Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 13
        new Ingredient { PublicName = "Cheddar", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 14
    ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
        new Menu
        {
            Name = "Pizze Klasyczne",
            AlternateName = "Classic Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Margherita"),
                CreateRandomMenuItem("Pepperoni"),
                CreateRandomMenuItem("Four Seasons"),
                CreateRandomMenuItem("Vegetarian"),
                CreateRandomMenuItem("Mexican"),
                CreateRandomMenuItem("Cheese Lover"),
                CreateRandomMenuItem("Spicy Italian"),
                CreateRandomMenuItem("Mediterranean"),
                CreateRandomMenuItem("Truffle Mushroom"),
                CreateRandomMenuItem("Meat Feast"),
                CreateRandomMenuItem("Pesto Delight"),
                CreateRandomMenuItem("Garlic Supreme"),
                CreateRandomMenuItem("Seafood Special"),
                CreateRandomMenuItem("Smoky Bacon"),
            ],
        },
        new Menu
        {
            Name = "Pizze Specjalne",
            AlternateName = "Specialty Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Hawaiian"),
                CreateRandomMenuItem("BBQ Chicken"),
                CreateRandomMenuItem("Meat Lovers"),
                CreateRandomMenuItem("Seafood Delight"),
                CreateRandomMenuItem("Quattro Formaggi"),
                CreateRandomMenuItem("Veggie Supreme"),
                CreateRandomMenuItem("Buffalo Chicken"),
                CreateRandomMenuItem("Mediterranean"),
                CreateRandomMenuItem("Spicy Jalapeño"),
                CreateRandomMenuItem("Truffle Mushroom"),
                CreateRandomMenuItem("Pesto Chicken"),
                CreateRandomMenuItem("Philly Cheesesteak"),
                CreateRandomMenuItem("Margherita Deluxe"),
                CreateRandomMenuItem("Spinach & Artichoke"),
                CreateRandomMenuItem("Taco Fiesta"),
                CreateRandomMenuItem("Caprese"),
            ],
        },
        new Menu
        {
            Name = "Pizze Wegańskie",
            AlternateName = "Vegan Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Vegan Delight"),
                CreateRandomMenuItem("Vegan Margherita"),
                CreateRandomMenuItem("Spicy Vegan BBQ"),
                CreateRandomMenuItem("Mediterranean Veggie Vegan"),
                CreateRandomMenuItem("Vegan Pesto Paradise"),
                CreateRandomMenuItem("Mushroom & Spinach Vegan"),
                CreateRandomMenuItem("Roasted Veggie Vegan"),
            ],
        },
        new Menu
        {
            Name = "Napoje",
            AlternateName = "Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Coca-Cola"),
                CreateRandomMenuItem("Orange Juice"),
                CreateRandomMenuItem("Mineral Water"),
                CreateRandomMenuItem("Pepsi"),
                CreateRandomMenuItem("Lemonade"),
                CreateRandomMenuItem("Iced Tea"),
                CreateRandomMenuItem("Sparkling Water"),
                CreateRandomMenuItem("Apple Juice"),
                CreateRandomMenuItem("Cold Brew Coffee"),
                CreateRandomMenuItem("Ginger Ale"),
                CreateRandomMenuItem("Energy Drink"),
            ],
        },
        new Menu
        {
            Name = "Napoje Alkoholowe",
            AlternateName = "Alcoholic Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            MenuItems = [
                CreateRandomMenuItem("Beer", "Piwo", 5m),
                CreateRandomMenuItem("Red Wine", "Wino czerwone", 12m),
                CreateRandomMenuItem("White Wine", "Wino białe", 12m),
                CreateRandomMenuItem("Whiskey", 40m),
                CreateRandomMenuItem("Gin and Tonic", 12m),
                CreateRandomMenuItem("Margarita", 15m),
            ],
        },
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) =>
    [
        new Review
        {
            Author = users.Customer3,
            Stars = 2,
            CreatedAt = new DateTime(2024, 5, 10),
            Contents =
                "Baaardzo średnio, myślałem, że będzie na poziomie restauracji numer 1 pana John Doe, a okazało się słabiutko",
        },
        new Review
        {
            Author = users.Customer1,
            Stars = 1,
            CreatedAt = new DateTime(2024, 3, 11),
            Contents = "Kompletna porażka! Jedzenie zimne, kelner nieuprzejmy - fatalnie!",
        },
    ];

    /// <inheritdoc />
    protected override async Task<List<User>> CreateEmployees() =>
    [
        await CreateRestaurantEmployee(
            "arkadiusz", "Arkadiusz", "Przykładowski",
            "57b05d56-c9ad-4269-919d-6de7df80ebd2", true, true),
        await CreateRestaurantEmployee(
            "janusz", "Janusz", "Przykładowski",
            "c124588a-536a-481b-9882-85e097b8b0ba", false, true),
    ];
}
