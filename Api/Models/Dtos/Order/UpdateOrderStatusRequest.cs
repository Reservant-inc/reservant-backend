namespace Reservant.Api.Models.Dtos.Order;

/// <summary>
/// Used by restaurant employees to update order's status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Order status
    /// </summary>
    public OrderStatus Status
    {
        get
        {
            if (Items.Any(i => i.Status == OrderStatus.Cancelled))
            {
                return OrderStatus.Cancelled;
            }
            if (Items.Any(i => i.Status == OrderStatus.InProgress))
            {
                return OrderStatus.InProgress;
            }
            if (Items.Any(i => i.Status == OrderStatus.Taken))
            {
                return OrderStatus.Taken;
            }
            return OrderStatus.Ready;
        }
    }

    /// <summary>
    /// Serving employee IDs
    /// </summary>
    public ICollection<string> EmployeeIds { get; set; }

    /// <summary>
    /// List of Items included in the order
    /// </summary>
    public required List<UpdateOrderItemStatusRequest> Items { get; set; }
}
