using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Models.Dtos.MenuItem;


namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controller resposnible for registration of a new restaurant, listing owned restaurants and accessing restaurant data
    /// </summary>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [ApiController, Route("/my-restaurants")]
    [Authorize(Roles = Roles.Customer)]
    public class MyRestaurantsController(RestaurantService restaurantService, UserManager<User> userManager) : Controller
    {
        /// <summary>
        /// Create a new restaurant (and optionally a new group)
        /// </summary>
        /// <remarks>
        /// If groupId is null, then creates a new group with the same name as the restaurant.
        /// </remarks>
        /// <param name="request"> Create Restaurant Request DTO</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> CreateRestaurant(CreateRestaurantRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.CreateRestaurantAsync(request, user);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
                return ValidationProblem();
            }

            return Ok();
        }
        /// <summary>
        /// Get restaurants owned by the user.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<RestaurantSummaryVM>>> GetMyRestaurants()
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.GetMyRestaurantsAsync(user);
            return Ok(result);
        }
        /// <summary>
        /// Get a specific restaurant owned by the user.
        /// </summary>
        /// <param name="id">Id of the restaurant.</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<ActionResult<RestaurantVM>> GetMyRestaurantById(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.GetMyRestaurantByIdAsync(user, id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Adds an employee to the restaurant
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id:int}/employees")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> AddEmployee(AddEmployeeRequest request, int id)
        {
            var userId = userManager.GetUserId(User);
            var result = await restaurantService.AddEmployeeAsync(request, id, userId!);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }

            return Ok();
        }

        /// <summary>
        /// Terminates an employee's employment by setting the end date to today's date.
        /// </summary>
        /// <param name="id">ID of the restaurant.</param>
        /// <param name="employmentId">ID of the employment to terminate.</param>
        /// <returns>A result indicating success or failure.</returns>
        [HttpDelete("{restaurantId:int}/employees/{employmentId:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> DeleteEmployment(int restaurantId, int employmentId)
        {
            var userId = userManager.GetUserId(User);
            var result = await restaurantService.DeleteEmploymentAsync(restaurantId, employmentId, userId);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }

            return Ok();
        }

        [HttpPost("{id:int}/move-to-group")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<RestaurantSummaryVM>> PostRestaurantToGroup(int id, MoveToGroupRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.MoveRestaurantToGroupAsync(id, request, user);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get list of restaurant's employees
        /// </summary>
        /// <param name="id">ID of the restaurant</param>
        [HttpGet("{id:int}/employees")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<List<RestaurantEmployeeVM>>> GetEmployees(int id)
        {
            var userId = userManager.GetUserId(User);
            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.GetEmployeesAsync(id, userId);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates restaurant info
        /// </summary>
        /// <param name="id">ID of the restaurant</param>
        /// <param name="request">Request with data</param>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RestaurantVM>> EditRestaurantInfo(int id, UpdateRestaurantRequest request)
        {
            var user = await userManager.GetUserAsync(User);

            var result = await restaurantService.UpdateRestaurantAsync(id, request, user);

            if (!result.IsError)
            {
                return Ok(result.Value);
            }
            ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
            return ValidationProblem();
        }

        /// <summary>
        /// Validates input fields for Restaurant Registration process
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("validate-first-step")]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<ActionResult> ValidateFirstStep(ValidateRestaurantFirstStepRequest dto)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.ValidateFirstStepAsync(dto, user!);

            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }

            return Ok();
        }

        /// <summary>
        /// Get list of menus by given restaurant id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:int}/menus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<MenuSummaryVM>>> GetMenusById(int id)
        {
            var result = await restaurantService.GetMenusAsync(id);

            if (result.IsNullOrEmpty()) return NotFound($"Menus with id {id} not found.");

            return Ok(result);
        }

        /// <summary>
        /// Gets menu items from the given restaurant
        /// </summary>
        /// <param name="restaurantId"></param>
        /// <returns>The found list of menuItems</returns>
        [HttpGet("{id:int}/menu-items")]
        [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<ActionResult<List<MenuItemVM>>> GetMenuItems(int id)
        {
            var user = await userManager.GetUserAsync(User);

            var res = await restaurantService.GetMenuItemsAsync(user!, id);

            if (res.IsError)
            {
                ValidationUtils.AddErrorsToModel(res.Errors!, ModelState);
                return ValidationProblem();
            }

            return Ok(res.Value);
        }

        /// <summary>
        /// Delete restaurant
        /// </summary>
        /// <remarks>If the group the restaurant was in is left empty it is also deleted</remarks>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204), ProducesResponseType(404)]
        public async Task<ActionResult> SoftDeleteRestaurant(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await restaurantService.SoftDeleteRestaurantAsync(id, user);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
