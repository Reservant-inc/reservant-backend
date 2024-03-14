﻿using Microsoft.AspNetCore.Identity;
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
        public async Task<Result<Restaurant>> CreateRestaurantAsync(CreateRestaurantRequest request, User user) {
            
            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                OwnerId = user.Id,
                Owner = user
            };


            var errors = new List<ValidationResult>();
            if (!ValidationUtils.TryValidate(restaurant, errors))
            {
                return errors;
            }
            
            context.Add(restaurant);
            
            if (request.Tables.Count > 0)
            {
                var restaurantId = context.Restaurants.LastAsync().Result.Id;
                var tables = request.Tables.Select(t => new Table
                {
                    RestaurantId = restaurantId,
                    Capacity = t.Capacity,
                    Id = t.Id
                });
            }
                
            
            await context.SaveChangesAsync();

            return restaurant;
        }

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

        public async Task<Result<RestaurantVM>> GetMyRestaurantByIdAsync(User user, int id)
        {
            var userId = user.Id;
/*            var restaurant = await context.Restaurants.Where(r => r.Id == id)
                                                    .Where(r => r.OwnerId == userId)
                                                    .FirstOrDefaultAsync();
            var errors = new List<ValidationResult>();
            if (!ValidationUtils.TryValidate(restaurant, errors))
            {
                return errors;
            }
            List<Table> tables;
            if (restaurant.Tables == null) 
            { 
                tables = new List<Table>();
            } else
            {
                tables = restaurant.Tables;
            }
            

            var tablesVM = new List<TableVM>();
            foreach(var table in restaurant.Tables)
            {
                tablesVM.Add(
                    new TableVM
                    {
                        Id = table.Id,
                        Capacity = table.Capacity
                    });
            }*/


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
