using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Services.ReportServices;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controller resposnible for registration of a new restaurant, listing owned restaurants and accessing restaurant data
    /// </summary>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [ApiController, Route("/my-restaurants")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    public class MyRestaurantsController(
        RestaurantService restaurantService,
        UserManager<User> userManager)
        : StrictController
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
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.CreateRestaurantAsync))]
        public async Task<ActionResult<MyRestaurantVM>> CreateRestaurant(CreateRestaurantRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.CreateRestaurantAsync(request, user);
            return OkOrErrors(result);
        }
        /// <summary>
        /// Get restaurants owned by the user.
        /// </summary>
        /// <param name="name">Search by name</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<RestaurantSummaryVM>>> GetMyRestaurants(string? name = null)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.GetMyRestaurantsAsync(user, name);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific restaurant owned by the user.
        /// </summary>
        /// <param name="restaurantId">Id of the restaurant.</param>
        /// <returns></returns>
        [HttpGet("{restaurantId:int}")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<ActionResult<MyRestaurantVM>> GetMyRestaurantById(int restaurantId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.GetMyRestaurantByIdAsync(user, restaurantId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Adds a list of employees to the restaurant
        /// </summary>
        /// <param name="request"></param>
        /// <param name="restaurantId"></param>
        /// <returns></returns>
        [HttpPost("{restaurantId:int}/employees")]
        [ProducesResponseType(204), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.AddEmployeeAsync))]
        public async Task<ActionResult> AddEmployee(List<AddEmployeeRequest> request, int restaurantId)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.AddEmployeeAsync(request, restaurantId, userId.Value);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Move a Restaurant to another RestaurantGroup
        /// </summary>
        /// <param name="restaurantId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{restaurantId:int}/move-to-group")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.MoveRestaurantToGroupAsync))]
        public async Task<ActionResult<RestaurantSummaryVM>> PostRestaurantToGroup(int restaurantId, MoveToGroupRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.MoveRestaurantToGroupAsync(restaurantId, request, user);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Get list of restaurant's employees
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        [HttpGet("{restaurantId:int}/employees")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetEmployeesAsync))]
        public async Task<ActionResult<List<RestaurantEmployeeVM>>> GetEmployees(int restaurantId)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.GetEmployeesAsync(restaurantId, userId.Value);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Updates restaurant info
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="request">Request with data</param>
        [HttpPut("{restaurantId:int}")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.UpdateRestaurantAsync))]
        public async Task<ActionResult<MyRestaurantVM>> EditRestaurantInfo(int restaurantId, UpdateRestaurantRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.UpdateRestaurantAsync(restaurantId, request, user);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Validates input fields for Restaurant Registration process
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("validate-first-step")]
        [ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(401)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.ValidateFirstStepAsync))]
        public async Task<ActionResult> ValidateFirstStep(ValidateRestaurantFirstStepRequest dto)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.ValidateFirstStepAsync(dto, user);
            return OkOrErrors(result);
        }


        /// <summary>
        /// Get list of menus by given restaurant id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{restaurantId:int}/menus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<ActionResult<List<MenuSummaryVM>>> GetMenusById(int restaurantId)
        {
            var userId = User.GetUserId();

            var result = await restaurantService.GetMenusOwnerAsync(restaurantId, userId!.Value);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Gets menu items from the given restaurant
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <returns>The found list of menuItems</returns>
        [HttpGet("{restaurantId:int}/menu-items")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetMenuItemsCustomerAsync))]
        public async Task<ActionResult<List<MenuItemVM>>> GetMenuItems(int restaurantId)
        {
            var userId = User.GetUserId();

            var res = await restaurantService.GetMenuItemsOwnerAsync(userId!.Value, restaurantId);
            return OkOrErrors(res);
        }

        /// <summary>
        /// Archive restaurant
        /// </summary>
        /// <remarks>If the group the restaurant was in is left empty it is also deleted</remarks>
        [HttpDelete("{restaurantId:int}")]
        [ProducesResponseType(204), ProducesResponseType(404)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.ArchiveRestaurantAsync))]
        public async Task<ActionResult> ArchiveRestaurant(int restaurantId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await restaurantService.ArchiveRestaurantAsync(restaurantId, user);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Finds reports created by the user filtered by optional filters
        /// </summary>
        /// <param name="dateFrom">Starting date to look for reports</param>
        /// <param name="dateUntil">Ending date to look for reports</param>
        /// <param name="category">category of the reports to look for</param>
        /// <param name="reportedUserId">id of the user that was reported in the reports</param>
        /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
        /// <param name="service"></param>
        /// <returns></returns>
        [HttpGet("{restaurantId:int}/reports")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<GetReportsService>(nameof(GetReportsService.GetMyRestaurantsReportsAsync))]
        [Authorize(Roles = Roles.RestaurantOwner)]
        public async Task<ActionResult<List<ReportVM>>> GetReports(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateUntil,
            [FromQuery] ReportCategory? category,
            [FromQuery] Guid? reportedUserId,
            int restaurantId,
            [FromServices] GetReportsService service)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }
            return OkOrErrors(await service.GetMyRestaurantsReportsAsync(user, dateFrom, dateUntil, category, reportedUserId, restaurantId));
        }
    }
}
