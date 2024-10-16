namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Used by restaurant employees to update order's status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Serving employee IDs
    /// </summary>
    public required ICollection<Guid> EmployeeIds { get; set; }

    /// <summary>
    /// List of Items included in the order
    /// </summary>
    public required List<UpdateOrderItemStatusRequest> Items { get; set; }
}
