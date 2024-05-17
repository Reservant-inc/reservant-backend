namespace Reservant.Api.Models.Dtos.OrderItem;

/// <summary>
/// Information about an ordered item
/// </summary>
public class OrderItemVM
{
    /// <summary>
    /// Ordered item ID
    /// </summary>
    public required int MenuItemId { get; init; }

    /// <summary>
    /// Number of items ordered
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Cost of all the items
    /// </summary>
    public required decimal Cost { get; init; }

    /// <summary>
    /// Order status
    /// </summary>
    public required OrderStatus Status { get; init; }
}
