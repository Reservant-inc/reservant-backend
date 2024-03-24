using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Managing Restaurant Groups
    /// </summary>
    [ApiController, Route("/my-restaurant-groups")]
    public class MyRestaurantGroupsController(RestaurantGroupService groupService, UserManager<User> userManager) : Controller
    {
        /// <summary>
        /// Retrieves information about a specific restaurant group
        /// </summary>
        [HttpGet("{id:int}"), Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RestaurantGroupVM>> GetRestaurantGroup(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await groupService.GetRestaurantGroupAsync(id, user.Id);

            if (!result.IsError)
            {
                return Ok(result.Value);
            }

            return NotFound();
        }
        
        /// <summary>
        /// Deletes a specific restaurant group
        /// </summary>
        [HttpDelete("{id:int}"), Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<bool>> DeleteRestaurantGroup(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await groupService.DeleteRestaurantGroupAsync(id, user.Id);

            if (!result.IsError)
            {
                return Ok(result.Value);
            }

            return NotFound();
        }
    }
}
