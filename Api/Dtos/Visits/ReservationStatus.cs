namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Filter visits by the state of the reservation
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Reservations that have a deposit to be paid
    /// </summary>
    DepositNotPaid,

    /// <summary>
    /// Reservations not yet accepted or declined by the restaurant
    /// </summary>
    ToBeReviewed,

    /// <summary>
    /// Reservations approved by the restaurant
    /// </summary>
    Approved,

    /// <summary>
    /// Reservations declined by the restaurant
    /// </summary>
    Declined,
}
