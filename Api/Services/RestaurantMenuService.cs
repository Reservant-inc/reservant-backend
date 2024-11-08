using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;


/// <summary>
/// Util class for managing RestaurantMenus
/// </summary>
public class RestaurantMenuService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper)
{

    /// <summary>
    /// Returns a menu with given Id
    /// </summary>
    /// <param name="menuId"> Id of the menu.</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    public async Task<Result<MenuVM>> GetSingleMenuAsync(int menuId)
    {
        var menu = await context.Menus
            .Include(m => m.MenuItems)
            .Where(m => m.MenuId == menuId)
            .ProjectTo<MenuVM>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (menu is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Menu with ID {menuId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return menu;
    }


    /// <summary>
    /// Posts menu to the restaurant
    /// </summary>
    /// <param name="req">Request for Menu to be created.</param>
    /// <param name="user">Currently logged-in user</param>
    /// <returns></returns>
    [ErrorCode(nameof(CreateMenuRequest.RestaurantId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(CreateMenuRequest.RestaurantId), ErrorCodes.AccessDenied, $"User is not the owner of the restaurant with ID")]
    public async Task<Result<MenuSummaryVM>> PostMenuToRestaurant(CreateMenuRequest req, User user)
    {
        var restaurant = await context.Restaurants
            .AsNoTracking()
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.RestaurantId == req.RestaurantId);

        if (restaurant == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(req.RestaurantId),
                ErrorMessage = $"Restaurant with ID: {req.RestaurantId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (restaurant.Group.OwnerId != user.Id)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(req.RestaurantId),
                ErrorMessage = $"User is not the owner of the restaurant with ID: {req.RestaurantId}.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var newMenu = new Menu
        {
            Name = req.Name.Trim(),
            AlternateName = req.AlternateName?.Trim(),
            MenuType = req.MenuType,
            DateFrom = req.DateFrom,
            DateUntil = req.DateUntil,
            RestaurantId = req.RestaurantId
        };

        var result = await validationService.ValidateAsync(newMenu, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.Menus.Add(newMenu);
        await context.SaveChangesAsync();

        return mapper.Map<MenuSummaryVM>(newMenu);
    }


    /// <summary>
    /// Add MenuItems to a Menu
    /// </summary>
    /// <param name="menuId">ID of the Menu</param>
    /// <param name="request">Information about the items</param>
    /// <param name="user">Currently logged-in user</param>
    [ErrorCode("<menuId>", ErrorCodes.NotFound)]
    [ErrorCode(nameof(Menu.MenuId), ErrorCodes.AccessDenied, "User is not the owner of the restaurant that contains menu ID")]
    [ErrorCode(nameof(AddItemsRequest.ItemIds), ErrorCodes.BelongsToAnotherRestaurant, "MenuItems with IDs belong to another restaurant")]
    [ErrorCode(nameof(AddItemsRequest.ItemIds), ErrorCodes.NotFound)]
    [ValidatorErrorCodes<Menu>]
    public async Task<Result<MenuVM>> AddItemsToMenuAsync(int menuId, AddItemsRequest request, User user)
    {
        var menuToUpdate = await context.Menus
            .Include(m => m.MenuItems)
            .Include(m => m.Restaurant)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync(m => m.MenuId == menuId);

        if (menuToUpdate == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(menuId),
                ErrorMessage = $"Menu with id ${menuId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var restaurant = menuToUpdate.Restaurant;

        if (restaurant.Group.OwnerId != user.Id)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(menuToUpdate.MenuId),
                ErrorMessage = $"User is not the owner of the restaurant that contains menu ID: {menuToUpdate.MenuId}.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var menuItemsToAdd = await context.MenuItems
            .Where(item => request.ItemIds.Contains(item.MenuItemId))
            .ToListAsync();

        var fromAnotherRestaurant = menuItemsToAdd
            .Where(mi => mi.RestaurantId != menuToUpdate.RestaurantId)
            .Select(mi => mi.MenuItemId)
            .ToList();
        if (fromAnotherRestaurant.Count != 0)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.ItemIds),
                ErrorMessage = $"MenuItems with IDs {string.Join(", ", fromAnotherRestaurant)} belong to another restaurant",
                ErrorCode = ErrorCodes.BelongsToAnotherRestaurant
            };
        }

        var nonExistentItemIds = request.ItemIds.Except(menuItemsToAdd.Select(item => item.MenuItemId)).ToList();
        if (nonExistentItemIds.Count != 0)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.ItemIds),
                ErrorMessage = $"MenuItems with IDs {string.Join(", ", nonExistentItemIds)} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Adding items
        foreach (var item in menuItemsToAdd)
        {
            // Adding only items that are not already in menu
            if (!menuToUpdate.MenuItems.Any(mi => mi.MenuItemId == item.MenuItemId))
            {
                menuToUpdate.MenuItems.Add(item);
            }
        }

        var result = await validationService.ValidateAsync(menuToUpdate, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        await context.SaveChangesAsync();

        return mapper.Map<MenuVM>(menuToUpdate);
    }

    /// <summary>
    /// Update a Menu
    /// </summary>
    /// <param name="request">New values for the attributes</param>
    /// <param name="menuId">ID of the Menu to update</param>
    /// <param name="user">Currently logged-in user</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User not permitted to edit menu with ID.")]
    [ErrorCode(nameof(UpdateMenuRequest.MenuItemsIds), ErrorCodes.NotFound)]
    [ValidatorErrorCodes<Menu>]
    public async Task<Result<MenuVM>> UpdateMenuAsync(UpdateMenuRequest request, int menuId, User user)
    {
        // Getting menu
        var menu = await context.Menus
            .Where(m => m.MenuId == menuId)
            .Include(menu => menu.Restaurant)
            .ThenInclude(restaurant => restaurant.Group)
            .Include(menu => menu.MenuItems)
            .FirstOrDefaultAsync();

        // Checking if menu exists
        if (menu == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Menu with ID {menuId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Checking ownership of menu
        if (menu.Restaurant.Group.OwnerId != user.Id)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"User not permitted to edit menu with ID {menuId}.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        // Validating menu items
        var menuItems = await context.MenuItems
            .Where(m => m.RestaurantId == menu.RestaurantId && request.MenuItemsIds.Contains(m.MenuItemId))
            .ToListAsync();
        if (menuItems.Count != request.MenuItemsIds.Count)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Some of the provided menu items either don't exist or belong to another restaurant",
                PropertyName = nameof(request.MenuItemsIds),
            };
        }

        menu.Name = request.Name.Trim();
        menu.AlternateName = request.AlternateName?.Trim();
        menu.MenuType = request.MenuType;
        menu.DateFrom = request.DateFrom;
        menu.DateUntil = request.DateUntil;
        menu.MenuItems = menuItems;

        var result = await validationService.ValidateAsync(menu, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        await context.SaveChangesAsync();

        return mapper.Map<MenuVM>(menu);
    }

    /// <summary>
    /// Delete a Menu
    /// </summary>
    /// <param name="id">ID of the menu to delete</param>
    /// <param name="user">Currently logged-in user</param>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied, "Menu is not owned by user")]
    public async Task<Result> DeleteMenuAsync(int id, User user)
    {
        var menu = await context.Menus.Where(m => m.MenuId == id)
            .Include(m => m.Restaurant)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync();

        if (menu == null)
        {
            return new ValidationFailure
            {
                ErrorMessage = "Menu not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (menu.Restaurant.Group.OwnerId != user.Id)
        {
            return new ValidationFailure
            {
                ErrorMessage = "Menu is not owned by user",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        menu.IsDeleted = true;
        await context.SaveChangesAsync();

        return Result.Success;
    }


    /// <summary>
    /// Remove MenuItems from a Menu
    /// </summary>
    /// <returns>MenuItem</returns>
    [ErrorCode(nameof(menuId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(req.ItemIds), ErrorCodes.NotFound, "Some of the menu items not found in the menu")]
    public async Task<Result> RemoveMenuItemFromMenuAsync(User user, int menuId, RemoveItemsRequest req)
    {
        var menu = await context.Menus
            .Include(m => m.MenuItems)
            .FirstOrDefaultAsync(m => m.MenuId == menuId && user.Id == m.Restaurant.Group.OwnerId);

        if (menu == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(menuId),
                ErrorMessage = "Menu not found",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var menuItemsToRemove = menu.MenuItems
            .Where(menuItem => req.ItemIds.Contains(menuItem.MenuItemId))
            .ToList();

        if (menuItemsToRemove.Count != req.ItemIds.Count)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(req.ItemIds),
                ErrorMessage = "Some of the menu items not found in the menu",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        foreach (var menuItem in menuItemsToRemove)
        {
            menu.MenuItems.Remove(menuItem);
        }
        await context.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Get list of items of a single menu
    /// </summary>
    /// <param name="menuId">ID of the menu</param>
    /// <param name="name">Search by name</param>
    /// <param name="orderBy">Sorting order</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    public async Task<Result<Pagination<MenuItemSummaryVM>>> GetMenuItemsAsync(
        int menuId, int page, int perPage,
        string? name = null, MenuItemSorting orderBy = MenuItemSorting.PriceDesc)
    {
        var menu = await context.Menus.FirstOrDefaultAsync(x => x.MenuId == menuId);
        if (menu is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Menu with ID {menuId} not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var query = context.Entry(menu)
            .Collection(x => x.MenuItems)
            .Query();

        if (name is not null)
        {
            query = query.Where(x => x.Name.Contains(name));
        }

        query = orderBy switch
        {
            MenuItemSorting.NameAsc => query.OrderBy(x => x.Name),
            MenuItemSorting.NameDesc => query.OrderByDescending(x => x.Name),
            MenuItemSorting.PriceAsc => query.OrderBy(x => x.Price),
            MenuItemSorting.PriceDesc => query.OrderByDescending(x => x.Price),
            MenuItemSorting.AlcoholAsc => query.OrderBy(x => x.AlcoholPercentage),
            MenuItemSorting.AlcoholDesc => query.OrderByDescending(x => x.AlcoholPercentage),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };

        return await query
            .ProjectTo<MenuItemSummaryVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, Enum.GetNames<MenuItemSorting>(), maxPerPage: 20);
    }
}
