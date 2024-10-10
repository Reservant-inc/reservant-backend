using Reservant.Api.Dtos.Orders;

namespace Reservant.Api.Dtos.OrderItems;

/// <summary>
/// Used in <see cref="CreateOrderRequest"/>
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// ID of the ordered MenuItem
    /// </summary>
    public required int MenuItemId { get; init; }

    /// <summary>
    /// Number of items ordered
    /// </summary>
    public required int Amount { get; set; }
}
