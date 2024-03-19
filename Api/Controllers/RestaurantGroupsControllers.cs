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
    /// Controller resposnible for registration of a new restaurant, listing owned restaurants and accessing restaurant data
    /// Only RestaurantOwner can use this controller
    /// </summary>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [ApiController, Route("/my-restaurants-Groups")]
    //[Authorize(Roles = Roles.RestaurantOwner)]
    public class MyRestaurantGroupsController(RestaurantGroupService restaurantGroupService, UserManager<User> userManager) : Controller
    {
        [HttpGet("{id:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<RestaurantGroupSummaryVM>> GetMyRestaurantById(int id) 
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantGroupService.GetRestaurantGroupSummary(user);
            if (result.Value == null)
            {
                return NotFound();
            }
            return Ok(result.Value);
        }
    }
}