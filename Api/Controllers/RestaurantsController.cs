using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Dtos.Events;
using Reservant.Api.Dtos.Ingredients;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Reviews;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Dtos.Tables;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Controllers;


/// <summary>
/// Restaurants from the perspective of other people than the owner
/// </summary>
[ApiController, Route("/restaurants")]
public class RestaurantController(UserManager<User> userManager, RestaurantService service) : StrictController
{

    /// <summary>
    /// Find restaurants by different criteria
    /// </summary>
    /// <remarks>
    /// Returns them sorted from the nearest to the farthest if origLat and origLon are provided;
    /// Else sorts them alphabetically by name
    /// </remarks>
    /// <param name="origLat">Latitude of the point to search from; if provided the restaurants will be sorted by distance</param>
    /// <param name="origLon">Longitude of the point to search from; if provided the restaurants will be sorted by distance</param>
    /// <param name="name">Search by name</param>
    /// <param name="tags">Search restaurants that have certain tags (specify up to 4 times to search by multiple tags)</param>
    /// <param name="minRating">Search restaurants with at least this many stars</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <param name="lat1">Search within a rectengular area: first point's latitude</param>
    /// <param name="lon1">Search within a rectengular area: first point's longitude</param>
    /// <param name="lat2">Search within a rectengular area: second point's latitude</param>
    /// <param name="lon2">Search within a rectengular area: second point's longitude</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<NearRestaurantVM>>> FindRestaurants(
        double? origLat, double? origLon,
        string? name, [FromQuery] HashSet<string> tags,
        int? minRating,
        double? lat1, double? lon1, double? lat2, double? lon2,
        int page = 0, int perPage = 10)
    {
        var result = await service.FindRestaurantsAsync(
            origLat, origLon,
            name, tags, minRating,
            lat1, lon1, lat2, lon2,
            page, perPage);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Verify restaurant
    /// </summary>
    /// <remarks>
    /// For CustomerSupportAgent. Sets the restaurant's verifier ID to the current user's ID.
    /// </remarks>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <response code="400">Restaurant already verified</response>
    [HttpPost("{restaurantId:int}/verify")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.SetVerifiedIdAsync))]
    public async Task<ActionResult> SetVerifiedId(int restaurantId)
    {
        var result = await service.SetVerifiedIdAsync(User.GetUserId()!.Value, restaurantId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get orders with pagination and sorting
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="returnFinished">Return finished orders</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <param name="orderBy">Order by criteria</param>
    /// <param name="tableId">Optional table number filter by Id</param>
    /// <param name="assignedEmployeeId">Optional emplyee number filter by Id</param>
    /// <returns>List of orders with pagination</returns>
    [HttpGet("{restaurantId:int}/orders")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    public async Task<ActionResult<Pagination<OrderSummaryVM>>> GetOrders(int restaurantId, [FromQuery] bool returnFinished = false, [FromQuery] int page = 0, [FromQuery] int perPage = 10, [FromQuery] OrderSorting? orderBy = null, int? tableId = null, Guid? assignedEmployeeId = null )
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetOrdersAsync(userId.Value, restaurantId, returnFinished, page, perPage, orderBy,tableId,assignedEmployeeId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get future events in a restaurant with pagination.
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant.</param>
    /// <param name="page">Page number to return.</param>
    /// <param name="perPage">Items per page.</param>
    /// <returns>Paginated list of future events.</returns>
    [HttpGet("{restaurantId:int}/events")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<EventSummaryVM>>> GetFutureEventsByRestaurant(int restaurantId, [FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetFutureEventsByRestaurantAsync(restaurantId, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Add restaurant review
    /// </summary>
    /// <remarks>
    /// Adds review from logged in user
    /// </remarks>
    [HttpPost("{restaurantId:int}/reviews")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.CreateReviewAsync))]
    public async Task<ActionResult<ReviewVM>> CreateReview(int restaurantId, CreateReviewRequest createReviewRequest)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.CreateReviewAsync(user, restaurantId, createReviewRequest);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Returns reviews by id
    /// </summary>
    /// <remarks>
    /// Returns reviews from restaurant with given restaurant Id
    /// </remarks>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="orderBy">Order of the reviews</param>
    /// <param name="page">Page number of the reviews</param>
    /// <param name="perPage">Number of reviews per page</param>
    /// <param name="name"></param>
    /// <param name="surname"></param>
    [HttpGet("{restaurantId:int}/reviews")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetReviewsAsync))]
    public async Task<ActionResult<Pagination<ReviewVM>>> CreateReviews(int restaurantId, ReviewOrderSorting orderBy = ReviewOrderSorting.DateDesc, int page = 0, int perPage = 10, [FromQuery] string? name = null, [FromQuery] string? surname = null)
    {
        var result = await service.GetReviewsAsync(restaurantId, orderBy, page, perPage, name, surname);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get detailed information about a restaurant
    /// </summary>
    /// <remarks>
    /// Only shows verified restaurants
    /// </remarks>
    /// <param name="restaurantId"></param>
    /// <returns></returns>
    [HttpGet("{restaurantId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetRestaurantByIdAsync))]
    public async Task<ActionResult<RestaurantVM>> GetRestaurantDetails(int restaurantId)
    {
        var result = await service.GetRestaurantByIdAsync(restaurantId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get visits in a restaurant
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant.</param>
    /// <param name="dateStart">Filter out visits before the date</param>
    /// <param name="dateEnd">Filter out visits after the date</param>
    /// <param name="tableId">Only visits assigned to the specified table ID</param>
    /// <param name="hasOrders">
    /// If true, only visits with orders; if false, only visits without orders; if null, all visits
    /// </param>
    /// <param name="isTakeaway">
    /// If true, only takeaway visits; if false, only dine-in visits; if null, all visits
    /// </param>
    /// <param name="reservationStatus">Filter visits by the state of the reservation</param>
    /// <param name="visitSorting">Order visits</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paged list of visits</returns>
    [HttpGet("{restaurantId:int}/visits")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetVisitsInRestaurantAsync))]
    [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.RestaurantEmployee}")]
    public async Task<ActionResult<Pagination<VisitVM>>> GetVisitsInRestaurant(
        int restaurantId,
        DateOnly? dateStart,
        DateOnly? dateEnd,
        int? tableId,
        bool? hasOrders,
        bool? isTakeaway,
        ReservationStatus? reservationStatus,
        VisitSorting visitSorting,
        [FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var result = await service.GetVisitsInRestaurantAsync(
            User.GetUserId()!.Value,
            restaurantId,
            dateStart,
            dateEnd,
            tableId,
            hasOrders,
            isTakeaway,
            reservationStatus,
            visitSorting,
            page,
            perPage);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Get list of ingredients for a restaurant
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="orderBy">Sorting order</param>
    /// <returns>Paginated list of ingredients</returns>
    [HttpGet("{restaurantId:int}/ingredients")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<IngredientVM>>> GetIngredients(
        int restaurantId,
        [FromQuery] IngredientSorting orderBy = IngredientSorting.NameAsc)
    {
        var result = await service.GetIngredientsAsync(restaurantId, orderBy);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get deliveries in a restaurant
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="returnDelivered">If true, return finished deliveries, unfinished otherwise</param>
    /// <param name="userId">Search by user ID</param>
    /// <param name="userName">Search by user name</param>
    /// <param name="orderBy">Order results by</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    [HttpGet("{restaurantId:int}/deliveries")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetDeliveriesInRestaurantAsync))]
    public async Task<ActionResult<Pagination<DeliverySummaryVM>>> GetDeliveries(
        int restaurantId,
        bool returnDelivered,
        Guid? userId,
        string? userName,
        DeliverySorting orderBy,
        int page = 0,
        int perPage = 10)
    {
        var result = await service.GetDeliveriesInRestaurantAsync(
            restaurantId, returnDelivered, userId, userName, orderBy,
            User.GetUserId()!.Value, page, perPage);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get list of menus by given restaurant id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{restaurantId:int}/menus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [Authorize(Roles = $"{Roles.Customer}, {Roles.RestaurantEmployee}")]
    public async Task<ActionResult<List<MenuSummaryVM>>> GetMenusById(int restaurantId)
    {
        var result = await service.GetMenusCustomerAsync(restaurantId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Gets menu items from the given restaurant
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <returns>The found list of menuItems</returns>
    [HttpGet("{restaurantId:int}/menu-items")]
    [Authorize(Roles = $"{Roles.Customer}, {Roles.RestaurantEmployee}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<List<MenuItemVM>>> GetMenuItems(int restaurantId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.GetMenuItemsCustomerAsync(user, restaurantId);
        return OkOrErrors(res);
    }

    /// <summary>
    /// Get time spans on a given date that a reservation can be made in
    /// </summary>
    /// <param name="restaurantId">Restaurant ID</param>
    /// <param name="date">Date of the reservation</param>
    /// <param name="numberOfGuests">Number of people that will be going</param>
    /// <returns>Available hours list</returns>
    [HttpGet("{restaurantId:int}/available-hours")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetAvailableHoursAsync))]
    public async Task<ActionResult<List<AvailableHoursVM>>> GetAvailableHours(
        int restaurantId, [FromQuery] DateOnly date, [FromQuery] int numberOfGuests)
    {
        var availableHours = await service.GetAvailableHoursAsync(restaurantId, date, numberOfGuests);
        return OkOrErrors(availableHours);
    }

    /// <summary>
    /// Get a list of employees with limited data for a specific restaurant.
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="hallOnly">Show only hall employees</param>
    /// <param name="backdoorOnly">Show only backdoor employees</param>
    /// <returns>List of employees with limited data</returns>
    [HttpGet("{restaurantId:int}/employees")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantService>(nameof(RestaurantService.GetEmployeesBasicInfoAsync))]
    [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.RestaurantEmployee}")]
    public async Task<ActionResult<List<EmployeeBasicInfoVM>>> GetEmployees(
        int restaurantId,
        bool hallOnly,
        bool backdoorOnly)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetEmployeesBasicInfoAsync(
            restaurantId, userId.Value,
            hallOnly, backdoorOnly);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Gets a list of tables in a restaurant
    /// </summary>
    /// <param name="restaurantId"></param>
    /// <returns></returns>
    [HttpGet("{restaurantId:int}/tables")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    public async Task<ActionResult<List<RestaurantTableVM>>> GetTables(int restaurantId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var tables = await service.GetTablesAsync(restaurantId, user);
        return OkOrErrors(tables);
    }
}
