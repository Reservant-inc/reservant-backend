using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;
using Reservant.Api.Options;
using Microsoft.Extensions.Options;
using Reservant.Api.Identity;

using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using Reservant.Api.Identity;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Location;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.Review;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validators;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Models.Dtos.User;


namespace Reservant.Api.Services
{
    /// <summary>
    /// Service responsible for managing user authentication
    /// </summary>
    public class AuthorizationService(
        UserManager<User> userManager, 
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
}
