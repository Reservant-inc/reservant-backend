using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

public class RestaurantGroupService(ApiDbContext context)
{
    
    /// <summary>
    /// Updates restaurant group information
    /// </summary>
    public async Task<Result<RestaurantGroup>> UpdateRestaurantGroupAsync(int groupId, UpdateRestaurantGroupRequest request, string userId)
    {
        var restaurantGroup = await context.RestaurantGroups
            .Include(rg => rg.Restaurants)
            .FirstOrDefaultAsync(rg => rg.Id == groupId);

        if (restaurantGroup == null)
        {
            return new Result<RestaurantGroup>([
                new ValidationResult($"RestaurantGroup with ID {groupId} not found.")
            ]);
        }

        if (restaurantGroup.OwnerId != userId)
        {
            return new Result<RestaurantGroup>([
                new ValidationResult($"User with ID {userId} is not an Owner of group {groupId}.")
            ]);
        }

        var requestedRestaurantIds = new HashSet<int>(request.RestaurantIds);
        var currentRestaurantIds = restaurantGroup.Restaurants.Select(r => r.Id).ToHashSet();

        var restaurantsToVerifyOwnership = await context.Restaurants
            .Where(r => requestedRestaurantIds.Contains(r.Id))
            .ToListAsync();

        var notOwnedRestaurantIds = restaurantsToVerifyOwnership
            .Where(r => r.OwnerId != userId)
            .Select(r => r.Id)
            .ToList();

        if (notOwnedRestaurantIds.Any())
        {
            return new Result<RestaurantGroup>([
                new ValidationResult(
                    $"User with ID {userId} is not the Owner of restaurant(s) with IDs: {string.Join(", ", notOwnedRestaurantIds)}")
            ]);
        }

        // Deleting old restaurants
        foreach (var restaurantId in currentRestaurantIds)
        {
            if (!requestedRestaurantIds.Contains(restaurantId))
            {
                var restaurantToRemove = restaurantGroup.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
                if (restaurantToRemove != null)
                {
                    restaurantGroup.Restaurants.Remove(restaurantToRemove);
                }
            }
        }

        // Adding new restaurants
        foreach (var restaurantId in requestedRestaurantIds)
        {
            if (!currentRestaurantIds.Contains(restaurantId))
            {
                var restaurantToAdd = restaurantsToVerifyOwnership.FirstOrDefault(r => r.Id == restaurantId);
                if (restaurantToAdd != null)
                {
                    restaurantGroup.Restaurants.Add(restaurantToAdd);
                }
            }
        }

        try
        {
            // Renaming
            restaurantGroup.Name = request.Name;
            await context.SaveChangesAsync();
            return new Result<RestaurantGroup>(restaurantGroup);
        }
        catch (Exception ex)
        {
            return new Result<RestaurantGroup>([
                new ValidationResult("An error occurred while updating the restaurant group.")
            ]);
        }
    }


}