using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Seeder class for Kowalski's restaurant
/// </summary>
public class KowalskisRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<KowalskisRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _kowalski;
    private readonly RestaurantGroup _kowalskisGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="KowalskisRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="kowalski">The owner user</param>
    /// <param name="kowalskisGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public KowalskisRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<KowalskisRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User kowalski,
        RestaurantGroup kowalskisGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _kowalski = kowalski;
        _kowalskisGroup = kowalskisGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for Kowalski's restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-kk.pdf", _kowalski);

        var kowalskisRestaurant = new Restaurant
        {
            Name = "Kowalski's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "0000000000",
            Address = "ul. Konstruktorska 5",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.99866252013997, 52.1853141)),
            Group = _kowalskisGroup,
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
            Logo = await RequireFileUpload("ResLogo4.png", _kowalski),
            ProvideDelivery = false,
            Description = "Kowalski's Restaurant",
            Photos = new List<RestaurantPhoto>(),
            Tags = await _context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = _verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(8, 00), new TimeOnly(18, 00),
                new TimeOnly(8, 00), new TimeOnly(16, 00),
                true),
        };

        // Create tables
        kowalskisRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = kowalskisRestaurant,
                TableId = 1,
                Capacity = 3
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                TableId = 2,
                Capacity = 2
            },
            new()
            {
                Restaurant = kowalskisRestaurant,
                TableId = 3,
                Capacity = 4
            }
        };

        // Create ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
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
            new Ingredient { PublicName = "Chili Peppers", UnitOfMeasurement = UnitOfMeasurement.Gram } //14
        };

        _context.Ingredients.AddRange(ingredients);

        // Create menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Thai Specialties",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = kowalskisRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Pad Thai",
                        Price = 29m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 200 }, // Noodles
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 100 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 }, // Tofu
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 1 }, // Eggs
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 }, // Soy Sauce
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 10 }, // Peanuts
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 0.5 } // Lime
                        }
                    },
                    new MenuItem
                    {
                        Name = "Green Curry",
                        Price = 35m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("restaurantboss3.PNG", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 150 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 100 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 30 }, // Curry Paste
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 200 }, // Coconut Milk
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 5 } // Chili Peppers
                        }
                    },
                    new MenuItem
                    {
                        Name = "Massaman Curry",
                        Price = 38m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski), // Reusing padthai.png
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 150 }, // Beef
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 100 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 30 }, // Curry Paste
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 200 }, // Coconut Milk
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 20 } // Peanuts
                        }
                    },
                    new MenuItem
                    {
                        Name = "Tom Yum Soup",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi2.jpg", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Shrimp
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 5 }, // Chili Peppers
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 0.5 }, // Lime
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 10 } // Fish Sauce
                        }
                    },
                    new MenuItem
                    {
                        Name = "Pineapple Fried Rice",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("restaurantboss3.PNG", _kowalski), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Rice
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 100 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 }, // Vegetables
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Pineapple", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 50 }
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Japanese Cuisine",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = kowalskisRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Sushi Set",
                        Price = 50m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi2.jpg", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 150 }, // Rice
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 50 }, // Shrimp
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Raw Fish", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 30 } // Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Chicken Teriyaki",
                        Price = 40m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 150 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 }, // Soy Sauce
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 } // Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Ramen",
                        Price = 35m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi2.jpg", _kowalski), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 200 }, // Noodles
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 100 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 1 }, // Eggs
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 } // Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Beef Udon",
                        Price = 38m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 200 }, // Noodles
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 100 }, // Beef
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 } // Soy Sauce
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Indian Dishes",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = kowalskisRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Butter Chicken",
                        Price = 36m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("restaurantboss3.PNG", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 150 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 30 }, // Curry Paste
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 } // Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Vegetable Curry",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 150 }, // Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 30 }, // Curry Paste
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 100 } // Coconut Milk
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food, // Since only Food and Alcohol are available
                Restaurant = kowalskisRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Green Tea",
                        Price = 8m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResLogo4.png", _kowalski)
                    },
                    new MenuItem
                    {
                        Name = "Mango Lassi",
                        Price = 10m,
                        AlcoholPercentage = null,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("padthai.png", _kowalski)
                    }
                }
            },
            new Menu
            {
                Name = "Alcoholic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Alcohol,
                Restaurant = kowalskisRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Sake",
                        Price = 15m,
                        AlcoholPercentage = 14m,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("restaurantboss3.PNG", _kowalski)
                    },
                    new MenuItem
                    {
                        Name = "Thai Beer",
                        Price = 12m,
                        AlcoholPercentage = 5m,
                        Restaurant = kowalskisRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResLogo4.png", _kowalski)
                    }
                }
            }
        };

        kowalskisRestaurant.Menus = menus;

        _context.Restaurants.Add(kowalskisRestaurant);

        // Save changes
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates weekly opening hours.
    /// </summary>
    /// <param name="from">Weekday opening time</param>
    /// <param name="until">Weekday closing time</param>
    /// <param name="weekendFrom">Weekend opening time</param>
    /// <param name="weekendUntil">Weekend closing time</param>
    /// <param name="openOnSunday">Indicates if open on Sunday</param>
    /// <returns>A WeeklyOpeningHours object</returns>
    private static WeeklyOpeningHours CreateOpeningHours(
        TimeOnly from, TimeOnly until,
        TimeOnly weekendFrom, TimeOnly weekendUntil,
        bool openOnSunday = true)
    {
        return new WeeklyOpeningHours(new List<OpeningHours>
        {
            new OpeningHours { From = from, Until = until }, // Monday
            new OpeningHours { From = from, Until = until }, // Tuesday
            new OpeningHours { From = from, Until = until }, // Wednesday
            new OpeningHours { From = from, Until = until }, // Thursday
            new OpeningHours { From = from, Until = until }, // Friday
            new OpeningHours { From = weekendFrom, Until = weekendUntil }, // Saturday
            openOnSunday ? new OpeningHours { From = weekendFrom, Until = weekendUntil } : new OpeningHours() // Sunday
        });
    }

    /// <summary>
    /// Requires a file upload to be present and owned by the specified user.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="owner">The owner user</param>
    /// <returns>The FileUpload object</returns>
    private async Task<FileUpload> RequireFileUpload(string fileName, User owner)
    {
        var upload = await _context.FileUploads.FirstOrDefaultAsync(x => x.FileName == fileName) ??
               throw new InvalidDataException($"Upload {fileName} not found");
        if (upload.UserId != owner.Id)
        {
            throw new InvalidDataException($"Upload {fileName} is not owned by {owner.UserName}");
        }

        return upload;
    }
}
