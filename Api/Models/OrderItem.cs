using Reservant.Api.Data;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models;

/// <summary>
/// Pozycja w zamówieniu
/// </summary>
public class OrderItem : ISoftDeletable
{
    /// <summary>
    /// Number of items ordered
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// ID of the order
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// ID of the menu item ordered
    /// </summary>
    public int MenuItemId { get; set; }

    /// <summary>
    /// Status of the item
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Navigational property for the order
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Navigational property for the menu item ordered
    /// </summary>
    public MenuItem? MenuItem { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
