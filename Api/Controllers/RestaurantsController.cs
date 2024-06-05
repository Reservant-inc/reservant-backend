using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.Order;
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
    /// <param name="id">ID of the restaurant</param>
    /// <param name="returnFinished">Return finished orders</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <param name="orderBy">Order by criteria</param>
    /// <returns>List of orders with pagination</returns>
    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<Pagination<OrderSummaryVM>>> GetOrders(int id, [FromQuery] bool returnFinished = false, [FromQuery] int page = 0, [FromQuery] int perPage = 10, [FromQuery] OrderSorting? orderBy = null)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetOrdersAsync(userId, id, returnFinished, page, perPage, orderBy);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get future events in a restaurant with pagination.
    /// </summary>
    /// <param name="id">ID of the restaurant.</param>
    /// <param name="page">Page number to return.</param>
    /// <param name="perPage">Items per page.</param>
    /// <returns>Paginated list of future events.</returns>
    [HttpGet("{id:int}/events")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<EventSummaryVM>>> GetFutureEventsByRestaurant(int id, [FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.GetFutureEventsByRestaurantAsync(id, page, perPage);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

}
