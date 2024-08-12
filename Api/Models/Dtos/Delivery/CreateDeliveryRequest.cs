namespace Reservant.Api.Models.Dtos.Delivery;

public class CreateDeliveryRequest
{
    public required List<DeliveryPositionVM> Positions { get; init; }
}
