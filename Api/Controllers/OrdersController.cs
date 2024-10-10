using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
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
    /// <param name="orderId">Id of the order</param>
    /// <returns>OrderVM or NotFound if order wasn't found</returns>
    [HttpGet("{orderId:int}")]
    [Authorize(Roles = $"{Roles.Customer},{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    public async Task<ActionResult<OrderVM>> GetOrderById(int orderId)
    {
        var order = await orderService.GetOrderById(orderId, User);
        return OkOrErrors(order);
    }

    /// <summary>
    /// Controller responsible for canceling orders
    /// </summary>
    [HttpPost("{orderId:int}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    public async Task<ActionResult> CancelOrder(int orderId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await orderService.CancelOrderAsync(orderId, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Creates an order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost()]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<OrderService>(nameof(OrderService.CreateOrderAsync))]
    public async Task<ActionResult<OrderSummaryVM>> CreateOrder(CreateOrderRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await orderService.CreateOrderAsync(request, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Update information about an Order as a restaurant employee
    /// </summary>
    /// <param name="orderId">order id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{orderId:int}/status")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<OrderVM>> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, user);
        return OkOrErrors(result);
    }
}
