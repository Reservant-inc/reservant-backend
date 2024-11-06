using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.RestaurantSeeders;

/// <summary>
/// Seeder class for Witcher's restaurant
/// </summary>
public class WitcherRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<WitcherRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _geralt;
    private readonly RestaurantGroup _geraltsGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="WitcherRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="geralt">The owner user</param>
    /// <param name="geraltsGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public WitcherRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<WitcherRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User geralt,
        RestaurantGroup geraltsGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _geralt = geralt;
        _geraltsGroup = geraltsGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for Witcher's restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-GR.pdf", _geralt);

        var witcherRestaurant = new Restaurant
        {
            Name = "Kaer Morhen Inn",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "9876543210",
            Address = "ul. Stary Szlak 1",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.921482, 52.401038)),
            Group = _geraltsGroup,
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
            Logo = await RequireFileUpload("witcher_logo.png", _geralt),
            ProvideDelivery = false,
            Description = "A rustic inn serving hearty meals from the Witcher universe",
            Photos = new List<RestaurantPhoto>(),
            Tags = await _context.RestaurantTags
                .Where(rt => rt.Name == "OnSite")
                .ToListAsync(),
            VerifierId = _verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(12, 00), new TimeOnly(22, 00),
                new TimeOnly(12, 00), new TimeOnly(23, 00)),
        };

        // Create tables
        witcherRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = witcherRestaurant,
                TableId = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = witcherRestaurant,
                TableId = 2,
                Capacity = 6
            },
            new()
            {
                Restaurant = witcherRestaurant,
                TableId = 3,
                Capacity = 2
            },
            new()
            {
                Restaurant = witcherRestaurant,
                TableId = 4,
                Capacity = 8
            }
        };

        // Create photos
        witcherRestaurant.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = witcherRestaurant,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("witcher_inn_inside.jpg", _geralt)
            }
        };

        // Ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
            new Ingredient { PublicName = "Bread", UnitOfMeasurement = UnitOfMeasurement.Unit }, // 0
            new Ingredient { PublicName = "Beef", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 1
            new Ingredient { PublicName = "Pork", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 2
            new Ingredient { PublicName = "Chicken", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 3
            new Ingredient { PublicName = "Potatoes", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 4
            new Ingredient { PublicName = "Carrots", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 5
            new Ingredient { PublicName = "Cabbage", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 6
            new Ingredient { PublicName = "Onions", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 7
            new Ingredient { PublicName = "Garlic", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 8
            new Ingredient { PublicName = "Mushrooms", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 9
            new Ingredient { PublicName = "Herbs", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 10
            new Ingredient { PublicName = "Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 11
            new Ingredient { PublicName = "Fish", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 12
            new Ingredient { PublicName = "Butter", UnitOfMeasurement = UnitOfMeasurement.Gram }, // 13
            new Ingredient { PublicName = "Flour", UnitOfMeasurement = UnitOfMeasurement.Gram } //14
        };

        _context.Ingredients.AddRange(ingredients);

        // Menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Hearty Meals",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = witcherRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Beef Stew",
                        Price = 35m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("beef_stew.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 200 }, // Beef
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 100 }, // Potatoes
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 50 }, // Carrots
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 10 }, // Garlic
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 5 } // Herbs
                        }
                    },
                    new MenuItem
                    {
                        Name = "Roasted Chicken",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("roasted_chicken.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 250 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 100 }, // Potatoes
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 5 } // Herbs
                        }
                    },
                    new MenuItem
                    {
                        Name = "Pork Sausages with Cabbage",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pork_sausages.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 200 }, // Pork
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 100 }, // Cabbage
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 10 } // Garlic
                        }
                    },
                    new MenuItem
                    {
                        Name = "Fried Fish",
                        Price = 32m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("fried_fish.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 200 }, // Fish
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 20 }, // Butter
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 100 }, // Potatoes
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 5 } // Herbs
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Soups and Stews",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = witcherRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Mushroom Soup",
                        Price = 18m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("mushroom_soup.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 100 }, // Mushrooms
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 20 }, // Butter
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 10 } // Garlic
                        }
                    },
                    new MenuItem
                    {
                        Name = "Vegetable Stew",
                        Price = 16m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("vegetable_stew.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 100 }, // Potatoes
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 50 }, // Carrots
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 50 }, // Cabbage
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 5 } // Herbs
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Breads and Pies",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = witcherRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Freshly Baked Bread",
                        Price = 5m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("fresh_bread.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 1 }, // Bread
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 10 } // Butter
                        }
                    },
                    new MenuItem
                    {
                        Name = "Meat Pie",
                        Price = 20m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("meat_pie.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 100 }, // Beef
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 100 }, // Flour
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 50 }, // Butter
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 30 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 10 } // Garlic
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Cheeses",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = witcherRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Cheese Platter",
                        Price = 25m,
                        AlcoholPercentage = null,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("cheese_platter.jpg", _geralt),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 150 } // Cheese
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
                Restaurant = witcherRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Mahakaman Mead",
                        Price = 15m,
                        AlcoholPercentage = 10m,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("mead.jpg", _geralt)
                    },
                    new MenuItem
                    {
                        Name = "Redanian Lager",
                        Price = 12m,
                        AlcoholPercentage = 5m,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("lager.jpg", _geralt)
                    },
                    new MenuItem
                    {
                        Name = "Toussaint Red Wine",
                        Price = 18m,
                        AlcoholPercentage = 13m,
                        Restaurant = witcherRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("red_wine_toussaint.jpg", _geralt)
                    }
                }
            }
        };

        witcherRestaurant.Menus = menus;

        _context.Restaurants.Add(witcherRestaurant);

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
