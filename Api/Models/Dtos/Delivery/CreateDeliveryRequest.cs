namespace Reservant.Api.Models.Dtos.Delivery;

public class CreateDeliveryRequest
{
    
    /// <summary>
    /// Positions
    /// </summary>
    public required List<DeliveryPositionVM> Positions { get; init; }
}