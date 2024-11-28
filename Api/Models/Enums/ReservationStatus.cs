namespace Reservant.Api.Models.Enums;

/// <summary>
/// Current status of a reservation
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// There is a deposit to be paid by the customer
    /// </summary>
    DepositNotPaid,

    /// <summary>
    /// The reservation is waiting to be accepted or declined by the restaurant
    /// </summary>
    ToBeReviewedByRestaurant,

    /// <summary>
    /// The reservation has been accepted by the restaurant
    /// </summary>
    ApprovedByRestaurant,

    /// <summary>
    /// The reservation has been declined by the restaurant
    /// </summary>
    DeclinedByRestaurant,
}
