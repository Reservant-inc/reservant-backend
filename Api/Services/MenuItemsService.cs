﻿using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for creating and finding menu items
    /// </summary>
    /// <param name="context"></param>
    public class MenuItemsService(ApiDbContext context)
    {
        /// <summary>
        /// Validates and creates given menuItems
        /// </summary>
        /// <param name="user">The current user, must be a restaurant owner</param>
        /// <param name="req">MenuItems to be created</param>
        /// <returns>Validation results or the created menuItems</returns>
        public async Task<Result<MenuItemVM>> CreateMenuItemsAsync(User user, CreateMenuItemRequest req)
        {
            var errors = new List<ValidationResult>();

            var isRestaurantValid = await ValidateRestaurant(user, req.RestaurantId);

            if (isRestaurantValid.IsError)
            {
                return isRestaurantValid.Errors;
            }

            var menuItem = new MenuItem()
            {
                Price = req.Price,
                Name = req.Name,
                AlcoholPercentage = req.AlcoholPercentage,
                RestaurantId = req.RestaurantId,
            };


  
            if (!ValidationUtils.TryValidate(menuItem, errors))
            {
                return errors;
            }
            

            await context.MenuItems.AddRangeAsync(menuItem);
            await context.SaveChangesAsync();

            return new MenuItemVM()
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                AlcoholPercentage = menuItem.AlcoholPercentage,
            };

        }


        /// <summary>
        /// Validates and gets menu item by given id
        /// </summary>
        /// <param name="user"></param>
        /// <param name="restaurantId"></param>
        /// <param name="menuItemId"></param>
        /// <returns>MenuItem</returns>
        public async Task<Result<MenuItemVM>> GetMenuItemByIdAsync(User user, int restaurantId, int menuItemId)
        {
            var errors = new List<ValidationResult>();

            var isRestaurantValid = await ValidateRestaurant(user, restaurantId);

            if (isRestaurantValid.IsError)
            {
                return isRestaurantValid.Errors;
            }

            var item = await context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == menuItemId && i.RestaurantId == restaurantId);

            if (item == null)
            {
                errors.Add(new ValidationResult(
                   $"MenuItem: {menuItemId} not found"
                ));
                return errors;
            }

            return new MenuItemVM()
            {
                Id= item.Id,
                Name = item.Name,
                Price = item.Price,
                AlcoholPercentage = item.AlcoholPercentage,
            };

        }

        public async Task<Result<bool>> ValidateRestaurant(User user, int restaurantId)
        {
            var errors = new List<ValidationResult>();

            var restaurant = await context.Restaurants
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                errors.Add(new ValidationResult(
                    $"Restaurant: {restaurantId} not found."
                ));
                return errors;
            }

            if (restaurant.Group!.OwnerId != user.Id)
            {
                errors.Add(new ValidationResult(
                    $"Restaurant: {restaurantId} doesn't belong to the restautantOwner."
                ));
                return errors;
            }

            return true;
        }


        /// <summary>
        /// Deletes Menue
        /// </summary>
        /// <param name="user"></param>
        /// <param name="menueId"></param>
        /// <param name="menuItemId"></param>
        /// <returns>MenuItem</returns>
        public async Task<bool> RemoveMenuItemFromMenuAsync(User user, int menuId, int menuItemId)
        {
            var menuItem = await context
                .MenuItems
                .Include(m => m.Menus)
                .ThenInclude(m => m.Restaurant)
                .ThenInclude(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == menuItemId);

            if (menuItem == null)
                return false;

            var menu = menuItem.Menus.FirstOrDefault(m => m.Id == menuId);

            if (menu == null)
                return false;

            if (menuItem.Restaurant == null || menuItem.Restaurant.Group == null || menuItem.Restaurant.Group.Owner == null)
            {
                return false;
            }

            if (menuItem.Restaurant.Group.Owner.Id != user.Id) 
                return false;

            menuItem.Menus.Remove(menu);

            await context.SaveChangesAsync();

            return true;
        }

    }
}
