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
    public int Id { get; set; }

    /// <summary>
    /// Date of the visit
    /// </summary>
    public DateOnly Date { get; set; }

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
    [Range(0, 500), Column(TypeName = "decimal(5, 2)")]
    public decimal? Deposit { get; set; }

    /// <summary>
    /// Date the reservation was made
    /// </summary>
    public DateOnly? ReservationDate { get; set; }

    /// <summary>
    /// Optional tip
    /// </summary>
    [Range(0, 500), Column(TypeName = "decimal(5, 2)")]
    public decimal? Tip { get; set; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public bool Takeaway { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int TableRestaurantId { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId => TableRestaurantId;

    /// <summary>
    /// ID of the table within the restaurant
    /// </summary>
    public int TableId { get; set; }

    /// <summary>
    /// Navigational property for the client who made the reservation
    /// </summary>
    public User? Client { get; set; }

    /// <summary>
    /// People who visited the restaurant
    /// </summary>
    public ICollection<User>? Participants { get; set; }

    /// <summary>
    /// Orders made during the visit
    /// </summary>
    public ICollection<Order>? Orders { get; set; }

    /// <summary>
    /// Navigational collection for the table
    /// </summary>
    public Table? Table { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
