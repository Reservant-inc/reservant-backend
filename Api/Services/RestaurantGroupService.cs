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
/// <param name="_context">context</param>
public class RestaurantGroupService(ApiDbContext _context)
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
            if (!await _context.Restaurants.AnyAsync(x => x.Id == id))
            {
                errors.Add(new ValidationResult($"Restaurant: {id} not found"));
                return errors;
            }

        }


        //check if all restaurantIds from request belong to current user
        var restaurants = await _context.Restaurants
                .Where(r => req.RestaurantIds.Contains(r.Id))
                .ToListAsync();

        var notOwnedRestaurants = restaurants.Where(r => r.OwnerId != user.Id);

        if (notOwnedRestaurants != null)
        {
            errors.Add(new ValidationResult(
                $"User is not the owner of restaurants: {String.Join(", ", notOwnedRestaurants.Select(r => r.Id))}"
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

        await _context.RestaurantGroups.AddAsync(group);
        await _context.SaveChangesAsync();
        return group;
    }
}
