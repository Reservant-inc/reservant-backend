namespace Reservant.Api.Models.Dtos.Delivery;

public class DeliveryVM
{
    public required int Id { get; init; }
    public required List<DeliveryPositionVM> Positions { get; init; }
}
