using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;


/// <summary>
/// Util class for managing RestaurantMenus
/// </summary>
/// <param name="context">context</param>
public class RestaurantMenuService(ApiDbContext context)
{
    
    /// <summary>
    /// Returns a list of menus of specific restaurant
    /// </summary>
    /// <param name="id"> Id of the restaurant.</param>
    /// <returns></returns>
    public async Task<List<MenuVM>> GetMenusAsync(int id)
    {
        var menus = await context.Menus
            .Where(m => m.RestaurantId == id)
            .Include(m => m.MenuItems)
            .Select(menu => new MenuVM
            {
                Id = menu.Id,
                MenuType = menu.MenuType,
                DateFrom = menu.DateFrom,
                DateUntil = menu.DateUntil,
                MenuItems = menu.MenuItems.Select(mi => new MenuItemSummaryVM
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Price = mi.Price,
                    AlcoholPercentage = mi.AlcoholPercentage
                }).ToList()
            })
            .ToListAsync();

        return menus;
    }
    
    /// <summary>
    /// Returns a menu with given Id of specific restaurant
    /// </summary>
    /// <param name="menuId"> Id of the menu.</param>
    /// <param name="restaurantId"> Id of the restaurant.</param>
    /// <returns></returns>
    public async Task<MenuVM> GetSingleMenuAsync(int restaurantId, int menuId)
    {
        var menu = await context.Menus
            .Include(m => m.MenuItems)
            .Where(m => m.Id == menuId && m.RestaurantId == restaurantId)
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
    public async Task<Result<Menu>> PostMenuToRestaurant(CreateMenuRequest req)
    {
        var restaurant = await context.Restaurants.FindAsync(req.RestaurantId);
        // var errors = new List<ValidationResult>();
        //     
        // if (restaurant == null)
        // {
        //     errors.Add(new ValidationResult($"Restaurant with id: {req.RestaurantId} not found.", [nameof(req.RestaurantId)]));
        //     return errors;
        // }

        var newMenu = new Menu
        {
            MenuType = req.MenuType,
            DateFrom = req.DateFrom,
            DateUntil = req.DateUntil,
            RestaurantId = req.RestaurantId
        };

        context.Menus.Add(newMenu);
        await context.SaveChangesAsync();

        return newMenu;
    }

    
    
}