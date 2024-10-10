using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Basic info about an Order
/// </summary>
public class OrderSummaryVM
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
    /// Date of the visit
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Optional note
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Total cost of the order
    /// </summary>
    public required decimal Cost { get; init; }

    /// <summary>
    /// Status of the whole order
    /// </summary>
    public required OrderStatus Status { get; init; }
}
