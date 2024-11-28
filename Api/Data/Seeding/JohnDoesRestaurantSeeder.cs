using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Restaurant seeder for John Doe's
/// </summary>
public class JohnDoesRestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
    ) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 1;

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
        var verifier = users.CustomerSupportAgent;
        var exampleDocument = await RequireFileUpload("test-jd.pdf");

        return new Restaurant
        {
            Name = "John Doe's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1231264550",
            Address = "ul. Marszałkowska 2",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.91364863552046, 52.39625635)),
            Group = CreateRestaurantGroup(owner),
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            MaxReservationDurationMinutes = 120,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo2.png"),
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Tags = await RequireRestaurantTags(["OnSite", "Takeaway"]),
            VerifierId = verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(22, 00),
                new TimeOnly(10, 00), new TimeOnly(23, 00)),
            Tables = new List<Table>
            {
                new() { TableId = 1, Capacity = 4, },
                new() { TableId = 2, Capacity = 4, },
                new() { TableId = 3, Capacity = 6, },
                new() { TableId = 4, Capacity = 2, },
            },
            Photos = await RequireRestaurantPhotos("ResInside5.jpg"),
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
    [
        new Ingredient
        {
            PublicName = "Dough",
            UnitOfMeasurement = UnitOfMeasurement.Gram,
            Corrections = [
                new IngredientAmountCorrection
                {
                    OldAmount = 1000,
                    NewAmount = 950,
                    CorrectionDate = DateTime.UtcNow.AddDays(-12),
                    User = RestaurantOwner,
                    Comment = "Adjusted inventory after delivery",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 950,
                    NewAmount = 900,
                    CorrectionDate = DateTime.UtcNow.AddDays(-11),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Used for special catering order",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 900,
                    NewAmount = 850,
                    CorrectionDate = DateTime.UtcNow.AddDays(-10),
                    User = RestaurantOwner,
                    Comment = "Prepared extra dough for weekend rush",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 850,
                    NewAmount = 800,
                    CorrectionDate = DateTime.UtcNow.AddDays(-9),
                    User = RestaurantOwner,
                    Comment = "Adjusted inventory after spoilage",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 800,
                    NewAmount = 750,
                    CorrectionDate = DateTime.UtcNow.AddDays(-8),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Used for testing new recipe",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 750,
                    NewAmount = 700,
                    CorrectionDate = DateTime.UtcNow.AddDays(-7),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Prepared dough for special event",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 700,
                    NewAmount = 650,
                    CorrectionDate = DateTime.UtcNow.AddDays(-6),
                    User = RestaurantOwner,
                    Comment = "Adjusted inventory after staff meal",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 650,
                    NewAmount = 600,
                    CorrectionDate = DateTime.UtcNow.AddDays(-5),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Used for charity event",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 600,
                    NewAmount = 550,
                    CorrectionDate = DateTime.UtcNow.AddDays(-4),
                    User = RestaurantOwner,
                    Comment = "Prepared dough for school workshop",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 550,
                    NewAmount = 500,
                    CorrectionDate = DateTime.UtcNow.AddDays(-3),
                    User = RestaurantOwner,
                    Comment = "Adjusted inventory after stocktake",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 500,
                    NewAmount = 450,
                    CorrectionDate = DateTime.UtcNow.AddDays(-2),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Used for experimental dish",
                },
                new IngredientAmountCorrection
                {
                    OldAmount = 450,
                    NewAmount = 400,
                    CorrectionDate = DateTime.UtcNow.AddDays(-1),
                    User = FindEmployeeByLogin("backdoors"),
                    Comment = "Prepared dough for family gathering",
                },
            ],
        }, // 0
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
            Name = "Classic Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Margherita"),
                CreateRandomMenuItem("Pepperoni"),
                CreateRandomMenuItem("Four Seasons"),
                CreateRandomMenuItem("Vegeterian"),
                CreateRandomMenuItem("Mexican"),
            ],
        },

        new Menu
        {
            Name = "Specialty Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Hawaiian"),
                CreateRandomMenuItem("BBQ Chicken"),
                CreateRandomMenuItem("Meat Lovers"),
                CreateRandomMenuItem("Seafood Delight"),
                CreateRandomMenuItem("Quattro Formaggi"),
            ],
        },
        new Menu
        {
            Name = "Vegan Pizzas",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Vegan Delight"),
                CreateRandomMenuItem("Vegan Margherita"),
            ],
        },
        new Menu
        {
            Name = "Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Coca-Cola"),
                CreateRandomMenuItem("Orange Juice"),
                CreateRandomMenuItem("Mineral Water"),
            ],
        },
        new Menu
        {
            Name = "Alcoholic Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            MenuItems = [
                CreateRandomMenuItem("Beer", 5m),
                CreateRandomMenuItem("Red Wine", 12m),
                CreateRandomMenuItem("White Wine", 12m),
            ],
        },
    ];

    /// <inheritdoc />
    protected async override Task<List<User>> CreateEmployees() =>
    [
        await CreateRestaurantEmployee(
            "hall", "Pracownik Sali", "Przykładowski",
            "22781e02-d83a-44ef-8cf4-735e95d9a0b2", true, false),
        await CreateRestaurantEmployee(
            "backdoors", "Pracownik Zaplecza", "Przykładowski",
            "06c12721-e59e-402f-aafb-2b43a4dd23f2", true, false),
    ];
}