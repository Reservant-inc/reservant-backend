using Reservant.Api.Models;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

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
    /// <param name="userId">The Id of user to be tested as owner</param>
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    public async Task<Result> VerifyOwnerRole(int restaurantId, Guid userId)
    {
        var restaurant = await context
            .Restaurants
            .Include(x => x.Group)
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId && r.Group.OwnerId == userId);

        if (restaurant == null)
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User is not the owner of the restaurant.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        return Result.Success;
    }

    /// <summary>
    /// Verify that the user is a backdoor employee or the owner of the restaurant.
    /// Return a validation error if not.
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="userId">ID of the user</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    public async Task<Result> VerifyRestaurantBackdoorAccess(int restaurantId, Guid userId)
    {
        var userHasBackdoorsAccess = await context.Restaurants
            .Where(r => r.RestaurantId == restaurantId)
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

    /// <summary>
    /// Verify that the user is the visit's participant
    /// </summary>
    /// <param name="visitId">ID of the visit</param>
    /// <param name="userId">ID of the user</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.AccessDenied, "User does not participate in the visit")]
    public async Task<Result> VerifyVisitParticipant(int visitId, Guid userId)
    {
        var userIsVisitParticipant = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Select(v => v.ClientId == userId
                || v.Participants.Any(p => p.Id == userId))
            .SingleOrDefaultAsync();
        if (!userIsVisitParticipant)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User does not participate in the visit",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        return Result.Success;
    }
}
