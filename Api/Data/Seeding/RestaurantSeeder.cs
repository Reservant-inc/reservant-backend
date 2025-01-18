using Microsoft.EntityFrameworkCore;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Base class for every restaurant seeder
/// </summary>
public abstract class RestaurantSeeder(ApiDbContext context, UserService userService, RestaurantService restaurantService)
{
    private Restaurant _restaurant = null!;
    private User _restaurantOwner = null!;
    private Ingredient[] _ingredients = null!;
    private readonly List<FileUpload> _menuItemPhotos = [];
    private Random _random = null!;
    private List<User> _employees = null!;

    /// <summary>
    /// Owner of the restaurant
    /// </summary>
    protected User RestaurantOwner => _restaurantOwner;

    /// <summary>
    /// Create the restaurant
    /// </summary>
    /// <remarks>
    /// Orchestrates the overloaded methods
    /// </remarks>
    public async Task<Restaurant> Seed(UserSeeder users)
    {
        _random = new Random(RandomSeed);
        _restaurantOwner = GetRestaurantOwner(users);
        foreach (var fileName in GetMenuItemPhotoFileNames())
        {
            _menuItemPhotos.Add(await RequireFileUpload(fileName));
        }

        _restaurant = await CreateRestaurant(_restaurantOwner, users);
        context.Add(_restaurant);
        await context.SaveChangesAsync();

        _restaurant.Reviews = CreateReviews(users);

        _employees = await CreateEmployees();

        var ingredients = CreateIngredients();
        foreach (var ingredient in ingredients)
        {
            ingredient.Restaurant = _restaurant;
        }

        context.AddRange(ingredients.AsEnumerable());
        _ingredients = ingredients;

        var menus = CreateMenus();
        _restaurant.Menus = menus;
        context.Update(_restaurant);

        await context.SaveChangesAsync();
        return _restaurant;
    }

    /// <summary>
    /// Seed for the randomizer
    /// </summary>
    protected abstract int RandomSeed { get; }

    /// <summary>
    /// Get file names of uploads that can be used as menu item photos
    /// </summary>
    /// <remarks>
    /// Called once
    /// </remarks>
    protected abstract List<string> GetMenuItemPhotoFileNames();

    /// <summary>
    /// Get owner of the restaurant
    /// </summary>
    /// <remarks>
    /// Called once in the beginning, the result is cached
    /// </remarks>
    protected abstract User GetRestaurantOwner(UserSeeder users);

    /// <summary>
    /// Create the restaurant object
    /// </summary>
    protected abstract Task<Restaurant> CreateRestaurant(User owner, UserSeeder users);

    /// <summary>
    /// Create the ingredient list
    /// </summary>
    /// <remarks>
    /// Called once, the result is cached
    /// </remarks>
    protected abstract Ingredient[] CreateIngredients();

    /// <summary>
    /// Create the menu list
    /// </summary>
    /// <returns></returns>
    protected abstract List<Menu> CreateMenus();

    /// <summary>
    /// Create the restaurant employees
    /// </summary>
    /// <returns></returns>
    protected virtual Task<List<User>> CreateEmployees() => Task.FromResult(new List<User>());

    /// <summary>
    /// Create the restaurant reviews
    /// </summary>
    protected abstract Review[] CreateReviews(UserSeeder users);

    /// <summary>
    /// Find employee by login
    /// </summary>
    protected User FindEmployeeByLogin(string login)
    {
        return _employees.First(e => e.UserName!.Split(UserService.RestaurantEmployeeLoginSeparator)[1] == login);
    }

    /// <summary>
    /// Create a restaurant employee
    /// </summary>
    protected async Task<User> CreateRestaurantEmployee(
        string login, string firstName, string lastName, string id, bool isHallEmployee, bool isBackdoorEmployee)
    {
        var employee = (await userService.RegisterRestaurantEmployeeAsync(new RegisterRestaurantEmployeeRequest
        {
            Login = login,
            Password = UserSeeder.ExamplePassword,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = new DateOnly(2001, 5, 5),
            PhoneNumber = new PhoneNumber("+48", "123456789"),
        }, _restaurantOwner, Guid.Parse(id))).OrThrow();

        (await restaurantService.AddEmployeeAsync(
            new List<AddEmployeeRequest> {
                new AddEmployeeRequest
                {
                    EmployeeId = employee.Id,
                    IsBackdoorEmployee = isBackdoorEmployee,
                    IsHallEmployee = isHallEmployee,
                },
            },
            _restaurant.RestaurantId,
            _restaurantOwner.Id)).OrThrow();

        return employee;
    }

    /// <summary>
    /// Create a new restaurant group for the user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    protected static RestaurantGroup CreateRestaurantGroup(User user)
    {
        return new RestaurantGroup
        {
            Name = $"{user.FullName}'s Restaurant Group",
            Owner = user,
            OwnerId = user.Id,
        };
    }

