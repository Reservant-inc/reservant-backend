using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <inheritdoc />
public class ArrakisSpiceDinerSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
    ) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 4;

    /// <inheritdoc />
    protected override List<string> GetMenuItemPhotoFileNames() => ["makarony.png", "meksykanskie.png", "kurczak.png"];

    /// <inheritdoc />
    protected override User GetRestaurantOwner(UserSeeder users) => users.PaulAtreides;

    /// <inheritdoc />
    protected override async Task<Restaurant> CreateRestaurant(User owner, UserSeeder users)
    {
        var exampleDocument = await RequireFileUpload("test-PA.pdf");
        var verifier = users.CustomerSupportAgent;

        return new Restaurant
        {
            Name = "Arrakis Spice Diner",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "8765432109",
            Address = "ul. Pustynna 42",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(20.923456, 52.402345)),
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
            Logo = await RequireFileUpload("ResLogo5.png"),
            ProvideDelivery = true,
            Description = "A themed restaurant offering exotic dishes inspired by the Dune universe",
            Photos = await RequireRestaurantPhotos("ResInside7.jpg"),
            Tags = await RequireRestaurantTags("OnSite", "Takeaway"),
            VerifierId = verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(11, 00), new TimeOnly(22, 00),
                new TimeOnly(11, 00), new TimeOnly(23, 00)),
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
        new Ingredient { PublicName = "Spice", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 0
        new Ingredient { PublicName = "Sandworm Meat", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 1
        new Ingredient { PublicName = "Desert Herbs", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
        new Ingredient { PublicName = "Water", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 3
        new Ingredient { PublicName = "Fremen Bread", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 4
        new Ingredient { PublicName = "Date Palm Fruits", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
        new Ingredient { PublicName = "Cactus Juice", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 6
        new Ingredient { PublicName = "Desert Grains", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
        new Ingredient { PublicName = "Quail Eggs", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 8
        new Ingredient { PublicName = "Spiced Wine", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 9
        new Ingredient { PublicName = "Melange Sauce", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 10
        new Ingredient { PublicName = "Sietch Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
        new Ingredient { PublicName = "Desert Vegetables", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 12
        new Ingredient { PublicName = "Herbal Tea", UnitOfMeasurement = UnitOfMeasurement.Liter }, // 13
        new Ingredient { PublicName = "Stilgar's Spice Mix", UnitOfMeasurement = UnitOfMeasurement.Gram }, //14
    ];

    /// <inheritdoc />
    protected override List<Menu> CreateMenus() =>
    [
        new Menu
        {
            Name = "Fremen Delicacies",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Spice Sandworm"),
                CreateRandomMenuItem("Fremen Bread"),
                CreateRandomMenuItem("Sietch Stew"),
                CreateRandomMenuItem("Arrakis Roasted Lamb"),
                CreateRandomMenuItem("Dune Spice Soup"),
                CreateRandomMenuItem("Fremen Fish Tacos"),
                CreateRandomMenuItem("Shai-Hulud Shawarma"),
                CreateRandomMenuItem("Dune Desert Porridge"),
                CreateRandomMenuItem("Water of Life Cocktail"),
                CreateRandomMenuItem("Chalmun's Spice Cake"),
                CreateRandomMenuItem("Sardaukar Steak"),
                CreateRandomMenuItem("Harkonnen Hot Wings"),
            },
        },
        new Menu
        {
            Name = "Spice Sweets",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = new List<MenuItem>
            {
                CreateRandomMenuItem("Spice Cake"),
                CreateRandomMenuItem("Cinnamon Spice Cookies"),
                CreateRandomMenuItem("Gingerbread Truffles"),
                CreateRandomMenuItem("Chili Chocolate Mousse"),
                CreateRandomMenuItem("Cardamom Panna Cotta"),
                CreateRandomMenuItem("Turmeric Honeycomb"),
                CreateRandomMenuItem("Clove Caramel Flan"),
                CreateRandomMenuItem("Saffron Almond Baklava"),
                CreateRandomMenuItem("Pepper Spice Brownies"),
            },
        },
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) =>
    [
        new Review
        {
            Author = users.Customer1,
            Stars = 3,
            CreatedAt = new DateTime(2024, 7, 19),
            Contents = "Średnie jedzenie, ale przyjemna atmosfera. Może wrócę spróbować innych dań.",
        },
        new Review
        {
            Author = users.Customer3,
            Stars = 4,
            CreatedAt = new DateTime(2024, 7, 22),
            Contents = "Dobre jedzenie, ale niektóre potrawy były trochę zbyt słone. Ogólnie pozytywnie.",
        },
    ];
}
