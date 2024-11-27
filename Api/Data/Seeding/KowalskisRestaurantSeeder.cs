using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <inheritdoc />
public class KowalskisRestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
    ) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 2;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() => ["padthai.png", "ResSushi2.jpg", "menu.png"];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.KrzysztofKowalski;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-kk.pdf");
        var verifier = users.CustomerSupportAgent;

        return new Restaurant
        {
            Name = "Kowalski's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Konstruktorska 5",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.99866252013997, 52.1853141)),
            Group = CreateRestaurantGroup(owner),
            RentalContractFileName = null,
            RentalContract = exampleDocument,
            AlcoholLicenseFileName = null,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            IdCardFileName = null!,
            MaxReservationDurationMinutes = 120,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo4.png"),
            ProvideDelivery = false,
            Description = "Kowalski's Restaurant",
            Photos = new List<RestaurantPhoto>(),
            Tags = await RequireRestaurantTags("OnSite", "Takeaway"),
            VerifierId = verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(8, 00), new TimeOnly(18, 00),
                new TimeOnly(8, 00), new TimeOnly(16, 00)),
            Tables = new List<Table>
            {
                new() { TableId = 1, Capacity = 3, },
                new() { TableId = 2, Capacity = 2, },
                new() { TableId = 3, Capacity = 4, },
            },
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
    [
        new Ingredient { PublicName = "Rice", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 0
        new Ingredient { PublicName = "Tofu", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 1
        new Ingredient { PublicName = "Noodles", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
        new Ingredient { PublicName = "Chicken", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
        new Ingredient { PublicName = "Beef", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 4
        new Ingredient { PublicName = "Soy Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 5
        new Ingredient { PublicName = "Vegetables", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 6
        new Ingredient { PublicName = "Shrimp", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
        new Ingredient { PublicName = "Eggs", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 8
        new Ingredient { PublicName = "Curry Paste", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 9
        new Ingredient { PublicName = "Coconut Milk", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 10
        new Ingredient { PublicName = "Peanuts", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
        new Ingredient { PublicName = "Lime", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 12
        new Ingredient { PublicName = "Fish Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 13
        new Ingredient { PublicName = "Chili Peppers", UnitOfMeasurement = UnitOfMeasurement.Gram }, //14
    ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
            new Menu
            {
                Name = "Thai Specialties",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                MenuItems =
                [
                    CreateRandomMenuItem("Pad Thai"),
                    CreateRandomMenuItem("Green Curry"),
                    CreateRandomMenuItem("Massaman Curry"),
                    CreateRandomMenuItem("Tom Yum Soup"),
                    CreateRandomMenuItem("Pineapple Fried Rice"),

                ],
            },
            new Menu
            {
                Name = "Japanese Cuisine",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                MenuItems = new List<MenuItem>
                {
                    CreateRandomMenuItem("Sushi Set"),
                    CreateRandomMenuItem("Chicken Teriyaki"),
                    CreateRandomMenuItem("Ramen"),
                    CreateRandomMenuItem("Beef Udon"),
                },
            },
            new Menu
            {
                Name = "Indian Dishes",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                MenuItems = new List<MenuItem>
                {
                    CreateRandomMenuItem("Butter Chicken"),
                    CreateRandomMenuItem("Vegetable Curry"),
                },
            },
            new Menu
            {
                Name = "Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food, // Since only Food and Alcohol are available
                MenuItems = [
                    CreateRandomMenuItem("Green Tea"),
                    CreateRandomMenuItem("Mango Lassi"),
                ],
            },
            new Menu
            {
                Name = "Alcoholic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Alcohol,
                MenuItems = [
                    CreateRandomMenuItem("Sake", 15m),
                    CreateRandomMenuItem("Thai Beer", 12m),
                ],
            }
    ];
}
