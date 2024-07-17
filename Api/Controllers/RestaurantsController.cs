using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Models.Dtos.Review;
using Reservant.Api.Services;
using Reservant.Api.Validation;

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
    /// Returns them sorted from the nearest to the farthest
    /// </remarks>
    /// <param name="origLat">Latitude of the point to search from</param>
    /// <param name="origLon">Longitude of the point to search from</param>
    /// <param name="name">Search by name</param>
    /// <param name="tag">Search restaurants that have a certain tag</param>
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
        double origLat, double origLon,
        string? name, string? tag,
        double? lat1, double? lon1, double? lat2, double? lon2,
        int page = 0, int perPage = 10)
    {
        var result = await service.FindRestaurantsAsync(
            origLat, origLon,
            name, tag,
            lat1, lon1, lat2, lon2,
            page, perPage);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
    [ProducesResponseType(200), ProducesResponseType(404),ProducesResponseType(400)]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    public async Task<ActionResult> SetVerifiedId(int restaurantId)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.SetVerifiedIdAsync(user, restaurantId);

        switch (result)
        {
            case VerificationResult.VerifierSetSuccessfully:
                return Ok();
            case VerificationResult.VerifierAlreadyExists:
                return BadRequest();
            case VerificationResult.RestaurantNotFound:
                return NotFound();
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Get orders with pagination and sorting
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <param name="returnFinished">Return finished orders</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <param name="orderBy">Order by criteria</param>
    /// <returns>List of orders with pagination</returns>
    [HttpGet("{restaurantId:int}/orders")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    public async Task<ActionResult<Pagination<OrderSummaryVM>>> GetOrders(int restaurantId, [FromQuery] bool returnFinished = false, [FromQuery] int page = 0, [FromQuery] int perPage = 10, [FromQuery] OrderSorting? orderBy = null)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetOrdersAsync(userId, restaurantId, returnFinished, page, perPage, orderBy);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
    public async Task<ActionResult<ReviewVM>> CreateReview(int restaurantId, CreateReviewRequest createReviewRequest)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.CreateReviewAsync( user,  restaurantId, createReviewRequest);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
    [HttpGet("{restaurantId:int}/reviews")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<Pagination<ReviewVM>>> CreateReviews(int restaurantId, ReviewOrderSorting orderBy = ReviewOrderSorting.DateDesc, int page = 0, int perPage = 10)
    {
        var result = await service.GetReviewsAsync(restaurantId, orderBy, page, perPage);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }
}
