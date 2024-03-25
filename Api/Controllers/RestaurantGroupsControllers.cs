using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Vmodels;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controler for getting restaurantGroups of restaurant owner
    /// Only RestaurantOwner can use this controller
    /// </summary>
    /// <request code="404"> Not Found </request>
    [ApiController, Route("/my-restaurants-groups")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    public class MyRestaurantGroupsController(RestaurantGroupService restaurantGroupService, SignInManager<User> signInManager, UserManager<User> userManager) : Controller
    {

        /// <summary>
        /// Get a group of restaurants asociated with logged in restaurant owner
        /// </summary>
        /// <returns>RestaurantGroupSummaryVM</returns>
        [HttpGet()]
        [ProducesResponseType(200)]
        public async Task<ActionResult<RestaurantGroupSummaryVM>> GetMyRestaurantById() 
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await restaurantGroupService.GetRestaurantGroupSummary(user);
            if (result.Value == null)
            {
                return NotFound();
            }
            return Ok(result.Value);
        }
    }
}