using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<OrderVM>> GetOrderById(int id)
    {
        var order = await orderService.GetOrderById(id);

        if (order.IsError)
        {
            return order.ToValidationProblem();
        }

        return Ok(order.Value);
    }

}
