    /// <summary>
    /// Service used for getting summary of a group of restaurants 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    // public async Task<RestaurantGroupSummaryVM> GetRestaurantGroupSummary(int ownerID)
    // {
        
    //     try
    //     {
    //         // I want to replace with accesing database and retriving group based ownerID
    //         var summaryVM = new RestaurantGroupSummaryVM
    //         {
    //             Id = 0, 
    //             Name = "aaa", 
    //             RestaurantCount = 0 
    //         };

    //         return summaryVM;
    //     }
    //     catch (Exception ex)
    //     {
    //         return null;
    //     }

    // }



﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Models.Vmodels;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                .Where(r => r.OwnerId == userId)//r=>true
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