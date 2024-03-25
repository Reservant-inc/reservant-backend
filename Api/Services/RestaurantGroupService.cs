using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestaurantGroupService"/> class responsible for managing restaurant groups.
    /// </summary>
    /// <param name="context">The database context.</param>
    public class RestaurantGroupService(ApiDbContext context)
    { 
        /// <summary>
        /// Retrieves information about a specific restaurant group.
        /// </summary>
        /// <param name="groupId">The ID of the restaurant group.</param>
        /// <param name="userId">The ID of the user requesting the information.</param>
        /// <returns>A <see cref="Task{Result}"/> representing the asynchronous operation, containing the result of the operation.</returns>
        public async Task<Result<RestaurantGroupVM>> GetRestaurantGroupAsync(int groupId, string userId)
        {
            var restaurantGroup = await context.RestaurantGroups
                .Include(rg => rg.Restaurants)
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
                    Address = r.Address
                }).ToList()
            });
        }
        /// <summary>
        /// Deletes a restaurant group.
        /// </summary>
        /// <param name="groupId">The ID of the restaurant group to delete.</param>
        /// <param name="userId">The ID of the user requesting the deletion.</param>
        /// <returns>A <see cref="Task{Result}"/> representing the asynchronous operation, containing the result of the operation.</returns>
        public async Task<Result<bool>> DeleteRestaurantGroupAsync(int groupId, string userId)
        {
            var restaurantGroup = await context.RestaurantGroups
                .FirstOrDefaultAsync(rg => rg.Id == groupId && rg.OwnerId == userId);
            
            if (restaurantGroup == null)
            {
                return new Result<bool>([
                    new ValidationResult($"RestaurantGroup with ID {groupId} not found.")
                ]);
            }

            context.RestaurantGroups.Remove(restaurantGroup);
            await context.SaveChangesAsync();

            return new Result<bool>(true);
        }

    }
}