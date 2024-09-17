using Reservant.Api.Dtos.OrderItem;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Order;

/// <summary>
/// Information about an Order
/// </summary>
public class OrderVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int OrderId { get; set; }

    /// <summary>
    /// ID of the visit
    /// </summary>
    public required int VisitId { get; set; }

    /// <summary>
    /// Total cost of the order
    /// </summary>
    public required decimal Cost { get; init; }

    /// <summary>
    /// Status of the whole order
    /// </summary>
    public required OrderStatus Status { get; init; }

    /// <summary>
    /// Ordered items
    /// </summary>
    public required List<OrderItemVM> Items { get; init; }

    /// <summary>
    /// Serving employee's ID
    /// </summary>
    public string? EmployeeId { get; set; }
}
