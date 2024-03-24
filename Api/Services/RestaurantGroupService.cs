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
    public async Task<Result<RestaurantGroupVM>> UpdateRestaurantGroupAsync(int groupId, UpdateRestaurantGroupRequest request, string userId)
    {
        // Current group
        var restaurantGroup = await context.RestaurantGroups
            .Include(rg => rg.Restaurants)
            .FirstOrDefaultAsync(rg => rg.Id == groupId);
        
        Console.WriteLine("CURRENT GROUP: "+restaurantGroup);
        
        if (restaurantGroup == null)
        {
            return new Result<RestaurantGroupVM>([
                new ValidationResult($"RestaurantGroup with ID {groupId} not found.")
            ]);
        }
        
        // Checking ownership
        if (restaurantGroup.OwnerId != userId)
        {
            return new Result<RestaurantGroupVM>([
                new ValidationResult($"User with ID {userId} is not an Owner of group {groupId}.")
            ]);
        }
        Console.WriteLine("OWNERSHIP: "+ restaurantGroup.OwnerId == userId);

        // Requested restaurants to be added/deleted
        var requestedRestaurantIds = new HashSet<int>(request.RestaurantIds);
        var currentRestaurantIds = new HashSet<int>(restaurantGroup.Restaurants.Select(r => r.Id));

        // Deleting restaurants that aren't part of the group
        var restaurantsToRemove = restaurantGroup.Restaurants
            .Where(r => !requestedRestaurantIds.Contains(r.Id))
            .ToList();

        foreach (var restaurant in restaurantsToRemove)
        {
            restaurantGroup.Restaurants.Remove(restaurant);
        }

        // Adding new restaurants to the group
        foreach (var restaurantId in requestedRestaurantIds.Except(currentRestaurantIds))
        {
            var restaurantToAdd = await context.Restaurants.FindAsync(restaurantId);
            if (restaurantToAdd != null && restaurantToAdd.GroupId == groupId)
            {
                restaurantGroup.Restaurants.Add(restaurantToAdd);
            }
        }

        try
        {
            restaurantGroup.Name = request.Name;
            await context.SaveChangesAsync();

            return new Result<RestaurantGroupVM>(new RestaurantGroupVM
            {
                Id = restaurantGroup.Id,
                Name = restaurantGroup.Name,
                Restaurants = restaurantGroup.Restaurants.Select(restaurant => new RestaurantSummaryVM
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return new Result<RestaurantGroupVM>([
                new ValidationResult("An error occurred while updating the restaurant group.")
            ]);
        }
    }

    
}