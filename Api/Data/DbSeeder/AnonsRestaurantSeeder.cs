using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.RestaurantSeeders;

/// <summary>
/// Seeder class for Anonymous' restaurant
/// </summary>
public class AnonRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<AnonRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _anon;
    private readonly RestaurantGroup _anonGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnonRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="anon">The owner user</param>
    /// <param name="anonGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public AnonRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<AnonRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User anon,
        RestaurantGroup anonGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _anon = anon;
        _anonGroup = anonGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for Anonymous' restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-AY.pdf", _anon);

        var anonRestaurant = new Restaurant
        {
            Name = "Anonymous' Restaurant",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "6667676878",
            Address = "ul. 123",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.90990467467737, 52.397394571933175)),
            Group = _anonGroup,
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
            Logo = await RequireFileUpload("ResLogo1.png", _anon),
            ProvideDelivery = true,
            Description = "The second example restaurant",
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
        anonRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = anonRestaurant,
                TableId = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = anonRestaurant,
                TableId = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = anonRestaurant,
                TableId = 3,
                Capacity = 6
            },
            new()
            {
                Restaurant = anonRestaurant,
                TableId = 4,
                Capacity = 2
            }
        };

        // Create photos
        anonRestaurant.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = anonRestaurant,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside1.jpg", _anon)
            }
        };

        // Ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
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
        };

        _context.Ingredients.AddRange(ingredients);

        // Menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Italian Cuisine",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = anonRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Spaghetti Bolognese",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResBurger1.jpg", _anon),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Pasta
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 100 }, // Tomato Sauce
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 100 }, // Beef Patty (ground)
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 20 }, // Parmesan
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 5 } // Basil
                        }
                    },
                    new MenuItem
                    {
                        Name = "Penne Arrabiata",
                        Price = 26m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("wege.png", _anon),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Pasta
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 100 }, // Tomato Sauce
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 10 }, // Garlic
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 0 } // Placeholder (no fries)
                        }
                    },
                    new MenuItem
                    {
                        Name = "Garlic Bread",
                        Price = 12m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResBurger1.jpg", _anon),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 1 }, // Bread
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 10 }, // Olive Oil
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 5 } // Garlic
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Salads",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = anonRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Caesar Salad",
                        Price = 24m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _anon),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Lettuce
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 100 }, // Chicken Breast
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 20 }, // Parmesan
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 1 } // Bread (croutons)
                        }
                    },
                    new MenuItem
                    {
                        Name = "Greek Salad",
                        Price = 22m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _anon), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Lettuce
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 50 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 50 }, // Cucumbers
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Feta Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 5 } // Garlic
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
                Restaurant = anonRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Classic Beef Burger",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResBurger2.jpg", _anon),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 1 }, // Bread (bun)
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 150 }, // Beef Patty
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 20 }, // Cheddar Cheese
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 20 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 } // Lettuce
                        }
                    },
                    new MenuItem
                    {
                        Name = "Chicken Burger",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResBurger2.jpg", _anon), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 1 }, // Bread (bun)
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 150 }, // Chicken Breast
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 20 }, // Cheddar Cheese
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 20 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 } // Lettuce
                        }
                    },
                    new MenuItem
                    {
                        Name = "Bacon Burger",
                        Price = 32m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _anon), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 1 }, // Bread (bun)
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 150 }, // Beef Patty
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 20 }, // Cheddar Cheese
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 30 }, // Bacon
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 20 }, // Tomatoes
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 } // Lettuce
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
                Restaurant = anonRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "French Fries",
                        Price = 10m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _anon), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 150 } // Fries
                        }
                    },
                    new MenuItem
                    {
                        Name = "Onion Rings",
                        Price = 12m,
                        AlcoholPercentage = null,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sushi.png", _anon), // Ponowne użycie obrazu
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 50 } // Garlic (as placeholder for onions)
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Alcoholic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Alcohol,
                Restaurant = anonRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Craft Beer",
                        Price = 15m,
                        AlcoholPercentage = 5m,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("woda.png", _anon)
                    },
                    new MenuItem
                    {
                        Name = "House Wine",
                        Price = 20m,
                        AlcoholPercentage = 12m,
                        Restaurant = anonRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("woda.png", _anon)
                    }
                }
            }
        };

        anonRestaurant.Menus = menus;

        _context.Restaurants.Add(anonRestaurant);

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
