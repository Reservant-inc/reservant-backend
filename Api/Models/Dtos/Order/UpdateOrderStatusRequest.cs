namespace Reservant.Api.Models.Dtos.Order;

/// <summary>
/// Used by restaurant employees to update order's status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Serving employee IDs
    /// </summary>
    public ICollection<string> EmployeeIds { get; set; }

    /// <summary>
    /// List of Items included in the order
    /// </summary>
    public required List<UpdateOrderItemStatusRequest> Items { get; set; }
}
