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
     

            // var result = await context.Restaurants.Where(r => r.OwnerId == userId)
            //                                       .Select(r=> new RestaurantSummaryVM{
            //                                         Id = r.Id,
            //                                         Name = r.Name,
            //                                         Address = r.Address
            //                                       })
            //                                       .ToListAsync();
            // if (result.Count == 0)
            //     return null;
            // return result;

        public async Task<Result<IEnumerable<RestaurantGroupSummaryVM>>> GetRestaurantGroupSummary(string ID) {
            // var userId = user.Id;


            var result = await context
                .RestaurantGroups
                .Where(r => r.OwnerId == ID)//r=>true
                .Select(r => new RestaurantGroupSummaryVM {
                    Id = r.Id,
                    Name = r.Name,
                    RestaurantCount = r.Restaurants != null ? r.Restaurants.Count() : 0                
            })
                .ToListAsync();

            return result;

            //   var users = await context.Users.ToListAsync();


            //     string aaaa = "";

            //     foreach (var user in users)
            //     {
            //         aaaa += $"User ID: {user.Id}, First Name: {user.FirstName}, Last Name: {user.LastName}, Email: {user.Email}, Phone Number: {user.PhoneNumber}\n";
            //     }



            // try
            //     {
            //         var summaryVM = new RestaurantGroupSummaryVM
            //         {
            //             Id = 0, 
            //             Name = "aaa"+aaaa, 
            //             RestaurantCount = 0 
            //         };

            //         var resultList = new List<RestaurantGroupSummaryVM> { summaryVM };
            //         var result = new Result<IEnumerable<RestaurantGroupSummaryVM>>(resultList);
            //         return result;

            //     }
            //     catch (Exception ex)
            //     {
            //         return null;
            //     }
        
        }


                
        

    }
}    