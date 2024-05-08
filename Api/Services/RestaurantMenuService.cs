using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace Reservant.Api.Services;


/// <summary>
/// Util class for managing RestaurantMenus
/// </summary>
/// <param name="context">context</param>
public class RestaurantMenuService(ApiDbContext context)
{

    /// <summary>
    /// Returns a menu with given Id 
    /// </summary>
    /// <param name="menuId"> Id of the menu.</param>
    /// <returns></returns>
    public async Task<Result<MenuVM?>> GetSingleMenuAsync(int menuId)
    {
        var errors = new List<ValidationResult>();

        var menuExists = await context.Menus.AnyAsync(m => m.Id == menuId);
        if (!menuExists)
            errors.Add(new ValidationResult($"Menu with ID {menuId} not found."));

        if (!errors.IsNullOrEmpty()) return errors;

        var menu = await context.Menus
            .Include(m => m.MenuItems)
            .Where(m => m.Id == menuId)
            .Select(m => new MenuVM
            {
                Id = m.Id,
                MenuType = m.MenuType,
                DateFrom = m.DateFrom,
                DateUntil = m.DateUntil,
                MenuItems = m.MenuItems.Select(mi => new MenuItemSummaryVM
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Price = mi.Price,
                    AlcoholPercentage = mi.AlcoholPercentage
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return menu;
    }


    /// <summary>
    /// Posts menu to the restaurant
    /// </summary>
    /// <param name="req">Request for Menu to be created.</param>
    /// <returns></returns>
    public async Task<Result<MenuSummaryVM>> PostMenuToRestaurant(CreateMenuRequest req, User user)
    {
        var errors = new List<ValidationResult>();

        var restaurant = await context.Restaurants
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == req.RestaurantId);

        if (restaurant == null)
        {
            errors.Add(new ValidationResult($"Restaurant with ID: {req.RestaurantId} not found.", [nameof(req.RestaurantId)]));
        }
        else
        {
            if (restaurant.Group == null || restaurant.Group.OwnerId != user.Id)
                errors.Add(new ValidationResult($"User is not the owner of the restaurant with ID: {req.RestaurantId}.", [nameof(req.RestaurantId)]));
        }

        if (!errors.IsNullOrEmpty()) return errors;


        var newMenu = new Menu
        {
            MenuType = req.MenuType,
            DateFrom = req.DateFrom,
            DateUntil = req.DateUntil,
            RestaurantId = req.RestaurantId
        };

        if (!ValidationUtils.TryValidate(newMenu, errors))
            return errors;

        context.Menus.Add(newMenu);
        await context.SaveChangesAsync();


        var menuSummary = new MenuSummaryVM
        {
            Id = newMenu.Id,
            MenuType = newMenu.MenuType,
            DateFrom = newMenu.DateFrom,
            DateUntil = newMenu.DateUntil
        };

        return menuSummary;
    }


    public async Task<Result<MenuVM>> AddItemsToMenuAsync(int menuId, AddItemsRequest request, User user)
    {

        var errors = new List<ValidationResult>();

        var menuToUpdate = await context.Menus
            .Include(m => m.MenuItems)
            .Include(m => m.Restaurant!)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync(m => m.Id == menuId);

        if (menuToUpdate == null)
        {
            errors.Add(new ValidationResult($"Menu with id ${menuId} not found.", [nameof(menuId)]));
            return errors;
        }

        var restaurant = menuToUpdate.Restaurant!;

        if (restaurant.Group.OwnerId != user.Id)
        {
            errors.Add(new ValidationResult($"User is not the owner of the restaurant that contains menu ID: {menuToUpdate.Id}.", [nameof(menuToUpdate.Id)]));
            return errors;
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
            errors.Add(new ValidationResult(
                $"MenuItems with IDs {string.Join(", ", fromAnotherRestaurant)} belong to another restaurant",
                [nameof(request.ItemIds)]));
            return errors;
        }

        var nonExistentItemIds = request.ItemIds.Except(menuItemsToAdd.Select(item => item.Id)).ToList();
        if (nonExistentItemIds.Any())
        {
            errors.Add(new ValidationResult($"MenuItems with IDs {string.Join(", ", nonExistentItemIds)} not found.", [nameof(request.ItemIds)]));
            return errors;
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

        if (!ValidationUtils.TryValidate(menuToUpdate, errors))
            return errors;

        await context.SaveChangesAsync();

        return new MenuVM
        {
            Id = menuToUpdate.Id,
            MenuType = menuToUpdate.MenuType,
            DateFrom = menuToUpdate.DateFrom,
            DateUntil = menuToUpdate.DateUntil,
            MenuItems = menuToUpdate.MenuItems.Select(mi => new MenuItemSummaryVM
            {
                Id = mi.Id,
                Name = mi.Name,
                Price = mi.Price,
                AlcoholPercentage = mi.AlcoholPercentage
            }).ToList()
        };
    }

    public async Task<Result<MenuVM>> UpdateMenuAsync(UpdateMenuRequest request, int menuId, User user)
    {
        var errors = new List<ValidationResult>();

        // Getting menu
        var menu = await context.Menus
            .Where(m => m.Id == menuId)
            .Include(menu => menu.Restaurant)
            .ThenInclude(restaurant => restaurant!.Group)
            .Include(menu => menu.MenuItems)
            .FirstOrDefaultAsync();

        // Checking if menu exists
        if (menu == null)
        {
            errors.Add(new ValidationResult($"Menu with ID {menuId} not found."));
            return errors;
        }

        // Checking ownership of menu
        if (menu.Restaurant!.Group!.OwnerId != user.Id)
        {
            errors.Add(new ValidationResult($"User not permitted to edit menu with ID {menuId}."));
            return errors;
        }

        menu.MenuType = request.MenuType;
        menu.DateFrom = request.DateFrom;
        menu.DateUntil = request.DateUntil;

        if (!ValidationUtils.TryValidate(menu, errors))
        {
            return errors;
        }

        await context.SaveChangesAsync();

        return new MenuVM()
        {
            DateFrom = menu.DateFrom,
            DateUntil = menu.DateUntil,
            Id = menu.Id,
            MenuItems = menu.MenuItems.Select(mi => new MenuItemSummaryVM
            {
                Id = mi.Id,
                Name = mi.Name,
                Price = mi.Price,
                AlcoholPercentage = mi.AlcoholPercentage
            }).ToList(),
            MenuType = menu.MenuType
        };
    }
    public async Task<Result<bool>> DeleteMenuAsync(int id, User user)
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
        return true;
    }
}
