using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;

namespace Reservant.Api.Services;


/// <summary>
/// Util class for managing RestaurantMenus
/// </summary>
/// <param name="context">context</param>
public class RestaurantMenuService(ApiDbContext context)
{
    public async Task<List<MenuVM>> GetMenusById(int id)
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
    
}