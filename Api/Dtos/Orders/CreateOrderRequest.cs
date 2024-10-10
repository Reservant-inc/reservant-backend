using Reservant.Api.Dtos.OrderItems;

namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Request to create an Order
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// ID of the visit associated with the order
    /// </summary>
    public required int VisitId { get; init; }

    /// <summary>
    /// Optional note
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// Ordered MenuItems
    /// </summary>
    public required List<CreateOrderItemRequest> Items { get; init; }
}
