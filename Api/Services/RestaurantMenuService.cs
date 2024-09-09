﻿using ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Menu;
using Reservant.Api.Dtos.MenuItem;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;


/// <summary>
/// Util class for managing RestaurantMenus
/// </summary>
public class RestaurantMenuService(
    ApiDbContext context,
    FileUploadService uploadService,
    ValidationService validationService)
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
            .Where(m => m.Id == menuId)
            .Select(m => new MenuVM
            {
                MenuId = m.Id,
                Name = m.Name,
                AlternateName = m.AlternateName,
                MenuType = m.MenuType,
                DateFrom = m.DateFrom,
                DateUntil = m.DateUntil,
                Photo = uploadService.GetPathForFileName(m.PhotoFileName),
                MenuItems = m.MenuItems.Select(mi => new MenuItemSummaryVM
                {
                    MenuItemId = mi.Id,
                    Name = mi.Name,
                    AlternateName = mi.AlternateName,
                    Price = mi.Price,
                    AlcoholPercentage = mi.AlcoholPercentage,
                    Photo = uploadService.GetPathForFileName(mi.PhotoFileName)
                }).ToList()
            })
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
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == req.RestaurantId);

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
            PhotoFileName = req.Photo,
            RestaurantId = req.RestaurantId
        };

        var result = await validationService.ValidateAsync(newMenu, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.Menus.Add(newMenu);
        await context.SaveChangesAsync();


        var menuSummary = new MenuSummaryVM
        {
            MenuId = newMenu.Id,
            Name = newMenu.Name,
            AlternateName = newMenu.AlternateName,
            MenuType = newMenu.MenuType,
            Photo = uploadService.GetPathForFileName(newMenu.PhotoFileName),
            DateFrom = newMenu.DateFrom,
            DateUntil = newMenu.DateUntil
        };

        return menuSummary;
    }


    /// <summary>
    /// Add MenuItems to a Menu
    /// </summary>
    /// <param name="menuId">ID of the Menu</param>
    /// <param name="request">Information about the items</param>
    /// <param name="user">Currently logged-in user</param>
    [ErrorCode("<menuId>", ErrorCodes.NotFound)]
    [ErrorCode(nameof(Menu.Id), ErrorCodes.AccessDenied, "User is not the owner of the restaurant that contains menu ID")]
    [ErrorCode(nameof(AddItemsRequest.ItemIds), ErrorCodes.BelongsToAnotherRestaurant, "MenuItems with IDs belong to another restaurant")]
    [ErrorCode(nameof(AddItemsRequest.ItemIds), ErrorCodes.NotFound)]
    [ValidatorErrorCodes<Menu>]
    public async Task<Result<MenuVM>> AddItemsToMenuAsync(int menuId, AddItemsRequest request, User user)
    {
        var menuToUpdate = await context.Menus
            .Include(m => m.MenuItems)
            .Include(m => m.Restaurant)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync(m => m.Id == menuId);

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
                PropertyName = nameof(menuToUpdate.Id),
                ErrorMessage = $"User is not the owner of the restaurant that contains menu ID: {menuToUpdate.Id}.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var menuItemsToAdd = await context.MenuItems
            .Where(item => request.ItemIds.Contains(item.Id))
            .ToListAsync();

        var fromAnotherRestaurant = menuItemsToAdd
            .Where(mi => mi.RestaurantId != menuToUpdate.RestaurantId)
            .Select(mi => mi.Id)
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

        var nonExistentItemIds = request.ItemIds.Except(menuItemsToAdd.Select(item => item.Id)).ToList();
        if (nonExistentItemIds.Any())
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
            if (!menuToUpdate.MenuItems.Any(mi => mi.Id == item.Id))
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

        return new MenuVM
        {
            MenuId = menuToUpdate.Id,
            Name = menuToUpdate.Name,
            AlternateName = menuToUpdate.AlternateName,
            MenuType = menuToUpdate.MenuType,
            DateFrom = menuToUpdate.DateFrom,
            DateUntil = menuToUpdate.DateUntil,
            Photo = uploadService.GetPathForFileName(menuToUpdate.PhotoFileName),
            MenuItems = menuToUpdate.MenuItems.Select(mi => new MenuItemSummaryVM
            {
                MenuItemId = mi.Id,
                Name = mi.Name,
                AlternateName = mi.AlternateName,
                Price = mi.Price,
                AlcoholPercentage = mi.AlcoholPercentage,
                Photo = uploadService.GetPathForFileName(mi.PhotoFileName)
            }).ToList()
        };
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
    [ValidatorErrorCodes<Menu>]
    public async Task<Result<MenuVM>> UpdateMenuAsync(UpdateMenuRequest request, int menuId, User user)
    {
        // Getting menu
        var menu = await context.Menus
            .Where(m => m.Id == menuId)
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

        menu.Name = request.Name.Trim();
        menu.AlternateName = request.AlternateName?.Trim();
        menu.MenuType = request.MenuType;
        menu.PhotoFileName = request.Photo.Trim();
        menu.DateFrom = request.DateFrom;
        menu.DateUntil = request.DateUntil;

        var result = await validationService.ValidateAsync(menu, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        await context.SaveChangesAsync();

        return new MenuVM()
        {
            Name = menu.Name,
            AlternateName = menu.AlternateName,
            DateFrom = menu.DateFrom,
            DateUntil = menu.DateUntil,
            Photo = uploadService.GetPathForFileName(menu.PhotoFileName),
            MenuId = menu.Id,
            MenuItems = menu.MenuItems.Select(mi => new MenuItemSummaryVM
            {
                MenuItemId = mi.Id,
                Name = mi.Name,
                AlternateName = mi.AlternateName,
                Price = mi.Price,
                AlcoholPercentage = mi.AlcoholPercentage,
                Photo = uploadService.GetPathForFileName(mi.PhotoFileName)
            }).ToList(),
            MenuType = menu.MenuType
        };
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
        var menu = await context.Menus.Where(m => m.Id == id)
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

        context.Remove(menu);
        await context.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Indicates an error returned from RemoveMenuItemFromMenuAsync
    /// </summary>
    public enum RemoveMenuItemResult
    {
        /// <summary>
        /// Success
        /// </summary>
        Success,

        /// <summary>
        /// Menu not found
        /// </summary>
        MenuNotFound,

        /// <summary>
        /// Bad request
        /// </summary>
        NoValidMenuItems
    }

    /// <summary>
    /// Remove a MenuItem from a Menu
    /// </summary>
    /// <returns>MenuItem</returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    public async Task<RemoveMenuItemResult> RemoveMenuItemFromMenuAsync(User user, int menuId, RemoveItemsRequest req)
    {
        var menuItemIds = req.ItemIds;

        var menu = await context.Menus
            .FirstOrDefaultAsync(m => m.Id == menuId && user.Id == m.Restaurant.Group.OwnerId);

        if (menu == null)
        {
            return RemoveMenuItemResult.MenuNotFound;
        }


        var menuItems = await context.Entry(menu)
                .Collection(m => m.MenuItems)
                .Query()
                .Where(m => menuItemIds.Contains(m.Id))
                .ToListAsync();


        if (menuItems.Count == 0)
        {
            return RemoveMenuItemResult.NoValidMenuItems;
        }

        menuItems.ForEach(m => m.Menus.Remove(menu));

        await context.SaveChangesAsync();

        return RemoveMenuItemResult.Success;
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
        var menu = await context.Menus.FirstOrDefaultAsync(x => x.Id == menuId);
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
            .Select(mi => new MenuItemSummaryVM
            {
                MenuItemId = mi.Id,
                Name = mi.Name,
                AlternateName = mi.AlternateName,
                Price = mi.Price,
                AlcoholPercentage = mi.AlcoholPercentage,
                Photo = uploadService.GetPathForFileName(mi.PhotoFileName)
            })
            .PaginateAsync(page, perPage, Enum.GetNames<MenuItemSorting>(), maxPerPage: 20);
    }
}
