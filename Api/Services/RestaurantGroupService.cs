using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.RestaurantGroups;

namespace Reservant.Api.Services;

/// <summary>
/// Util class for managing RestaurantGroups
/// </summary>
public class RestaurantGroupService(
    ApiDbContext context,
    RestaurantService restaurantService,
    ValidationService validationService,
    IMapper mapper)
{
    /// <summary>
    /// Create restaurant group for the current user
    /// </summary>
    /// <param name="req">Request</param>
    /// <param name="user">Restaurant owner</param>
    /// <returns></returns>
    public async Task<Result<RestaurantGroupVM>> CreateRestaurantGroup(CreateRestaurantGroupRequest req, User user)
    {
        //check if all restaurantIds from request exist
        foreach (var id in req.RestaurantIds)
        {
            if (!await context.Restaurants.AnyAsync(x => x.RestaurantId == id))
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(req.RestaurantIds),
                    ErrorMessage = $"Restaurant: {id} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

        }


        var restaurants = await context.Restaurants
                .Where(r => req.RestaurantIds.Contains(r.RestaurantId))
                .Include(r => r.Group)
                .Include(r => r.Tags)
                .Include(r => r.Reviews)
                .ToListAsync();


        //check if all restaurantIds from request belong to current user
        var notOwnedRestaurants = restaurants
            .Where(r => r.Group.OwnerId != user.Id)
            .ToList();

        if (notOwnedRestaurants.Count != 0)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(req.RestaurantIds),
                ErrorMessage =
                    $"User is not the owner of restaurants: {String.Join(", ", notOwnedRestaurants.Select(r => r.RestaurantId))}",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var group = new RestaurantGroup
        {
            Name = req.Name.Trim(),
            OwnerId = user.Id,
            Owner = user,
            Restaurants = restaurants
        };

        var result = await validationService.ValidateAsync(group, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        await context.RestaurantGroups.AddAsync(group);
        await context.SaveChangesAsync();

        await DeleteEmptyRestaurantGroups();

        return mapper.Map<RestaurantGroupVM>(group);
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
        var emptyGroups = allGroups.Where(g => g.Restaurants.Count == 0).ToList();

        if (emptyGroups.Count != 0)
        {
            foreach (var emptyGroup in emptyGroups)
            {
                emptyGroup.IsDeleted = true;
            }
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
            .Where(r => r.OwnerId == userId)
            .ProjectTo<RestaurantGroupSummaryVM>(mapper.ConfigurationProvider)
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// Retrieves information about a specific restaurant group.
    /// </summary>
    /// <param name="groupId">The ID of the restaurant group.</param>
    /// <param name="userId">The ID of the user requesting the information.</param>
    /// <returns>A <see cref="Task{Result}"/> representing the asynchronous operation, containing the result of the operation.</returns>
    public async Task<Result<RestaurantGroupVM>> GetRestaurantGroupAsync(int groupId, Guid userId)
    {
        var restaurantGroup = await context.RestaurantGroups
            .Include(rg => rg.Restaurants)
            .ThenInclude(rg => rg.Tags)
            .Include(rg => rg.Restaurants)
            .ThenInclude(r => r.Reviews)
            .FirstOrDefaultAsync(rg => rg.RestaurantGroupId == groupId && rg.OwnerId == userId);

        if (restaurantGroup == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"RestaurantGroup with ID {groupId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return new Result<RestaurantGroupVM>(mapper.Map<RestaurantGroupVM>(restaurantGroup));
    }

    /// <summary>
    /// Updates restaurant group name.
    /// </summary>
    /// <param name="groupId">The ID of the restaurant group.</param>
    /// <param name="request">Request containing restaurant name.</param>
    /// <param name="userId">Id of user calling the method</param>
    public async Task<Result<RestaurantGroupVM>> UpdateRestaurantGroupAsync(int groupId, UpdateRestaurantGroupRequest request, Guid userId)
    {
        var restaurantGroup = await context.RestaurantGroups
            .Include(restaurantGroup => restaurantGroup.Restaurants)
            .ThenInclude(restaurant => restaurant.Tags)
            .Include(restaurantGroup => restaurantGroup.Restaurants)
            .ThenInclude(restaurant => restaurant.Reviews)
            .FirstOrDefaultAsync(rg => rg.RestaurantGroupId == groupId);


        if (restaurantGroup == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"RestaurantGroup with ID {groupId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (restaurantGroup.OwnerId != userId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"User with ID {userId} is not an Owner of group {groupId}.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }



        restaurantGroup.Name = request.Name.Trim();

        var result = await validationService.ValidateAsync(restaurantGroup, userId);
        if (!result.IsValid)
        {
            return result;
        }

        await context.SaveChangesAsync();

        return mapper.Map<RestaurantGroupVM>(restaurantGroup);
    }

    /// <summary>
    /// Function for deleteing entire restaurant group in one go
    /// </summary>
    /// <param name="id">id of the restaurant group</param>
    /// <param name="user">owner of the restaurant group</param>
    /// <returns></returns>
    public async Task<Result> SoftDeleteRestaurantGroupAsync(int id, User user)
    {
        var group = await context.RestaurantGroups
            .Where(g => g.RestaurantGroupId == id)
            .Include(g => g.Restaurants)
            .FirstOrDefaultAsync();
        if (group == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Restaurant group not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }
        if (group.OwnerId != user.Id)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Restaurant group does not belong to the current user",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        foreach (Restaurant restaurant in group.Restaurants)
        {
            var res = await restaurantService.SoftDeleteRestaurantAsync(restaurant.RestaurantId, user);
            if (res.IsError)
            {
                return res.Errors;
            }
        }
        var emptyGroup = await context.RestaurantGroups.FindAsync(group.RestaurantGroupId);
        if (emptyGroup != null)
        {
            group.IsDeleted = true;
        }
        await context.SaveChangesAsync();
        return Result.Success;

    }
}
