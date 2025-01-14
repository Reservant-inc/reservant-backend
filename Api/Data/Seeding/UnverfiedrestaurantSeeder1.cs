using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <inheritdoc />
public class UnverfiedrestaurantSeeder1(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 9;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() =>
    ["ResBurger1.jpg", "ResBurger2.jpg", "sushi.png", "wege.png", "woda.png"];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.AnonYmus;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-AY.pdf");

        return new Restaurant
        {
            Name = "Unverified Restaurant 1",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "4564569978",
            Address = "ul. 123",
            PostalIndex = "00-002",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(21.00990467467737, 52.497394571933175)),
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
            Logo = await RequireFileUpload("ResLogo1.png"),
            ProvideDelivery = true,
            Description = "The firsr unverided example restaurant",
            Photos = await RequireRestaurantPhotos("ResInside1.jpg"),
            Tags = await RequireRestaurantTags("OnSite", "Takeaway"),
            VerifierId = null,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(22, 00),
                new TimeOnly(10, 00), new TimeOnly(23, 00)),
            Tables = new List<Table>
            {
                new()
                {
                    Number = 1,
                    Capacity = 4,
                },
                new()
                {
                    Number = 2,
                    Capacity = 4,
                },
                new()
                {
                    Number = 3,
                    Capacity = 6,
                },
                new()
                {
                    Number = 4,
                    Capacity = 2,
                },
            },
        };
    }

    /// <inheritdoc />
    protected override Ingredient[] CreateIngredients() =>
        [
            new Ingredient { PublicName = "Pasta", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 0
            new Ingredient { PublicName = "Tomato Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 1
            new Ingredient { PublicName = "Parmesan", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
            new Ingredient { PublicName = "Basil", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
            new Ingredient { PublicName = "Olive Oil", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 4
            new Ingredient { PublicName = "Garlic", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
            new Ingredient { PublicName = "Bread", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 6
            new Ingredient { PublicName = "Lettuce", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
            new Ingredient { PublicName = "Tomatoes", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 8
            new Ingredient { PublicName = "Cucumbers", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 9
            new Ingredient { PublicName = "Chicken Breast", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 10
            new Ingredient { PublicName = "Beef Patty", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
            new Ingredient { PublicName = "Cheddar Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 12
            new Ingredient { PublicName = "Bacon", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 13
            new Ingredient { PublicName = "Fries", UnitOfMeasurement = UnitOfMeasurement.Gram } // 14
        ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
        new Menu
        {
            Name = "Italian Cuisine",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Spaghetti Bolognese"),
                CreateRandomMenuItem("Penne Arrabiata"),
                CreateRandomMenuItem("Garlic Bread", "Chleb czosnkowy"),
                CreateRandomMenuItem("Margherita Pizza", "Pizza Margherita"),
                CreateRandomMenuItem("Lasagna", "Lasagne"),
                CreateRandomMenuItem("Carbonara Pasta", "Pasta alla Carbonara"),
                CreateRandomMenuItem("Seafood Risotto", "Risotto ai Frutti di Mare"),
                CreateRandomMenuItem("Stuffed Pasta Shells", "Conchiglioni Ripieni"),
                CreateRandomMenuItem("Chicken Parmigiana", "Pollo alla Parmigiana"),
                CreateRandomMenuItem("Minestrone Soup", "Minestrone"),
                CreateRandomMenuItem("Eggplant Parmesan", "Melanzane alla Parmigiana"),
                CreateRandomMenuItem("Tiramisu"),
                CreateRandomMenuItem("Focaccia Bread", "Focaccia"),
            },
        },
        new Menu
        {
            Name = "Salads",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Caesar Salad", "Salade César"),
                CreateRandomMenuItem("Greek Salad", "Χωριάτικη Σαλάτα"),
                CreateRandomMenuItem("Caprese Salad", "Insalata Caprese"),
                CreateRandomMenuItem("Nicoise Salad", "Sekude Niçoise"),
                CreateRandomMenuItem("Cobb Salad", "Ensalada Cobb"),
                CreateRandomMenuItem("Waldorf Salad", "Waldorfsalat"),
                CreateRandomMenuItem("Tabbouleh"),
                CreateRandomMenuItem("Fattoush"),
                CreateRandomMenuItem("Garden Salad", "Salata Verde"),
            },
        },
        new Menu
        {
            Name = "Burgers",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Classic Beef Burger"),
                CreateRandomMenuItem("Chicken Burger"),
                CreateRandomMenuItem("Bacon Burger"),
            },
        },
        new Menu
        {
            Name = "Sides",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("French Fries"),
                CreateRandomMenuItem("Onion Rings"),
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
                CreateRandomMenuItem("Craft Beer", 6m),
                CreateRandomMenuItem("House Wine", 10m),
            },
        },
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) => [];
}
