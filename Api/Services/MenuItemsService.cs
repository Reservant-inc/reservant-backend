﻿using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    public class MenuItemsService(ApiDbContext context)
    {

        public async Task<Result<List<MenuItem>>> CreateMenuItemsAsync(User user, int restaurantId, List<CreateMenuItemRequest> req)
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

            var menuItems = req.Select(item => new MenuItem()
            {
                Price = item.Price,
                Name = item.Name,
                AlcoholPercentage = item.AlcoholPercentage,
                RestaurantId = restaurantId,
            }).ToList();

            foreach (var item in req)
            {
                if (!ValidationUtils.TryValidate(item, errors))
                {
                    return errors;
                }
            }

            await context.MenuItems.AddRangeAsync(menuItems);
            await context.SaveChangesAsync();

            return menuItems;

        }

    }
}