using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services;

/// <summary>
/// Util class for managing RestaurantGroups
/// </summary>
/// <param name="context">context</param>
public class RestaurantGroupService(ApiDbContext context)
{

    /// <summary>
    /// Create restaurant group for the current user
    /// </summary>
    /// <param name="req">Request</param>
    /// <param name="user">Restaurant owner</param>
    /// <returns></returns>
    public async Task<Result<RestaurantGroup>> CreateRestaurantGroup(CreateRestaurantGroupRequest req, User user)
    {

        var errors = new List<ValidationResult>();

        //check if all restaurantIds from request exist
        foreach (var id in req.RestaurantIds)
        {
            if (!await context.Restaurants.AnyAsync(x => x.Id == id))
            {
                errors.Add(new ValidationResult($"Restaurant: {id} not found", [nameof(req.RestaurantIds)]));
                return errors;
            }

        }


        var restaurants = await context.Restaurants
                .Where(r => req.RestaurantIds.Contains(r.Id))
                .Include(r => r.Group)
                .ToListAsync();


        //check if all restaurantIds from request belong to current user
        var notOwnedRestaurants = restaurants.Where(r => r.Group!.OwnerId != user.Id);

        if (notOwnedRestaurants.Any())
        {
            errors.Add(new ValidationResult(
                $"User is not the owner of restaurants: {String.Join(", ", notOwnedRestaurants.Select(r => r.Id))}",
                [nameof(req.RestaurantIds)]
            ));
            return errors;
        }

        var group = new RestaurantGroup
        {
            Name = req.Name,
            OwnerId = user.Id,
            Owner = user,
            Restaurants = restaurants
        };

        if (!ValidationUtils.TryValidate(group, errors))
        {
            return errors;
        }

        await context.RestaurantGroups.AddAsync(group);
        await context.SaveChangesAsync();
        return group;
    }
}
