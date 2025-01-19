using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Services.ReportServices;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Dtos.Tables;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services.RestaurantServices;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controller resposnible for registration of a new restaurant, listing owned restaurants and accessing restaurant data
    /// </summary>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [ApiController, Route("/my-restaurants")]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.CreateRestaurantAsync))]
        [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200)]
        [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(404)]
        [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.MoveRestaurantToGroupAsync))]
        public async Task<ActionResult<MyRestaurantSummaryVM>> PostRestaurantToGroup(int restaurantId, MoveToGroupRequest request)
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
        [Authorize(Roles = Roles.RestaurantOwner)]
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
        [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(204), ProducesResponseType(404)]
        [MethodErrorCodes<ArchiveRestaurantService>(nameof(ArchiveRestaurantService.ArchiveRestaurant))]
        public async Task<ActionResult> ArchiveRestaurant(
            int restaurantId, [FromServices] ArchiveRestaurantService service)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await service.ArchiveRestaurant(restaurantId, user);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Finds reports created by the user filtered by optional filters
        /// </summary>
        /// <param name="dateFrom">Starting date to look for reports</param>
        /// <param name="dateUntil">Ending date to look for reports</param>
        /// <param name="category">category of the reports to look for</param>
        /// <param name="reportedUserId">id of the user that was reported in the reports</param>
        /// <param name="createdById">id of the user who created the report</param>
        /// <param name="assignedToId">Search only for reports that are assigned to the agent with the given ID</param>
        /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
        /// <param name="service"></param>
        /// <param name="status">status of the reports considered in the search</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <returns></returns>
        [HttpGet("{restaurantId:int}/reports")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<GetReportsService>(nameof(GetReportsService.GetMyRestaurantsReportsAsync))]
        [Authorize(Roles = Roles.RestaurantOwner)]
        public async Task<ActionResult<Pagination<ReportVM>>> GetReports(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateUntil,
            [FromQuery] ReportCategory? category,
            [FromQuery] Guid? reportedUserId,
            [FromQuery] Guid? createdById,
            [FromQuery] Guid? assignedToId,
            int restaurantId,
            [FromServices] GetReportsService service,
            [FromQuery] ReportStatus status = ReportStatus.All,
            [FromQuery] int page = 0,
            [FromQuery] int perPage = 10)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }
            return OkOrErrors(await service.GetMyRestaurantsReportsAsync(
                user, dateFrom, dateUntil,
                category, reportedUserId, restaurantId,
                createdById, assignedToId, status, page, perPage));
        }

        /// <summary>
        /// Retrives restaurant statistics by restaurant id and given time period
        /// </summary>
        [HttpGet("{restaurantId:int}/statistics")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<StatisticsService>(nameof(StatisticsService.GetStatsByRestaurantIdAsync))]
        [Authorize(Roles = Roles.RestaurantOwner)]
        public async Task<ActionResult<RestaurantStatsVM>> GetStatsByRestaurantId(
            int restaurantId,
            [FromQuery] RestaurantStatsRequest request,
            [FromServices] StatisticsService service)
        {
            var userId = User.GetUserId();
            var result = await service.GetStatsByRestaurantIdAsync(restaurantId, userId!.Value, request);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Retrieve statistics for all restaurants of the current user
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<StatisticsService>(nameof(StatisticsService.GetStatsOfRestaurantOwner))]
        [Authorize(Roles = Roles.RestaurantOwner)]
        public async Task<ActionResult<RestaurantStatsVM>> GetTotalStatistics(
            [FromQuery] RestaurantStatsRequest request,
            [FromServices] StatisticsService service)
        {
            var userId = User.GetUserId();
            var result = await service.GetStatsOfRestaurantOwner(userId!.Value, request);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Update the list of tables of a restaurant
        /// </summary>
        [HttpPut("{restaurantId:int}/tables")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [MethodErrorCodes<UpdateTablesService>(nameof(UpdateTablesService.UpdateTables))]
        [AuthorizeRoles(Roles.RestaurantOwner, Roles.RestaurantEmployee)]
        public async Task<ActionResult<MyRestaurantVM>> UpdateTables(
            int restaurantId,
            UpdateTablesRequest request,
            [FromServices] UpdateTablesService service)
        {
            var userId = User.GetUserId();
            var result = await service.UpdateTables(restaurantId, request, userId!.Value);
            return OkOrErrors(result);
        }
    }
}
