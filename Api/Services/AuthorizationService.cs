using Reservant.Api.Models;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services;

/// <summary>
/// Service responsible for managing user authorization
/// </summary>
public class AuthorizationService(
    ApiDbContext context
    )
{
    /// <summary>
    /// returns if user is an owner of a restaurant
    /// </summary>
    /// <param name="restaurantId">The id of restaurant</param>
    /// <param name="user">The user to be tested as owner</param>
    public async Task<Result<bool>> VerifyOwnerRole(int restaurantId, User user)
    {
        var restaurant = await context
            .Restaurants
            .Include(x => x.Group)
            .FirstOrDefaultAsync(r => r.Id == restaurantId && r.Group.OwnerId == user.Id);
        
        if (restaurant == null)
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User is not the owner of the restaurant.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        return true;
    }

    /// <summary>
    /// Verify that the user is a backdoor employee or the owner of the restaurant.
    /// Return a validation error if not.
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="userId">ID of the user</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    public async Task<Result> VerifyRestaurantBackdoorAccess(int restaurantId, string userId)
    {
        var userHasBackdoorsAccess = await context.Restaurants
            .Where(r => r.Id == restaurantId)
            .Select(r => r.Group.OwnerId == userId
                || r.Employments.Any(e =>
                    e.DateUntil == null && e.EmployeeId == userId && e.IsBackdoorEmployee))
            .SingleOrDefaultAsync();
        if (!userHasBackdoorsAccess)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User must either be a backdoor employee or the owner of the restaurant",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        return Result.Success;
    }
}
