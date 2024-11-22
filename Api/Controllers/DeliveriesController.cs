using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Services.DeliveryServices;

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

    /// <summary>
    /// Confirm that a delivery has been delivered and update ingredient amounts
    /// </summary>
    [HttpPost("{deliveryId:int}/confirm-delivered")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    [MethodErrorCodes<ConfirmDeliveredService>(nameof(ConfirmDeliveredService.ConfirmDelivered))]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<DeliveryVM>> ConfirmDelivered(
        int deliveryId, [FromServices] ConfirmDeliveredService service)
    {
        return OkOrErrors(await service.ConfirmDelivered(
            deliveryId, User.GetUserId()!.Value));
    }

    /// <summary>
    /// Mark a delivery as canceled
    /// </summary>
    [HttpPost("{deliveryId:int}/mark-canceled")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    [MethodErrorCodes<MarkCanceledService>(nameof(MarkCanceledService.MarkCanceled))]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<DeliveryVM>> MarkCanceled(
        int deliveryId, [FromServices] MarkCanceledService service)
    {
        return OkOrErrors(await service.MarkCanceled(
            deliveryId, User.GetUserId()!.Value));
    }
}
