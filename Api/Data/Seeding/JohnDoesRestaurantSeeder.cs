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
            Group = await CreateOrReuseRestaurantGroup(owner, "Original group"),
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
                new() { Number = 1, Capacity = 4, },
                new() { Number = 2, Capacity = 4, },
                new() { Number = 3, Capacity = 6, },
                new() { Number = 4, Capacity = 2, },
            },
            Photos = await RequireRestaurantPhotos(
                "ResInside5.jpg",
                "JohnDoesRestaurantPizza1.jpg",
                "JohnDoesRestaurantPizza2.jpg",
                "JohnDoesRestaurantPizza3.jpg",
                "JohnDoesRestaurantPizza4.jpg",
                "JohnDoesRestaurantPizza5.jpg"
                ),
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
    protected override async Task<List<User>> CreateEmployees() =>
    [
        await CreateRestaurantEmployee(
            "hall", "Pracownik Sali", "Przykładowski",
            "22781e02-d83a-44ef-8cf4-735e95d9a0b2", true, false),
        await CreateRestaurantEmployee(
            "backdoors", "Pracownik Zaplecza", "Przykładowski",
            "06c12721-e59e-402f-aafb-2b43a4dd23f2", false, true),
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) =>
    [
        new Review
        {
            Author = users.Customer2,
            Stars = 5,
            CreatedAt = new DateTime(2024, 1, 1),
            Contents = "Było bardzo smacznie, super obsługa, polecam",
        },
        new Review
        {
            Author = users.Customer1,
            Stars = 3,
            CreatedAt = new DateTime(2024, 5, 10),
            Contents = "Przeciętna ryba, średnia obsługa",
            RestaurantResponse = "Proponujemy następnym razem zamówić schabowego ;)"
        },
        new Review
        {
            Author = users.Customer3,
            Stars = 5,
            CreatedAt = new DateTime(2024, 7, 22),
            Contents = "Genialnie!!!!! Wrócę na 100%!",
            RestaurantResponse = "Dziękujemy :)"
        },
        new Review
        {
            Author = users.JohnDoe,
            Stars = 4,
            CreatedAt = new DateTime(2024, 9, 5),
            Contents = "Bardzo dobre jedzenie, ale czas oczekiwania mógłby być krótszy.",
            RestaurantResponse = "Dziękujemy za opinię! Postaramy się poprawić czas realizacji zamówień."
        },
        new Review
        {
            Author = users.AnonYmus,
            Stars = 5,
            CreatedAt = new DateTime(2024, 9, 10),
            Contents = "Najlepsza pizza w mieście! Gorąco polecam.",
            RestaurantResponse = "Cieszymy się, że smakowało! :)"
        },
        new Review
        {
            Author = users.GeraltRiv,
            Stars = 2,
            CreatedAt = new DateTime(2024, 9, 10),
            Contents = "Zimne jedzenie, długo czekałem na zamówienie.",
            RestaurantResponse = "Przykro nam, że nie spełniliśmy oczekiwań. Skontaktuj się z nami, abyśmy mogli to naprawić."
        },
        new Review
        {
            Author = users.KrzysztofKowalski,
            Stars = 3,
            CreatedAt = new DateTime(2024, 9, 18),
            Contents = "Jedzenie było dobre, ale porcje mogłyby być większe."
        },
        new Review
        {
            Author = users.PaulAtreides,
            Stars = 5,
            CreatedAt = new DateTime(2024, 9, 20),
            Contents = "Perfekcyjnie! Wszystko świeże i pyszne.",
            RestaurantResponse = "Dziękujemy! Zapraszamy ponownie!"
        },
        new Review
        {
            Author = users.WalterWhite,
            Stars = 4,
            CreatedAt = new DateTime(2024, 10, 2),
            Contents = "Dobra kawa i miła obsługa, ale miejsce trochę zatłoczone."
        },
        new Review
        {
            Author = users.Customers[17],
            Stars = 1,
            CreatedAt = new DateTime(2024, 10, 5),
            Contents = "Obsługa bardzo nieuprzejma, nie polecam.",
            RestaurantResponse = "Przykro nam to słyszeć. Możemy się skontaktować, by wyjaśnić sytuację?"
        },
        new Review
        {
            Author = users.Customers[3],
            Stars = 5,
            CreatedAt = new DateTime(2024, 10, 10),
            Contents = "Najlepszy deser, jaki jadłem!",
            RestaurantResponse = "Super, że smakowało! :)"
        },
        new Review
        {
            Author = users.Customers[4],
            Stars = 3,
            CreatedAt = new DateTime(2024, 10, 15),
            Contents = "Jedzenie dobre, ale kelner zapomniał o moim zamówieniu."
        },
        new Review
        {
            Author = users.Customers[5],
            Stars = 4,
            CreatedAt = new DateTime(2024, 10, 15),
            Contents = "Bardzo smacznie, ale ceny mogłyby być nieco niższe."
        },
        new Review
        {
            Author = users.Customers[6],
            Stars = 2,
            CreatedAt = new DateTime(2024, 10, 28),
            Contents = "Pizza przypalona, nie byłem zadowolony.",
            RestaurantResponse = "Przepraszamy za niedogodność! Chętnie poprawimy następnym razem."
        },
        new Review
        {
            Author = users.Customers[7],
            Stars = 5,
            CreatedAt = new DateTime(2024, 11, 3),
            Contents = "Idealne jedzenie i świetna atmosfera!",
            RestaurantResponse = "Bardzo dziękujemy! Do zobaczenia!"
        },
        new Review
        {
            Author = users.Customers[8],
            Stars = 3,
            CreatedAt = new DateTime(2024, 11, 8),
            Contents = "Porcje były nierówne, moje danie było mniejsze niż znajomego."
        },
        new Review
        {
            Author = users.Customers[9],
            Stars = 5,
            CreatedAt = new DateTime(2024, 11, 8),
            Contents = "Rewelacyjne dania! Wrócę na pewno.",
            RestaurantResponse = "Dziękujemy za miłe słowa!"
        },
        new Review
        {
            Author = users.Customers[10],
            Stars = 4,
            CreatedAt = new DateTime(2024, 11, 15),
            Contents = "Bardzo dobra obsługa, jedzenie smaczne, ale ceny wysokie."
        },
        new Review
        {
            Author = users.Customers[11],
            Stars = 1,
            CreatedAt = new DateTime(2024, 11, 20),
            Contents = "Nie dostałem zamówienia na czas. Totalna porażka.",
            RestaurantResponse = "Bardzo przepraszamy! Skontaktuj się z nami, postaramy się to wynagrodzić."
        },
        new Review
        {
            Author = users.Customers[12],
            Stars = 5,
            CreatedAt = new DateTime(2024, 12, 1),
            Contents = "Najlepsze jedzenie, jakie jadłem w tym roku!",
            RestaurantResponse = "Ogromnie się cieszymy! Zapraszamy częściej! :)"
        },
        new Review
        {
            Author = users.Customers[13],
            Stars = 3,
            CreatedAt = new DateTime(2024, 12, 5),
            Contents = "Dobre jedzenie, ale czas oczekiwania za długi."
        },
        new Review
        {
            Author = users.Customers[14],
            Stars = 4,
            CreatedAt = new DateTime(2024, 12, 10),
            Contents = "Bardzo miła obsługa i świetny klimat miejsca."
        },
        new Review
        {
            Author = users.Customers[15],
            Stars = 5,
            CreatedAt = new DateTime(2024, 12, 10),
            Contents = "Idealnie! Wszystko było doskonałe.",
            RestaurantResponse = "Dziękujemy! Do zobaczenia wkrótce!"
        },
        new Review
        {
            Author = users.Customers[16],
            Stars = 2,
            CreatedAt = new DateTime(2024, 12, 15),
            Contents = "Jakość jedzenia spadła w porównaniu do mojej poprzedniej wizyty.",
            RestaurantResponse = "Przykro nam to słyszeć. Możemy zapytać, co dokładnie nie spełniło Pana/Pani oczekiwań?"
        },
    ];
}
