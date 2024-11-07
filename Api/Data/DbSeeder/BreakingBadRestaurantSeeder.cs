using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.RestaurantSeeders;

#region BreakingBadRestaurantSeeder

/// <summary>
/// Seeder class for the "Los Pollos Hermanos" restaurant
/// </summary>
public class BreakingBadRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<BreakingBadRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _gusFring;
    private readonly RestaurantGroup _losPollosGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="BreakingBadRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="gusFring">The owner user</param>
    /// <param name="losPollosGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public BreakingBadRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<BreakingBadRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User gusFring,
        RestaurantGroup losPollosGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _gusFring = gusFring;
        _losPollosGroup = losPollosGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for the "Los Pollos Hermanos" restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-WW.pdf", _gusFring);

        var losPollosRestaurant = new Restaurant
        {
            Name = "Los Pollos Hermanos",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1234567890",
            Address = "ul. Albuquerque 23",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.924567, 52.403456)),
            Group = _losPollosGroup,
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
            Logo = await RequireFileUpload("ResLogo5.png", _gusFring),
            ProvideDelivery = true,
            Description = "A family-friendly restaurant specializing in fried chicken and classic American cuisine",
            Photos = new List<RestaurantPhoto>(),
            Tags = await _context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = _verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(22, 00),
                new TimeOnly(10, 00), new TimeOnly(23, 00)),
        };

        // Create tables
        losPollosRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = losPollosRestaurant,
                TableId = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = losPollosRestaurant,
                TableId = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = losPollosRestaurant,
                TableId = 3,
                Capacity = 6
            },
            new()
            {
                Restaurant = losPollosRestaurant,
                TableId = 4,
                Capacity = 2
            }
        };

        // Create photos
        losPollosRestaurant.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = losPollosRestaurant,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", _gusFring)
            }
        };

        // Ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
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
        };

        _context.Ingredients.AddRange(ingredients);

        // Menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Fried Chicken",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = losPollosRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Classic Fried Chicken",
                        Price = 25m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResMeal1.jpg", _gusFring),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 }, // Flour
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 1 }, // Eggs
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 30 } // Bread Crumbs
                        }
                    },
                    new MenuItem
                    {
                        Name = "Spicy Fried Chicken",
                        Price = 27m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResMeal1.jpg", _gusFring), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 }, // Flour
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 1 }, // Eggs
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 30 }, // Bread Crumbs
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Spices", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 10 }
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Burgers",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = losPollosRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Classic Beef Burger",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _gusFring),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 1 }, // Burger Bun
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 150 }, // Beef Patty
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 }, // Cheddar Cheese
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 20 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 } // Lettuce
                        }
                    },
                    new MenuItem
                    {
                        Name = "Chicken Burger",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _gusFring), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 1 }, // Burger Bun
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 150 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 }, // Cheddar Cheese
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 20 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 } // Lettuce
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Sides",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = losPollosRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "French Fries",
                        Price = 10m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResMeal1.jpg", _gusFring), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 150 } // Potatoes
                        }
                    },
                    new MenuItem
                    {
                        Name = "Coleslaw",
                        Price = 8m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _gusFring),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Cabbage", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 20 } // Mayonnaise
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
                Restaurant = losPollosRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Soft Drink",
                        Price = 5m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _gusFring) // Ponowne użycie obrazu
                    },
                    new MenuItem
                    {
                        Name = "Iced Tea",
                        Price = 6m,
                        AlcoholPercentage = null,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _gusFring) // Ponowne użycie obrazu
                    }
                }
            },
            new Menu
            {
                Name = "Alcoholic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Alcohol,
                Restaurant = losPollosRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Beer",
                        Price = 12m,
                        AlcoholPercentage = 5m,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _gusFring)
                    },
                    new MenuItem
                    {
                        Name = "House Wine",
                        Price = 18m,
                        AlcoholPercentage = 12m,
                        Restaurant = losPollosRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("wine.png", _gusFring)
                    }
                }
            }
        };

        losPollosRestaurant.Menus = menus;

        _context.Restaurants.Add(losPollosRestaurant);

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

#endregion
