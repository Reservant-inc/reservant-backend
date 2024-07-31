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
    /// <summary>
    /// Gets delivery with given id
    /// </summary>
    /// <param name="deliveryId"> Delivery id </param>
    /// <returns></returns>
    [HttpGet]
    [Route("/deliveries/{deliveryId:int}")]
    [Authorize(Roles = Roles.RestaurantBackdoorsEmployee)]
    public async Task<ActionResult<DeliveryVM>> GetDelivery(int deliveryId)
    {

        var result = await deliveryService.GetDeliveryAsync(deliveryId);
        
        if (!result.IsError)
        {
            return Ok(result.Value);
        }
        return result.ToValidationProblem();
    }

    /// <summary>
    /// Creates delivery
    /// </summary>
    /// <param name="deliveryVM">Info about delivery (products, quantity) </param>
    /// <returns>Created DeliveryVM</returns>
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