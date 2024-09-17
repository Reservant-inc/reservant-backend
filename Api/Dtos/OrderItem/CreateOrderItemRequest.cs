using Reservant.Api.Dtos.Order;

namespace Reservant.Api.Dtos.OrderItem;

/// <summary>
/// Used in <see cref="CreateOrderRequest"/>
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// ID of the ordered MenuItem
    /// </summary>
    public int MenuItemId { get; init; }

    /// <summary>
    /// Number of items ordered
    /// </summary>
    public int Amount { get; set; }
}
