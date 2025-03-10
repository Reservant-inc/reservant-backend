using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.OrderItems;

/// <summary>
/// Information about an ordered item
/// </summary>
public class OrderItemVM
{
    /// <summary>
    /// Ordered item ID
    /// </summary>
    public required MenuItemSummaryVM MenuItem { get; init; }

    /// <summary>
    /// Number of items ordered
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Price for which the item was ordered
    /// </summary>
    public required decimal OneItemPrice { get; init; }

    /// <summary>
    /// Cost of all the items
    /// </summary>
    public required decimal TotalCost { get; init; }

    /// <summary>
    /// Order status
    /// </summary>
    public required OrderStatus Status { get; init; }
}
