namespace Reservant.Api.Models.Dtos.Order;

/// <summary>
/// Used by restaurant employees to update order's status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Order status
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Serving employee's ID
    /// </summary>
    public ICollection<string> EmployeeId { get; set; }

    public required List<UpdateOrderItemStatusRequest> Items { get; set; }
}