    /// <summary>
    /// Create a new restaurant group for the user, or reuse it if there already is a group with this name
    /// </summary>
    /// <param name="user"></param>
    /// <param name="name">The name of the group to find or to create</param>
    /// <returns></returns>
    protected async Task<RestaurantGroup> CreateOrReuseRestaurantGroup(User user, string name)
    {
        var existingGroup = await context.RestaurantGroups
            .FirstOrDefaultAsync(g => g.Owner == user && g.Name == name);
        if (existingGroup is not null) return existingGroup;

        return new RestaurantGroup
        {
            Name = name,
            Owner = user,
            OwnerId = user.Id,
        };
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
    protected static WeeklyOpeningHours CreateOpeningHours(
        TimeOnly from, TimeOnly until,
        TimeOnly weekendFrom, TimeOnly weekendUntil,
        bool openOnSunday = true)
    {
        return new WeeklyOpeningHours([
            new OpeningHours { From = from, Until = until }, // Monday
            new OpeningHours { From = from, Until = until }, // Tuesday
            new OpeningHours { From = from, Until = until }, // Wednesday
            new OpeningHours { From = from, Until = until }, // Thursday
            new OpeningHours { From = from, Until = until }, // Friday
            new OpeningHours { From = weekendFrom, Until = weekendUntil }, // Saturday
            openOnSunday ? new OpeningHours { From = weekendFrom, Until = weekendUntil } : new OpeningHours(), // Sunday
        ]);
    }

    private const decimal MinMenuItemPrice = 5m;
    private const decimal MaxMenuItemPrice = 100m;
    private const int MaxMenuItemIngredientNumber = 5;
    private const int MinMenuItemIngredientAmount = 30;
    private const int MaxMenuItemIngredientAmount = 500;

    /// <summary>
    /// Create a menu item with randomized properties
    /// </summary>
    protected MenuItem CreateRandomMenuItem(string name, string? alternateName, decimal? alcoholPercentage = null) =>
        new()
        {
            Name = name,
            Price = NextRandomPrice(),
            AlcoholPercentage = alcoholPercentage,
            PhotoFileName = null!,
            Photo = NextRandomMenuItemPhoto(),
            Ingredients = FindRandomIngredientsForMenuItem(),
            Restaurant = _restaurant,
        };

    /// <summary>
    /// Create a menu item with randomized properties
    /// </summary>
    protected MenuItem CreateRandomMenuItem(string name, decimal? alcoholPercentage = null) =>
        CreateRandomMenuItem(name, null, alcoholPercentage);

    private decimal NextRandomPrice() =>
        Math.Round(MinMenuItemPrice + (decimal)_random.NextDouble() * (MaxMenuItemPrice - MinMenuItemPrice), 2);

    private FileUpload NextRandomMenuItemPhoto() =>
        _menuItemPhotos[_random.Next(0, _menuItemPhotos.Count)];

    private List<IngredientMenuItem> FindRandomIngredientsForMenuItem()
    {
        if (_ingredients.Length == 0) return [];

        var count = _random.Next(Math.Min(_ingredients.Length, MaxMenuItemIngredientNumber));
        return _random.GetItems(_ingredients, count)
            .Distinct()
            .Select(ingredient => new IngredientMenuItem
            {
                Ingredient = ingredient,
                AmountUsed = _random.Next(MinMenuItemIngredientAmount, MaxMenuItemIngredientAmount),
            })
            .ToList();
    }

    /// <summary>
    /// Find required photos in the database, load them as RestaurantPhotos. They must be owned by
    /// the restaurant owner
    /// </summary>
    /// <param name="fileNames">Names of the file uploads</param>
    protected async Task<List<RestaurantPhoto>> RequireRestaurantPhotos(params string[] fileNames)
    {
        List<RestaurantPhoto> result = [];
        foreach (var (fileName, index) in fileNames.Select((fileName, index) => (fileName, index)))
        {
            result.Add(
                new RestaurantPhoto
                {
                    Order = index + 1,
                    PhotoFileName = null!,
                    Photo = await RequireFileUpload(fileName),
                }
            );
        }

        return result;
    }

    /// <summary>
    /// Get restaurant tags by their names
    /// </summary>
    protected async Task<List<RestaurantTag>> RequireRestaurantTags(params string[] tagNames)
    {
        return await context.RestaurantTags
            .Where(rt => tagNames.Contains(rt.Name))
            .ToListAsync();
    }

    /// <summary>
    /// Requires a file upload to be present and owned by the restaurant owner
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The FileUpload object</returns>
    protected async Task<FileUpload> RequireFileUpload(string fileName)
    {
        var upload = await context.FileUploads.FirstOrDefaultAsync(x => x.FileName == fileName) ??
                     throw new InvalidDataException($"Upload {fileName} not found");
        if (upload.UserId != _restaurantOwner.Id)
        {
            throw new InvalidDataException($"Upload {fileName} is not owned by {_restaurantOwner.UserName}");
        }

        return upload;
    }
}
