using Reservant.Api.Models;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;

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
}
