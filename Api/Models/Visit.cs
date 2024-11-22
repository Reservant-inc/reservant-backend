using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Data;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models;

/// <summary>
/// The restaurant's decision whether to accept the reservation or not
/// </summary>
public class RestaurantDecision
{
    /// <summary>
    /// ID of the restaurant owner/hall employee who considered the reservation
    /// </summary>
    public Guid AnsweredById { get; set;}

    /// <summary>
    /// Whether the restaurant accepts or declines the reservation
    /// </summary>
    public bool IsAccepted  { get; set;}

    /// <summary>
    /// Navigation property for the restaurant owner/hall employee who considered the reservation
    /// </summary>
    public User AnsweredBy { get; set; } = null!;
}

/// <summary>
/// Reservation at a restaurant
/// </summary>
public class Reservation
{
    /// <summary>
    /// Date the reservation was made
    /// </summary>
    public DateTime? ReservationDate { get; set; }

    /// <summary>
    /// Start time of the reservation
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the reservation
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Deposit to be paid
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Deposit { get; set; }

    /// <summary>
    /// Date of the payment
    /// </summary>
    public DateTime? DepositPaymentTime { get; set; }

    /// <summary>
    /// The restaurant's decision whether to accept reservation or not
    /// </summary>
    public RestaurantDecision? Decision { get; set; }

    /// <summary>
    /// Current state of the visit
    /// </summary>
    public ReservationStatus CurrentStatus
    {
        get
        {
            return this switch
            {
                { Deposit: not null, DepositPaymentTime: null } => ReservationStatus.DepositNotPaid,
                { Decision: null } => ReservationStatus.ToBeReviewedByRestaurant,
                { Decision.IsAccepted: true } => ReservationStatus.ApprovedByRestaurant,
                { Decision.IsAccepted: false } => ReservationStatus.DeclinedByRestaurant,
                null => throw new InvalidOperationException(),
            };
        }
    }
}

/// <summary>
/// Restaurant visit
/// </summary>
public class Visit : ISoftDeletable
{
    /// <summary>
    /// Minimal reservation duration in minutes
    /// </summary>
    public const int MinReservationDurationMinutes = 30;

    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int VisitId { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Number of people who do not have an account
    /// </summary>
    public int NumberOfGuests { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Reservation
    /// </summary>
    public Reservation? Reservation { get; set; }

    /// <summary>
    /// Whether the client ordered a takeaway
    /// </summary>
    public bool Takeaway { get; set; }

    /// <summary>
    /// ID of the table within the restaurant
    /// </summary>
    public int? TableId { get; set; }

    /// <summary>
    /// Actual start time of the visit
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Actual end time of the visit
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Optional tip
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Tip { get; set; }

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
    public Table? Table { get; set; } = null!;

    /// <summary>
    /// Navigational property for restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Check whether the visit has already started
    /// </summary>
    public bool HasStarted() => StartTime is not null;

    /// <summary>
    /// Check whether the visit has already ended
    /// </summary>
    /// <returns></returns>
    public bool HasEnded() => EndTime is not null;
}
