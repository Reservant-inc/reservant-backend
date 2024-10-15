using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Restaurant visit
/// </summary>
public class Visit : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int VisitId { get; set; }

    /// <summary>
    /// Start time of the reservation
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// End time of the reservation
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Number of people who do not have an account
    /// </summary>
    public int NumberOfGuests { get; set; }

    /// <summary>
    /// Date of the payment
    /// </summary>
    public DateTime? PaymentTime { get; set; }

    /// <summary>
    /// Deposit
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Deposit { get; set; }

    /// <summary>
    /// Date the reservation was made
    /// </summary>
    public DateOnly? ReservationDate { get; set; }

    /// <summary>
    /// Optional tip
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Tip { get; set; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public bool Takeaway { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// ID of the table within the restaurant
    /// </summary>
    public int TableId { get; set; }

    /// <summary>
    /// Navigational property for the client who made the reservation
    /// </summary>
    public User Client { get; set; } = null!;

    /// <summary>
    /// People who visited the restaurant
    /// </summary>
    public ICollection<User> Participants { get; set; } = null!;

    /// <summary>
    /// Orders made during the visit
    /// </summary>
    public ICollection<Order> Orders { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the table
    /// </summary>
    public Table Table { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigational property for restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;
}
