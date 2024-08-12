using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Controller for deliveries
/// </summary>
[ApiController, Route("/deliveries")]
public class DeliveriesController(DeliveryService deliveryService): StrictController
{

    /// <summary>
    /// Gets the delivery with the given id 
    /// </summary>
    /// <param name="deliveryId"></param>
    /// <returns>OrderVM or NotFound if order wasn't found</returns>
    [HttpGet("/deliveries/{deliveryId::int}")]
    [Authorize(Roles = Roles.RestaurantBackdoorsEmployee)]
    public async Task<ActionResult<DeliveryVM>> GetDeliveryById(int deliveryId)
    {
        return OkOrErrors(await deliveryService.GetDeliveryById(deliveryId));
    }
}
