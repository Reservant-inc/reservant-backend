using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <inheritdoc />
public class JohnDoes3RestaurantSeeder(
    ApiDbContext context,
    UserService userService,
    RestaurantService restaurantService,
    GeometryFactory geometryFactory
) : RestaurantSeeder(context, userService, restaurantService)
{
    /// <inheritdoc />
    protected override int RandomSeed => 3;

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
            Name = "John Doe's 3",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "św. Andrzeja Boboli 8",
            PostalIndex = "02-525",
            City = "Warszawa",
            Location = geometryFactory.CreatePoint(new Coordinate(52.203048958172225, 21.00094438861871)),
            Group = await CreateOrReuseRestaurantGroup(owner, "New group"),
            RentalContractFileName = null,
            AlcoholLicenseFileName = null,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            MaxReservationDurationMinutes = 120,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResSushi1.jpg"),
            ProvideDelivery = false,
            Description = "Third example restaurant",
            Photos = await RequireRestaurantPhotos(
                "ResInside5.jpg",
                "JohnDoesRestaurantPizza1.jpg",
                "JohnDoesRestaurantPizza2.jpg",
                "JohnDoesRestaurantPizza3.jpg",
                "JohnDoesRestaurantPizza4.jpg",
                "JohnDoesRestaurantPizza5.jpg"
                ),
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
            Name = "Przystawki",
            AlternateName = "Appetizers",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Bruschetta"),
                CreateRandomMenuItem("Garlic Bread"),
                CreateRandomMenuItem("Caprese Salad"),
                CreateRandomMenuItem("Mozzarella Sticks"),
                CreateRandomMenuItem("Stuffed Mushrooms"),
                CreateRandomMenuItem("Chicken Wings"),
                CreateRandomMenuItem("Shrimp Cocktail"),
                CreateRandomMenuItem("Spinach Dip"),
            ],
        },
        new Menu
        {
            Name = "Sałatki",
            AlternateName = "Salads",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Caesar Salad"),
                CreateRandomMenuItem("Greek Salad"),
                CreateRandomMenuItem("Cobb Salad"),
                CreateRandomMenuItem("Caprese Salad"),
                CreateRandomMenuItem("Garden Salad"),
                CreateRandomMenuItem("Waldorf Salad"),
                CreateRandomMenuItem("Spinach and Strawberry Salad"),
                CreateRandomMenuItem("Quinoa Salad"),
            ],
        },
        new Menu
        {
            Name = "Dania Główne",
            AlternateName = "Main Courses",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Grilled Chicken"),
                CreateRandomMenuItem("Beef Steak"),
                CreateRandomMenuItem("Salmon Fillet"),
                CreateRandomMenuItem("Lamb Chops"),
                CreateRandomMenuItem("Vegetarian Pasta"),
                CreateRandomMenuItem("Stuffed Peppers"),
                CreateRandomMenuItem("Seafood Paella"),
                CreateRandomMenuItem("BBQ Ribs"),
            ],
        },
        new Menu
        {
            Name = "Desery",
            AlternateName = "Desserts",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Cheesecake"),
                CreateRandomMenuItem("Tiramisu"),
                CreateRandomMenuItem("Chocolate Lava Cake"),
                CreateRandomMenuItem("Fruit Tart"),
                CreateRandomMenuItem("Apple Pie"),
                CreateRandomMenuItem("Ice Cream Sundae"),
                CreateRandomMenuItem("Panna Cotta"),
                CreateRandomMenuItem("Crème Brûlée"),
            ],
        },
        new Menu
        {
            Name = "Koktajle",
            AlternateName = "Cocktails",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Alcohol,
            MenuItems = [
                CreateRandomMenuItem("Mojito", 12m),
                CreateRandomMenuItem("Piña Colada", 14m),
                CreateRandomMenuItem("Cosmopolitan", 15m),
                CreateRandomMenuItem("Martini", 16m),
                CreateRandomMenuItem("Old Fashioned", 18m),
                CreateRandomMenuItem("Negroni", 17m),
            ],
        },
        new Menu
        {
            Name = "Kawa i Herbata",
            AlternateName = "Coffee & Tea",
            DateFrom = new DateOnly(2024, 1, 1),
            DateUntil = null,
            MenuType = MenuType.Food,
            MenuItems = [
                CreateRandomMenuItem("Espresso"),
                CreateRandomMenuItem("Cappuccino"),
                CreateRandomMenuItem("Latte"),
                CreateRandomMenuItem("Americano"),
                CreateRandomMenuItem("Macchiato"),
                CreateRandomMenuItem("Green Tea"),
                CreateRandomMenuItem("Black Tea"),
                CreateRandomMenuItem("Herbal Tea"),
            ],
        },
    ];

    /// <inheritdoc />
    protected override Review[] CreateReviews(UserSeeder users) =>
    [
        new Review
        {
            Author = users.AnonYmus,
            Stars = 1,
            CreatedAt = new DateTime(2024, 3, 18),
            Contents = "Tragedia! Zamówienie pomylone, długi czas oczekiwania i nieprzyjemna obsługa. Nigdy więcej!",
        },
        new Review
        {
            Author = users.Customer3,
            Stars = 3,
            CreatedAt = new DateTime(2024, 5, 20),
            Contents = "Jedzenie dobre, ale czas oczekiwania na zamówienie był zbyt długi. Obsługa mogłaby być bardziej uważna.",
        },
        new Review
        {
            Author = users.GeraltRiv,
            Stars = 4,
            CreatedAt = new DateTime(2024, 5, 25),
            Contents = "Bardzo dobre jedzenie, ale nie było dostępnych kilku pozycji z menu, co trochę popsuło wrażenie.",
        },
        new Review
        {
            Author = users.WalterWhite,
            Stars = 5,
            CreatedAt = new DateTime(2024, 6, 5),
            Contents = "Rewelacyjne jedzenie i przemiła obsługa. Idealne miejsce na romantyczną kolację!",
        },
        new Review
        {
            Author = users.PaulAtreides,
            Stars = 5,
            CreatedAt = new DateTime(2024, 6, 30),
            Contents = "Najlepsza restauracja w mieście! Wszystko było idealne – od jedzenia po obsługę!",
        },
        new Review
        {
            Author = users.Customer2,
            Stars = 5,
            CreatedAt = new DateTime(2024, 6, 15),
            Contents = "Fantastyczna restauracja! Świetne jedzenie, miła obsługa i przyjemna atmosfera. Na pewno wrócę!",
        },
        new Review
        {
            Author = users.Customer1,
            Stars = 4,
            CreatedAt = new DateTime(2024, 7, 1),
            Contents = "Bardzo smaczne dania, zwłaszcza pizza! Ceny mogłyby być trochę niższe, ale generalnie super miejsce.",
        },
        new Review
        {
            Author = users.KrzysztofKowalski,
            Stars = 2,
            CreatedAt = new DateTime(2024, 7, 1),
            Contents = "Nie jestem zadowolony. Zamówienie przyszło zimne, a kelner nie był zbyt pomocny.",
        },
    ];

    /// <inheritdoc />
    protected override async Task<List<User>> CreateEmployees() =>
    [
        await CreateRestaurantEmployee(
            "mateusz", "Mateusz", "Przykładowski",
            "475d0aea-5978-4165-bccf-a26e4c826418", true, false),
        await CreateRestaurantEmployee(
            "tomasz", "Tomasz", "Przykładowski",
            "434bcdfe-d147-4896-876d-2f4ea56ffc32", false, true),
    ];
}
