using System.ComponentModel.DataAnnotations;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Info about a Visit
/// </summary>
public class VisitVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int VisitId { get; set; }

    /// <summary>
    /// Date of the visit
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Number of people who do not have an account
    /// </summary>
    public required int NumberOfGuests { get; set; }

    /// <summary>
    /// Date of the payment
    /// </summary>
    public required DateTime? PaymentTime { get; set; }

    /// <summary>
    /// Deposit
    /// </summary>
    public required decimal? Deposit { get; set; }

    /// <summary>
    /// Date the reservation was made
    /// </summary>
    public required DateOnly? ReservationDate { get; set; }

    /// <summary>
    /// Optional tip
    /// </summary>
    public required decimal? Tip { get; set; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public required bool Takeaway { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public required Guid ClientId { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; set; }

    /// <summary>
    /// ID of the table within the restaurant
    /// </summary>
    public required int TableId { get; set; }

    /// <summary>
    /// People who visited the restaurant
    /// </summary>
    public required List<UserSummaryVM> Participants { get; set; }

    /// <summary>
    /// Orders made during the visit
    /// </summary>
    public required List<OrderSummaryVM> Orders { get; set; }
}
