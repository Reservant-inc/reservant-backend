using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.RestaurantSeeders;

/// <summary>
/// Seeder class for Atreides' restaurant
/// </summary>
public class AtreidesRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<AtreidesRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _muadib;
    private readonly RestaurantGroup _paulsGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtreidesRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="muadib">The owner user</param>
    /// <param name="paulsGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public AtreidesRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<AtreidesRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User muadib,
        RestaurantGroup paulsGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _muadib = muadib;
        _paulsGroup = paulsGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for Atreides' restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-PA.pdf", _muadib);

        var atreidesRestaurant = new Restaurant
        {
            Name = "Arrakis Spice Diner",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "8765432109",
            Address = "ul. Pustynna 42",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.923456, 52.402345)),
            Group = _paulsGroup,
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
            Logo = await RequireFileUpload("atreides_logo.png", _muadib),
            ProvideDelivery = true,
            Description = "A themed restaurant offering exotic dishes inspired by the Dune universe",
            Photos = new List<RestaurantPhoto>(),
            Tags = await _context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = _verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(11, 00), new TimeOnly(22, 00),
                new TimeOnly(11, 00), new TimeOnly(23, 00)),
        };

        // Create tables
        atreidesRestaurant.Tables = new List<Table>
        {
            new()
            {
                Restaurant = atreidesRestaurant,
                TableId = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = atreidesRestaurant,
                TableId = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = atreidesRestaurant,
                TableId = 3,
                Capacity = 6
            },
            new()
            {
                Restaurant = atreidesRestaurant,
                TableId = 4,
                Capacity = 2
            }
        };

        // Create photos
        atreidesRestaurant.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = atreidesRestaurant,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("atreides_interior.jpg", _muadib)
            }
        };

        // Ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
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
            new Ingredient { PublicName = "Stilgar's Spice Mix", UnitOfMeasurement = UnitOfMeasurement.Gram } //14
        };

        _context.Ingredients.AddRange(ingredients);

        // Menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Fremen Delicacies",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = atreidesRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Spice-infused Sandworm Steak",
                        Price = 50m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("sandworm_steak.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 200 }, // Sandworm Meat
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 5 }, // Spice
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 10 }, // Desert Herbs
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 50 } // Desert Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Fremen Bread with Melange Sauce",
                        Price = 20m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("fremen_bread.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 1 }, // Fremen Bread
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 30 }, // Melange Sauce
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 5 } // Desert Herbs
                        }
                    },
                    new MenuItem
                    {
                        Name = "Desert Grain Salad",
                        Price = 18m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("grain_salad.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Desert Grains
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 50 }, // Desert Vegetables
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 30 }, // Date Palm Fruits
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 5 } // Desert Herbs
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Spice Sweets",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = atreidesRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Spice Cake",
                        Price = 15m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("spice_cake.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Desert Grains
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 5 }, // Spice
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 2 }, // Quail Eggs
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 } // Date Palm Fruits
                        }
                    },
                    new MenuItem
                    {
                        Name = "Desert Cheesecake",
                        Price = 18m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("desert_cheesecake.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 100 }, // Sietch Cheese
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 100 }, // Desert Grains
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 2 }, // Quail Eggs
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 } // Date Palm Fruits
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Exotic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food, // Since only Food and Alcohol are available
                Restaurant = atreidesRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Cactus Juice",
                        Price = 10m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("cactus_juice.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 250 } // Cactus Juice
                        }
                    },
                    new MenuItem
                    {
                        Name = "Herbal Tea",
                        Price = 8m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("herbal_tea.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 250 } // Herbal Tea
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
                Restaurant = atreidesRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Spiced Wine",
                        Price = 22m,
                        AlcoholPercentage = 14m,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("spiced_wine.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 150 }, // Spiced Wine
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 2 } // Spice
                        }
                    },
                    new MenuItem
                    {
                        Name = "Melange Liquor",
                        Price = 25m,
                        AlcoholPercentage = 20m,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("melange_liquor.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 5 }, // Spice
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 100 } // Spiced Wine
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Special Offers",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = atreidesRestaurant,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Stilgar's Special Stew",
                        Price = 45m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("stilgar_stew.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 150 }, // Sandworm Meat
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 10 }, // Stilgar's Spice Mix
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 10 }, // Desert Herbs
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 50 } // Desert Vegetables
                        }
                    },
                    new MenuItem
                    {
                        Name = "Paul's Desert Delight",
                        Price = 40m,
                        AlcoholPercentage = null,
                        Restaurant = atreidesRestaurant,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("paul_desert_delight.jpg", _muadib),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 200 }, // Water
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 5 }, // Spice
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 10 }, // Desert Herbs
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 30 } // Date Palm Fruits
                        }
                    }
                }
            }
        };

        atreidesRestaurant.Menus = menus;

        _context.Restaurants.Add(atreidesRestaurant);

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
