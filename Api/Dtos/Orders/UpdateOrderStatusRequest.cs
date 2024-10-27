namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Used by restaurant employees to update order's status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Serving employee ID
    /// </summary>
    public required Guid EmployeeId { get; set; }

    /// <summary>
    /// List of Items included in the order
    /// </summary>
    public required List<UpdateOrderItemStatusRequest> Items { get; set; }
}
