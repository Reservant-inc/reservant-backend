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
