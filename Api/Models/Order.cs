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
    /// Maximum length of the Note
    /// </summary>
    public const int MaxNoteLength = 100;

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
    [StringLength(MaxNoteLength)]
    public string? Note { get; set; }

    /// <summary>
    /// Navigational collection for the visit
    /// </summary>
    public Visit Visit { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the order items
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = null!;

    /// <summary>
    /// Navigational property for serving employee
    /// </summary>
    public User? AssignedEmployee { get; set; } = null!;

    /// <summary>
    /// Id of a serving employee
    /// </summary>
    public Guid? AssignedEmployeeId { get; set; } = null!;

    /// <summary>
    /// Time the order was paid for
    /// </summary>
    public DateTime? PaymentTime { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
