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
    public int OrderId { get; set; }

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
    [StringLength(36)]
    public string? EmployeeId { get; set; }

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
