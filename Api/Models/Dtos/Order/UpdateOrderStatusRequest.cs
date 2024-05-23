using Reservant.Api.Models.Enums;

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
    public string? EmployeeId { get; set; }
}
