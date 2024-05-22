using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing orders
/// </summary>
[ApiController, Route("/orders")]
public class OrdersController(OrderService orderService, UserManager<User> userManager) : Controller
{

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




}
