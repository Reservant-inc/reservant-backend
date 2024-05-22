using Reservant.Api.Models.Dtos.Order;

namespace Reservant.Api.Models.Dtos.OrderItem;

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
