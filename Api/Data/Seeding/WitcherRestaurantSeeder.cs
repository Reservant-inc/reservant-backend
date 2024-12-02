using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Seeder class for Witcher's restaurant
/// </summary>
public class WitcherRestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 30;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() =>
    [
        "ramen.png", "saladki.png", "stek.png", "ResKebab1.jpg", "ResKebab2.jpg",
    ];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.GeraltRiv;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-GR.pdf");
        var verifier = users.CustomerSupportManager;

        return new Restaurant
        {
            Name = "Kaer Morhen Inn",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "9876543210",
            Address = "ul. Stary Szlak 1",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.921482, 52.401038)),
            Group = CreateRestaurantGroup(owner),
            RentalContractFileName = null,
            RentalContract = exampleDocument,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            MaxReservationDurationMinutes = 120,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo3.png"),
            ProvideDelivery = false,
            Description = "A rustic inn serving hearty meals from the Witcher universe",
            Photos = await RequireRestaurantPhotos("ResInside4.jpg"),
            Tags = await RequireRestaurantTags("OnSite"),
            VerifierId = null,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(12, 00), new TimeOnly(22, 00),
                new TimeOnly(12, 00), new TimeOnly(23, 00)),
            Tables = new List<Table>
            {
                new() { TableId = 1, Capacity = 4 },
                new() { TableId = 2, Capacity = 6 },
                new() { TableId = 3, Capacity = 2 },
                new() { TableId = 4, Capacity = 8 },
            },
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
    [
        new Ingredient { PublicName = "Bread", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 0
        new Ingredient { PublicName = "Beef", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 1
        new Ingredient { PublicName = "Pork", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
        new Ingredient { PublicName = "Chicken", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
        new Ingredient { PublicName = "Potatoes", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 4
        new Ingredient { PublicName = "Carrots", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
        new Ingredient { PublicName = "Cabbage", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 6
        new Ingredient { PublicName = "Onions", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
        new Ingredient { PublicName = "Garlic", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 8
        new Ingredient { PublicName = "Mushrooms", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 9
        new Ingredient { PublicName = "Herbs", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 10
        new Ingredient { PublicName = "Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
        new Ingredient { PublicName = "Fish", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 12
        new Ingredient { PublicName = "Butter", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 13
        new Ingredient { PublicName = "Flour", UnitOfMeasurement = UnitOfMeasurement.Gram } //14
    ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
        new Menu
        {
            Name = "Hearty Meals",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Beef Stew"),
                CreateRandomMenuItem("Roasted Chicken"),
                CreateRandomMenuItem("Pork Sausages"),
                CreateRandomMenuItem("Fried Fish"),
            },
        },
        new Menu
        {
            Name = "Soups and Stews",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Mushroom Soup"),
                CreateRandomMenuItem("Vegetable Stew"),
            },
        },
        new Menu
        {
            Name = "Breads and Pies",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Freshly Baked Bread"),
                CreateRandomMenuItem("Meat Pie"),
            },
        },
        new Menu
        {
            Name = "Cheeses",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Cheese Platter"),
            },
        },
        new Menu
        {
            Name = "Alcoholic Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Mahakaman Mead"),
                CreateRandomMenuItem("Redanian Lager"),
                CreateRandomMenuItem("Toussaint Red Wine"),
            },
        },
    ];
}
