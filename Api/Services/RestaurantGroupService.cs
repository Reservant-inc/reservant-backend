using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.RestaurantGroup;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Restaurant;


namespace Reservant.Api.Services;

/// <summary>
/// Util class for managing RestaurantGroups
/// </summary>
/// <param name="context">context</param>
public class RestaurantGroupService(ApiDbContext context, FileUploadService uploadService)
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
            Name = req.Name.Trim(),
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

        await DeleteEmptyRestaurantGroups();

        return group;
    }

    /// <summary>
    /// Deletes empty groups
    /// </summary>
    /// <returns></returns>
    public async Task DeleteEmptyRestaurantGroups()
    {
        // Pobranie wszystkich grup restauracji
        var allGroups = await context.RestaurantGroups
            .Include(g => g.Restaurants)
            .ToListAsync();

        // Filtracja grup, kt�re s� puste (nie maj� restauracji)
        var emptyGroups = allGroups.Where(g => !g.Restaurants.Any()).ToList();

        if (emptyGroups.Any())
        {
            context.RestaurantGroups.RemoveRange(emptyGroups);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// gets simplification of groups of restaurant based on owner
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<List<RestaurantGroupSummaryVM>>> GetUsersRestaurantGroupSummary(User user)
    {
        var userId = user.Id;


        var result = await context
            .RestaurantGroups
            .Where(r => r.OwnerId == userId && !r.IsDeleted)
            .Select(r => new RestaurantGroupSummaryVM
            {
                Id = r.Id,
                Name = r.Name,
                RestaurantCount = r.Restaurants != null ? r.Restaurants.Count() : 0
            })
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// Retrieves information about a specific restaurant group.
    /// </summary>
    /// <param name="groupId">The ID of the restaurant group.</param>
    /// <param name="userId">The ID of the user requesting the information.</param>
    /// <returns>A <see cref="Task{Result}"/> representing the asynchronous operation, containing the result of the operation.</returns>
    public async Task<Result<RestaurantGroupVM>> GetRestaurantGroupAsync(int groupId, string userId)
    {
        var restaurantGroup = await context.RestaurantGroups
            .Include(rg => rg.Restaurants!)
            .ThenInclude(rg => rg.Tags!)
            .FirstOrDefaultAsync(rg => rg.Id == groupId && rg.OwnerId == userId);

        if (restaurantGroup == null)
        {
            return new Result<RestaurantGroupVM>([
                new ValidationResult($"RestaurantGroup with ID {groupId} not found.")
            ]);
        }

        return new Result<RestaurantGroupVM>(new RestaurantGroupVM
        {
            Id = restaurantGroup.Id,
            Name = restaurantGroup.Name,
            Restaurants = restaurantGroup.Restaurants.Select(r => new RestaurantSummaryVM
            {
                Id = r.Id,
                Name = r.Name,
                Nip = r.Nip,
                Address = r.Address,
                RestaurantType = r.RestaurantType,
                City = r.City,
                GroupId = r.GroupId,
                Logo = uploadService.GetPathForFileName(r.LogoFileName),
                Description = r.Description,
                ProvideDelivery = r.ProvideDelivery,
                Tags = r.Tags!.Select(t => t.Name).ToList(),
                IsVerified = r.VerifierId != null
            }).ToList()
        });
    }

    /// <summary>
    /// Updates restaurant group name.
    /// </summary>
    /// <param name="groupId">The ID of the restaurant group.</param>
    /// <param name="request">Request containing restaurant name.</param>
    /// <param name="userId">Id of user calling the method</param>
    public async Task<Result<RestaurantGroupVM>> UpdateRestaurantGroupAsync(int groupId, UpdateRestaurantGroupRequest request, string userId)
    {
        var errors = new List<ValidationResult>();
        var restaurantGroup = await context.RestaurantGroups
            .Include(restaurantGroup => restaurantGroup.Restaurants)!
            .ThenInclude(restaurant => restaurant.Tags!)
            .FirstOrDefaultAsync(rg => rg.Id == groupId);


        if (restaurantGroup == null)
        {
            errors.Add(new ValidationResult($"RestaurantGroup with ID {groupId} not found."));
            return errors;
        }

        if (restaurantGroup.OwnerId != userId)
        {
            errors.Add(new ValidationResult($"User with ID {userId} is not an Owner of group {groupId}."));
            return errors;
        }



        restaurantGroup.Name = request.Name.Trim();

        if (!ValidationUtils.TryValidate(restaurantGroup, errors))
        {
            return errors;
        }

        await context.SaveChangesAsync();

        return new RestaurantGroupVM
        {
            Id = restaurantGroup.Id,
            Name = restaurantGroup.Name,
            Restaurants = restaurantGroup.Restaurants.Select(r => new RestaurantSummaryVM
            {
                Id = r.Id,
                Name = r.Name,
                Nip = r.Nip,
                Address = r.Address,
                RestaurantType = r.RestaurantType,
                City = r.City,
                GroupId = r.GroupId,
                Logo = uploadService.GetPathForFileName(r.LogoFileName),
                Description = r.Description,
                ProvideDelivery = r.ProvideDelivery,
                Tags = r.Tags!.Select(t => t.Name).ToList(),
                IsVerified = r.VerifierId != null
            }).ToList()
        };
    }

    /// <summary>
    /// Function for deleteing entire restaurant group in one go
    /// </summary>
    /// <param name="id">id of the restaurant group</param>
    /// <param name="user">owner of the restaurant group</param>
    /// <returns></returns>
    public async Task<Result<bool>> SoftDeleteRestaurantGroupAsync(int id, User user)
    {
        var errors = new List<ValidationResult>();
        var group = await context.RestaurantGroups
            .Where(g => g.Id == id)
            .Include(g => g.Restaurants)
            .FirstOrDefaultAsync();
        if (group == null)
        {
            errors.Add(new ValidationResult("Restaurant group not found"));
            return errors;
        }
        if (group.OwnerId != user.Id) {
            errors.Add(new ValidationResult("Restaurant group does not belong "));
            return errors; }

        foreach (Restaurant restaurant in group.Restaurants)
        {
            context.Remove(restaurant);
        }

        context.Remove(group);
        await context.SaveChangesAsync();
        return true;

    }
}