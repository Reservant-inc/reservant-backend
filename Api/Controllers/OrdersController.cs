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

    /// <summary>
    /// Creates an order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost()]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<OrderSummaryVM>> CreateOrder(CreateOrderRequest request)
    {
        
        var result = await orderService.CreateOrderAsync(request);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }
    
    
    
    
}
