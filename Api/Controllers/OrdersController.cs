using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Services;


namespace Reservant.Api.Controllers;

/// <summary>
/// Managing orders
/// </summary>
[ApiController, Route("/orders")]
public class OrdersController(OrderService orderService, UserManager<User> userManager) : Controller
{
    /// <summary>
    /// Controller responsible for canceling orders
    /// </summary>
    [HttpPost("{id:int}/cancel")]
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
}
