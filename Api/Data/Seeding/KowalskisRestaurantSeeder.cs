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
                    CreateRandomMenuItem("Som Tum", "ส้มตำ"),
                    CreateRandomMenuItem("Pad See Ew", "ผัดซีอิ๊ว"),
                    CreateRandomMenuItem("Panang Curry", "พะแนง"),
                    CreateRandomMenuItem("Tom Kha Gai", "ต้มข่าไก่"),
                    CreateRandomMenuItem("Thai Basil Chicken", "ผัดกะเพราไก่"),
                    CreateRandomMenuItem("Mango Sticky Rice", "ข้าวเหนียวมะม่วง"),
                    CreateRandomMenuItem("Chicken Satay", "สะเต๊ะไก่"),
                    CreateRandomMenuItem("Larb", "ลาบ"),
                    CreateRandomMenuItem("Thai Fried Spring Rolls", "ปอเปี๊ยะทอด"),
                    CreateRandomMenuItem("Crispy Pork Belly with Kale", "คะน้าหมูกรอบ"),
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
                    CreateRandomMenuItem("Beef Udon"),
                    CreateRandomMenuItem("Sushi", "寿司"),
                    CreateRandomMenuItem("Ramen", "ラーメン"),
                    CreateRandomMenuItem("Tempura", "天ぷら"),
                    CreateRandomMenuItem("Udon", "うどん"),
                    CreateRandomMenuItem("Teriyaki Chicken", "照り焼きチキン"),
                    CreateRandomMenuItem("Tonkatsu", "豚カツ"),
                    CreateRandomMenuItem("Miso Soup", "味噌汁"),
                    CreateRandomMenuItem("Sashimi", "刺身"),
                    CreateRandomMenuItem("Okonomiyaki", "お好み焼き"),
                    CreateRandomMenuItem("Edamame", "枝豆"),
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
                    CreateRandomMenuItem("Chicken Tikka Masala", "चिकन टिक्का मसाला"),
                    CreateRandomMenuItem("Lamb Rogan Josh", "लंब रोज़ान जोश"),
                    CreateRandomMenuItem("Paneer Butter Masala", "पनीर बटर मसाला"),
                    CreateRandomMenuItem("Palak Paneer", "पालक पनीर"),
                    CreateRandomMenuItem("Biryani", "बिरयानी"),
                    CreateRandomMenuItem("Chole Bhature", "छोले भटूरे"),
                    CreateRandomMenuItem("Aloo Gobi", "आलू गोभी"),
                    CreateRandomMenuItem("Naan", "नान"),
                    CreateRandomMenuItem("Samosa", "समोसा"),
                    CreateRandomMenuItem("Dosa", "दोसा"),
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
                    CreateRandomMenuItem("Masala Chai"),
                    CreateRandomMenuItem("Lemon Water"),
                    CreateRandomMenuItem("Rose Lemonade"),
                    CreateRandomMenuItem("Sweet Lassi"),
                    CreateRandomMenuItem("Tamarind Juice"),
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
