using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Seeder class for the "Los Pollos Hermanos" restaurant
/// </summary>
public class BreakingBadRestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 29;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() =>
    [
        "burger.png", "drinki.png", "kebab.png", "ResVegan2.jpg",
    ];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.WalterWhite;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-WW.pdf");
        var verifier = users.CustomerSupportAgent;

        return new Restaurant
        {
            Name = "Los Pollos Hermanos",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1234567890",
            Address = "ul. Albuquerque 23",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.924567, 52.403456)),
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
            Logo = await RequireFileUpload("restaurantboss4.PNG"),
            ProvideDelivery = true,
            Description = "A family-friendly restaurant specializing in fried chicken and classic American cuisine",
            Photos = await RequireRestaurantPhotos("ResInside8.jpg"),
            Tags = await RequireRestaurantTags("OnSite", "Takeaway"),
            VerifierId = verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(22, 00),
                new TimeOnly(10, 00), new TimeOnly(23, 00)),
            Tables =
            [
                new Table { Number = 1, Capacity = 4 },
                new Table { Number = 2, Capacity = 4 },
                new Table { Number = 3, Capacity = 6 },
                new Table { Number = 4, Capacity = 2 },
            ],
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
    [
        new Ingredient { PublicName = "Chicken", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 0
        new Ingredient { PublicName = "Flour", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 1
        new Ingredient { PublicName = "Eggs", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 2
        new Ingredient { PublicName = "Bread Crumbs", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
        new Ingredient { PublicName = "Potatoes", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 4
        new Ingredient { PublicName = "Lettuce", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
        new Ingredient { PublicName = "Tomatoes", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 6
        new Ingredient { PublicName = "Cheddar Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
        new Ingredient { PublicName = "Beef Patty", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 8
        new Ingredient { PublicName = "Burger Bun", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 9
        new Ingredient { PublicName = "Onions", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 10
        new Ingredient { PublicName = "Pickles", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
        new Ingredient { PublicName = "Ketchup", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 12
        new Ingredient { PublicName = "Mayonnaise", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 13
        new Ingredient { PublicName = "BBQ Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter } //14
    ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
        new Menu
        {
            Name = "Fried Chicken",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems =
            [
                CreateRandomMenuItem("Classic Chicken"),
                CreateRandomMenuItem("Spicy Fried Chicken"),
                CreateRandomMenuItem("Buttermilk Fried Chicken"),
                CreateRandomMenuItem("Honey Glazed Fried Chicken"),
                CreateRandomMenuItem("Garlic Parmesan Chicken"),
                CreateRandomMenuItem("Crispy Chicken Tenders"),
                CreateRandomMenuItem("Buffalo Fried Chicken"),
                CreateRandomMenuItem("BBQ Fried Chicken"),
                CreateRandomMenuItem("Lemon Pepper Fried Chicken"),
                CreateRandomMenuItem("Korean Fried Chicken"),
                CreateRandomMenuItem("Southern Fried Chicken"),
                CreateRandomMenuItem("Cajun Fried Chicken"),
                CreateRandomMenuItem("Fried Chicken Wings"),
                CreateRandomMenuItem("Fried Chicken Sandwich"),
                CreateRandomMenuItem("Chili Lime Fried Chicken"),
                CreateRandomMenuItem("Herb-Crusted Fried Chicken"),
                CreateRandomMenuItem("Fried Chicken with Gravy"),

            ],
        },
        new Menu
        {
            Name = "Burgers",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems =
            [
                CreateRandomMenuItem("Classic Beef Burger"),
                CreateRandomMenuItem("Chicken Burger"),
                CreateRandomMenuItem("Cheeseburger"),
                CreateRandomMenuItem("Bacon Burger"),
                CreateRandomMenuItem("Veggie Burger"),
                CreateRandomMenuItem("BBQ Bacon Burger"),
                CreateRandomMenuItem("Mushroom Swiss Burger"),
                CreateRandomMenuItem("Spicy Jalapeño Burger"),
                CreateRandomMenuItem("Fish Burger"),
                CreateRandomMenuItem("Double Cheeseburger"),
            ],
        },
        new Menu
        {
            Name = "Sides",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems =
            [
                CreateRandomMenuItem("French Fries"),
                CreateRandomMenuItem("Coleslaw"),
                CreateRandomMenuItem("Onion Rings"),
                CreateRandomMenuItem("Garlic Bread"),
                CreateRandomMenuItem("Mashed Potatoes"),
                CreateRandomMenuItem("Sweet Potato Fries"),
            ],
        },
        new Menu
        {
            Name = "Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems =
            [
                CreateRandomMenuItem("Soft Drink"),
                CreateRandomMenuItem("Iced Tea"),
                CreateRandomMenuItem("Iced Coffee"),
                CreateRandomMenuItem("Peach Iced Tea"),
            ],
        },
        new Menu
        {
            Name = "Alcoholic Beverages",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            MenuItems =
            [
                CreateRandomMenuItem("Beer", 5m),
                CreateRandomMenuItem("House Wine", 12m),
            ],
        },
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) =>
    [
        new Review
        {
            Author = users.Customer1,
            Stars = 4,
            CreatedAt = new DateTime(2024, 7, 10),
            Contents = "Fajne jedzenie, ale z zaplecza wydobywał się dziwny niebieski dym... podejrzane",
        },
        new Review
        {
            Author = users.Customer2,
            Stars = 5,
            CreatedAt = new DateTime(2024, 7, 10),
            Contents = "ŚWIETNIE!! Kucharz zaoferował też \"specjalny\", niebieski deser;))",
        },
        new Review
        {
            Author = users.Customer2,
            Stars = 5,
            CreatedAt = new DateTime(2024, 7, 10),
            Contents = "Super restauracja, okazało się że właściciel uczył mnie chemii. Może zapytam czy ma jakiś pomysł na rozwinięcie biznesu..",
        },
    ];
}
