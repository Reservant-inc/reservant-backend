using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Options;
using Reservant.Api.Services;

namespace Reservant.Api.Data.RestaurantSeeders;

/// <summary>
/// Seeder class for John Doe's restaurant
/// </summary>
public class JohnDoesRestaurantSeeder
{
    private readonly ApiDbContext _context;
    private readonly UserService _userService;
    private readonly RestaurantService _restaurantService;
    private readonly ILogger<JohnDoesRestaurantSeeder> _logger;
    private readonly IOptions<FileUploadsOptions> _fileUploadsOptions;
    private readonly GeometryFactory _geometryFactory;
    private readonly User _johnDoe;
    private readonly RestaurantGroup _johnDoesGroup;
    private readonly User _verifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="JohnDoesRestaurantSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userService">The user service</param>
    /// <param name="restaurantService">The restaurant service</param>
    /// <param name="logger">The logger</param>
    /// <param name="fileUploadsOptions">The file uploads options</param>
    /// <param name="geometryFactory">The geometry factory</param>
    /// <param name="johnDoe">The owner user</param>
    /// <param name="johnDoesGroup">The restaurant group</param>
    /// <param name="verifier">The verifier user</param>
    public JohnDoesRestaurantSeeder(
        ApiDbContext context,
        UserService userService,
        RestaurantService restaurantService,
        ILogger<JohnDoesRestaurantSeeder> logger,
        IOptions<FileUploadsOptions> fileUploadsOptions,
        GeometryFactory geometryFactory,
        User johnDoe,
        RestaurantGroup johnDoesGroup,
        User verifier)
    {
        _context = context;
        _userService = userService;
        _restaurantService = restaurantService;
        _logger = logger;
        _fileUploadsOptions = fileUploadsOptions;
        _geometryFactory = geometryFactory;
        _johnDoe = johnDoe;
        _johnDoesGroup = johnDoesGroup;
        _verifier = verifier;
    }

