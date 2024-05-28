using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Services;
using Reservant.Api.Models;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing orders
/// </summary>
[ApiController, Route("/orders")]
public class OrdersController(OrderService orderService, UserManager<User> userManager) : StrictController
{
    /// <summary>
    /// Gets order with the given id
    /// </summary>
    /// <param name="id">Id of the order</param>
    /// <returns>OrderVM or NotFound if order wasn't found</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Customer},{Roles.RestaurantEmployee}")]
    public async Task<ActionResult<OrderVM>> GetOrderById(int id)
    {
        var order = await orderService.GetOrderById(id, User);

        if (order.IsError)
        {
            return order.ToValidationProblem();
        }

        return Ok(order.Value);
    }

    /// <summary>
    /// Controller responsible for canceling orders
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> CancelOrder(int id)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await orderService.CancelOrderAsync(id, user);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Creates an order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost()]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<OrderSummaryVM>> CreateOrder(CreateOrderRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await orderService.CreateOrderAsync(request, user);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id">order id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<OrderVM>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await orderService.UpdateOrderStatusAsync(id, request, user);
        if (result.IsError) {
            return result.ToValidationProblem();
        }
        return Ok(result.Value);
    }
}
