using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
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
    [ApiController, Route("/my-restaurants")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    public class MyRestaurantsController(RestaurantService restaurantService, SignInManager<User> signInManager, UserManager<User> userManager) : Controller
    {
        /// <summary>
        /// Create a new restaurant and add it to the database
        /// </summary>
        /// <param name="request"> Create Restaurant Request DTO</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> CreateRestaurant(CreateRestaurantRequest request) {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.CreateRestaurantAsync(request, user);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
                return BadRequest(ModelState);
            }

            return Ok();
        }
        /// <summary>
        /// Get restaurants owned by the user.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<List<RestaurantSummaryVM>>> GetMyRestaurants() {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.GetMyRestaurantsAsync(user);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Get a specific restaurant owned by the user.
        /// </summary>
        /// <param name="id">Id of the restaurant.</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<RestaurantVM>> GetMyRestaurantById(int id) {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.GetMyRestaurantByIdAsync(user, id);
            if (result.Value == null)
            {
                return NotFound();
            }
            return Ok(result.Value);
        }
    }
}
