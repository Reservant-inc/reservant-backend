using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Reservant.Api.Services
{
    public class RestaurantService(ApiDbContext context)
    {
        /// <summary>
        /// Register new Restaurant and add tables.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<Restaurant>> CreateRestaurantAsync(CreateRestaurantRequest request, User user) {

            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                OwnerId = user.Id,
                Owner = user,
                Tables = request.Tables.Count > 0 ? request.Tables.Select(t => new Table 
                { 
                    Capacity = t.Capacity
                }).ToList() : new List<Table>(),
            };

            foreach (var table in restaurant.Tables) 
            { 
                table.Restaurant = restaurant;
            }


            var errors = new List<ValidationResult>();
            if (!ValidationUtils.TryValidate(restaurant, errors))
            {
                return errors;
            }
            
            context.Add(restaurant);                
            
            await context.SaveChangesAsync();

            return restaurant;
        }
        /// <summary>
        /// Returns a list of restaurants owned by the user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<RestaurantSummaryVM>>> GetMyRestaurantsAsync(User user) {
            var userId = user.Id;
            var result = await context.Restaurants.Where(r => r.OwnerId == userId)
                                                  .Select(r=> new RestaurantSummaryVM{
                                                    Id = r.Id,
                                                    Name = r.Name,
                                                    Address = r.Address
                                                  })
                                                  .ToListAsync();
            var errors = new List<ValidationResult>();
            if (!ValidationUtils.TryValidate(result, errors))
            {
                return errors;
            }

            return result;
        }
        /// <summary>
        /// Returns a specific restaurant owned by the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"> Id of the restaurant.</param>
        /// <returns></returns>
        public async Task<Result<RestaurantVM>> GetMyRestaurantByIdAsync(User user, int id)
        {
            var userId = user.Id;
            var result = await context.Restaurants.Where(r => r.OwnerId == userId)
                                                  .Where(r => r.Id == id)
                                                  .Select(r => new RestaurantVM { 
                                                    Id = r.Id,
                                                    Name = r.Name,
                                                    Address = r.Address,
                                                    Tables = r.Tables.Select(t => new TableVM { 
                                                                            Id = t.Id,
                                                                            Capacity = t.Capacity})
                                                  })
                                                  .FirstOrDefaultAsync();
            var errors = new List<ValidationResult>();
            if (!ValidationUtils.TryValidate(result, errors))
            {
                return errors;
            }

            return result;
        }
    }
}
