using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
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

    [HttpGet]
    [Route("/deliveries/{id:int}")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<DeliveryVM>> GetDelivery(int id)
    {

        var result = await deliveryService.GetDeliveryAsync(id);
        
        if (!result.IsError)
        {
            return Ok(result.Value);
        }
        return result.ToValidationProblem();
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryVM>> CreateDelivery(CreateDeliveryRequest deliveryVM)
    {
        var user = await userManager.GetUserAsync(User);
    
        var result = await deliveryService.CreateDeliveryAsync(deliveryVM, user);
    
        if (!result.IsError)
        {
            return Ok(result.Value);
        }
        return result.ToValidationProblem();
    }
    
    
    
    
    
    
    
    
}