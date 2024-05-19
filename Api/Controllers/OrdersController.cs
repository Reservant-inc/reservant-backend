using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Reservant.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Models.Dtos.Auth;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Managing orders
    /// </summary>
    [ApiController, Route("/orders")]
    public class OrdersController(UserManager<User> userManager, OrderService service) : Controller
    {
        /// <summary>
        /// Get orders with pagination and sorting
        /// </summary>
        /// <param name="restaurantId">ID of the restaurant</param>
        /// <param name="returnFinished">Return finished orders</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Records per page</param>
        /// <param name="orderBy">Order by criteria</param>
        /// <returns>List of orders with pagination</returns>
        [HttpGet]
        [ProducesResponseType(200), ProducesResponseType(404), ProducesResponseType(400)]
        [Authorize(Roles = Roles.RestaurantEmployee)]
        public async Task<ActionResult<Pagination<OrderSummaryVM>>> GetOrders([FromQuery] int restaurantId, [FromQuery] bool returnFinished = false, [FromQuery] int page = 0, [FromQuery] int perPage = 10, [FromQuery] OrderSorting? orderBy = null)
        {
            var userId = userManager.GetUserId(User);
            var result = await service.GetOrdersAsync(userId, restaurantId, returnFinished, page, perPage, orderBy);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }

            return Ok(result.Value);
        }
    }
}
