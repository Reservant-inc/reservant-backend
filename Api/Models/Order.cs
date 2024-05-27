using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models;

/// <summary>
/// Order
/// </summary>
public class Order : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the visit
    /// </summary>
    public int VisitId { get; set; }

    /// <summary>
    /// Optional note
    /// </summary>
    [StringLength(100)]
    public string? Note { get; set; }

    /// <summary>
    /// Serving employee ID
    /// </summary>
    public string? EmployeeId { get; set; }

    /// <summary>
    /// Total cost of the order
    /// </summary>
    /// <remarks>
    /// OrderItems and then OrderItems.MenuItem must be loaded
    /// </remarks>
    public decimal Cost
    {
        get
        {
            var items = OrderItems ??
                        throw new InvalidOperationException(
                            $"{nameof(OrderItems)} must be loaded to compute {nameof(Cost)}");
            return items
                .Select(oi =>
                {
                    var item = oi.MenuItem ?? throw new InvalidOperationException(
                        $"{nameof(OrderItems)}.{nameof(OrderItem.MenuItem)} must be loaded to compute {nameof(Cost)}");
                    return item.Price * oi.Amount;
                })
                .Sum();
        }
    }

    /// <summary>
    /// Status of the whole order
    /// </summary>
    /// <remarks>
    /// OrderItems must be loaded
    /// </remarks>
    public OrderStatus Status
    {
        get
        {
            var items = OrderItems ??
                        throw new InvalidOperationException(
                            $"{nameof(OrderItems)} must be loaded to compute {nameof(Status)}");
            return items
                .Select(oi => oi.Status)
                .MaxBy(s => (int)s);
        }
    }

    /// <summary>
    /// Navigational collection for the visit
    /// </summary>
    public Visit Visit { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the order items
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = null!;

    /// <summary>
    /// Navigational property for serving employees
    /// </summary>
    public ICollection<User> Employees { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
