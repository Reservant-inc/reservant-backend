using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing orders
/// </summary>
[ApiController, Route("/orders")]
public class OrdersController(OrderService orderService) : Controller
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

}
