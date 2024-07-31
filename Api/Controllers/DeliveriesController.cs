using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Delivery controller.
/// </summary>
[ApiController, Route("/deliveries")]
public class DeliveriesController(
    UserManager<User> userManager,
    DeliveryService deliveryService
    ) : StrictController
{


    [HttpPost]
    public async Task<ActionResult<DeliveryVM>> createDelivery(DeliveryVM deliveryVM)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await deliveryService.createDeliveryAsync(deliveryVM, user);

        if (!result.IsError)
        {
            return Ok(result.Value);
        }
        return result.ToValidationProblem();
    }
    
    
    
    
    
    
    
    
}