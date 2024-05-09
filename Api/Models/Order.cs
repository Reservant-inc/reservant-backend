using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

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
    /// Optional note
    /// </summary>
    [StringLength(100)]
    public string? Note { get; set; }

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
    /// Navigational collection for the order items
    /// </summary>
    public ICollection<OrderItem>? OrderItems { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
