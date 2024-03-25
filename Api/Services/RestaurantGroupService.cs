
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Models.Vmodels;


namespace Reservant.Api.Services
{
    public class RestaurantGroupService(ApiDbContext context)
    {
 
        /// <summary>
        /// gets simplification of groups of restaurant based on owner
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<RestaurantGroupSummaryVM>>> GetRestaurantGroupSummary(User user) {
             var userId = user.Id;


            var result = await context
                .RestaurantGroups
                .Where(r => r.OwnerId == userId)
                .Select(r => new RestaurantGroupSummaryVM {
                    Id = r.Id,
                    Name = r.Name,
                    RestaurantCount = r.Restaurants != null ? r.Restaurants.Count() : 0                
            })
                .ToListAsync();

            return result;       
        }
    }
}    