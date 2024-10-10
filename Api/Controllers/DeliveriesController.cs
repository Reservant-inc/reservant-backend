using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Identity;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Controller for deliveries
/// </summary>
[ApiController, Route("/deliveries")]
public class DeliveriesController(DeliveryService deliveryService) : StrictController
{

    /// <summary>
    /// Gets the delivery with the given id
    /// </summary>
    /// <param name="deliveryId"></param>
    /// <returns>OrderVM or NotFound if order wasn't found</returns>
    [HttpGet("{deliveryId:int}")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    [MethodErrorCodes<DeliveryService>(nameof(DeliveryService.GetDeliveryById))]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<DeliveryVM>> GetDeliveryById(int deliveryId)
    {
        return OkOrErrors(await deliveryService.GetDeliveryById(
            deliveryId, User.GetUserId()!.Value));
    }

    /// <summary>
    /// Creates a new delivery
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    [MethodErrorCodes<DeliveryService>(nameof(DeliveryService.PostDelivery))]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<DeliveryVM>> PostDelivery(CreateDeliveryRequest request)
    {
        return OkOrErrors(await deliveryService.PostDelivery(
            request, User.GetUserId()!.Value));
    }
}
