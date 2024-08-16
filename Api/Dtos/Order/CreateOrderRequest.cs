using Reservant.Api.Dtos.OrderItem;

namespace Reservant.Api.Dtos.Order;

/// <summary>
/// Request to create an Order
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// ID of the visit associated with the order
    /// </summary>
    public int VisitId { get; init; }

    /// <summary>
    /// Optional note
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// Ordered MenuItems
    /// </summary>
    public required List<CreateOrderItemRequest> Items { get; init; }
}