    /// <summary>
    /// Seeds data for John Doe's restaurant.
    /// </summary>
    public async Task SeedAsync()
    {
        var exampleDocument = await RequireFileUpload("test-jd.pdf", _johnDoe);

        var johnDoes = new Restaurant
        {
            Name = "John Doe's",
            RestaurantType = RestaurantType.Restaurant,
            Nip = "1231264550",
            Address = "ul. Marszałkowska 2",
            PostalIndex = "00-000",
            City = "Warszawa",
            Location = _geometryFactory.CreatePoint(new Coordinate(20.91364863552046, 52.39625635)),
            Group = _johnDoesGroup,
            RentalContractFileName = null,
            AlcoholLicenseFileName = null!,
            AlcoholLicense = exampleDocument,
            BusinessPermissionFileName = null!,
            BusinessPermission = exampleDocument,
            MaxReservationDurationMinutes = 120,
            IdCardFileName = null!,
            IdCard = exampleDocument,
            LogoFileName = null!,
            Logo = await RequireFileUpload("ResLogo2.png", _johnDoe),
            ProvideDelivery = true,
            Description = "The first example restaurant",
            Tags = await _context.RestaurantTags
                .Where(rt => rt.Name == "OnSite" || rt.Name == "Takeaway")
                .ToListAsync(),
            VerifierId = _verifier.Id,
            OpeningHours = CreateOpeningHours(
                new TimeOnly(10, 00), new TimeOnly(22, 00),
                new TimeOnly(10, 00), new TimeOnly(23, 00)),
        };

        johnDoes.Tables = new List<Table>
        {
            new()
            {
                Restaurant = johnDoes,
                TableId = 1,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes,
                TableId = 2,
                Capacity = 4
            },
            new()
            {
                Restaurant = johnDoes,
                TableId = 3,
                Capacity = 6
            },
            new()
            {
                Restaurant = johnDoes,
                TableId = 4,
                Capacity = 2
            }
        };

        johnDoes.Photos = new List<RestaurantPhoto>
        {
            new()
            {
                Restaurant = johnDoes,
                Order = 1,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", _johnDoe)
            }
        };

        // Ingredients (up to 15)
        var ingredients = new List<Ingredient>
        {
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
            new Ingredient { PublicName = "Cheddar", UnitOfMeasurement = UnitOfMeasurement.Gram } // 14
        };

        _context.Ingredients.AddRange(ingredients);

        // Menus (4-5 menus)
        var menus = new List<Menu>
        {
            new Menu
            {
                Name = "Classic Pizzas",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = johnDoes,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Margherita",
                        Price = 25m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResPizza1.jpg", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Dough
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },  // Tomato Sauce
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 }, // Mozzarella
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 5 }    // Basil
                        }
                    },
                    new MenuItem
                    {
                        Name = "Pepperoni",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResPizza2.jpg", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 50 }  // Pepperoni
                        }
                    },
                    new MenuItem
                    {
                        Name = "Four Seasons",
                        Price = 35m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 30 }, // Mushrooms
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 30 }, // Ham
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 }, // Olives
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 }  // Onions
                        }
                    },
                    new MenuItem
                    {
                        Name = "Vegetarian",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi1.jpg", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 30 }, // Mushrooms
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 30 }, // Bell Peppers
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 }, // Olives
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 5 }  // Basil
                        }
                    },
                    new MenuItem
                    {
                        Name = "Mexican",
                        Price = 32m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResPizza1.jpg", _johnDoe), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 50 }, // BBQ Sauce
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 50 }, // Beef
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 30 }, // Bell Peppers
                            new IngredientMenuItem { Ingredient = ingredients[7], AmountUsed = 20 }, // Onions
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 50 } // Cheddar
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Specialty Pizzas",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = johnDoes,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Hawaiian",
                        Price = 32m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResPizza2.jpg", _johnDoe), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 50 }, // Ham
                            new IngredientMenuItem { Ingredient = ingredients[9], AmountUsed = 50 }   // Pineapple
                        }
                    },
                    new MenuItem
                    {
                        Name = "BBQ Chicken",
                        Price = 35m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[13], AmountUsed = 50 }, // BBQ Sauce
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 50 } // Chicken
                        }
                    },
                    new MenuItem
                    {
                        Name = "Meat Lovers",
                        Price = 38m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi1.jpg", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 },
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[3], AmountUsed = 30 }, // Pepperoni
                            new IngredientMenuItem { Ingredient = ingredients[10], AmountUsed = 30 }, // Ham
                            new IngredientMenuItem { Ingredient = ingredients[11], AmountUsed = 30 }, // Chicken
                            new IngredientMenuItem { Ingredient = ingredients[12], AmountUsed = 30 } // Beef
                        }
                    },
                    new MenuItem
                    {
                        Name = "Seafood Delight",
                        Price = 40m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResInside5.jpg", _johnDoe), // Using interior photo
                        Ingredients = new List<IngredientMenuItem>
                        {
                            // Assuming we have seafood ingredients
                        }
                    },
                    new MenuItem
                    {
                        Name = "Quattro Formaggi",
                        Price = 33m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResPizza1.jpg", _johnDoe), // Reusing image
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Dough
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },  // Tomato Sauce
                            new IngredientMenuItem { Ingredient = ingredients[2], AmountUsed = 50 },  // Mozzarella
                            new IngredientMenuItem { Ingredient = ingredients[14], AmountUsed = 50 }, // Cheddar
                            // Assuming other cheeses are added as ingredients
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Parmesan", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 50 },
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Gorgonzola", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 50 }
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Vegan Pizzas",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = johnDoes,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Vegan Delight",
                        Price = 30m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("pierogi.png", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Dough
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },  // Tomato Sauce
                            // Assuming vegan cheese is added as an ingredient
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Vegan Cheese", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[6], AmountUsed = 30 }, // Mushrooms
                            new IngredientMenuItem { Ingredient = ingredients[8], AmountUsed = 30 }, // Bell Peppers
                            new IngredientMenuItem { Ingredient = ingredients[5], AmountUsed = 20 }  // Olives
                        }
                    },
                    new MenuItem
                    {
                        Name = "Vegan Margherita",
                        Price = 28m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("ResSushi1.jpg", _johnDoe),
                        Ingredients = new List<IngredientMenuItem>
                        {
                            new IngredientMenuItem { Ingredient = ingredients[0], AmountUsed = 200 }, // Dough
                            new IngredientMenuItem { Ingredient = ingredients[1], AmountUsed = 50 },  // Tomato Sauce
                            new IngredientMenuItem { Ingredient = new Ingredient { PublicName = "Vegan Mozzarella", UnitOfMeasurement = UnitOfMeasurement.Gram }, AmountUsed = 100 },
                            new IngredientMenuItem { Ingredient = ingredients[4], AmountUsed = 5 }    // Basil
                        }
                    }
                }
            },
            new Menu
            {
                Name = "Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Food,
                Restaurant = johnDoes,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Coca-Cola",
                        Price = 5m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe), // Using available image
                    },
                    new MenuItem
                    {
                        Name = "Orange Juice",
                        Price = 6m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe),
                    },
                    new MenuItem
                    {
                        Name = "Mineral Water",
                        Price = 4m,
                        AlcoholPercentage = null,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe),
                    }
                }
            },
            new Menu
            {
                Name = "Alcoholic Beverages",
                DateFrom = new DateOnly(2024, 1, 1),
                DateUntil = null,
                MenuType = MenuType.Alcohol,
                Restaurant = johnDoes,
                MenuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Beer",
                        Price = 8m,
                        AlcoholPercentage = 5m,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe),
                    },
                    new MenuItem
                    {
                        Name = "Red Wine",
                        Price = 15m,
                        AlcoholPercentage = 12m,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe),
                    },
                    new MenuItem
                    {
                        Name = "White Wine",
                        Price = 15m,
                        AlcoholPercentage = 12m,
                        Restaurant = johnDoes,
                        PhotoFileName = null!,
                        Photo = await RequireFileUpload("piwo.png", _johnDoe),
                    }
                }
            }
        };

        johnDoes.Menus = menus;

        _context.Restaurants.Add(johnDoes);

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
