namespace Reservant.Api.Models.Dtos.Delivery;

public class CreateDeliveryRequest
{
    /// <summary>
    /// Unique identifier for the delivery record.
    /// </summary>
    public required int Id { get; init; }
    

    /// <summary>
    /// Positions
    /// </summary>
    public required List<DeliveryPositionVM> Positions { get; init; }
}