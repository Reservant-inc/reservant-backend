namespace Reservant.Api.Models.Dtos.Order;

/// <summary>
/// Basic info about an Order
/// </summary>
public class OrderSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int Id { get; set; }

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
}
