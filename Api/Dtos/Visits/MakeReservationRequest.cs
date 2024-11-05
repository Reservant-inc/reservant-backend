namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Request to create a Visit
/// </summary>
public class MakeReservationRequest
{
    /// <summary>
    /// Start time of the reservation
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// End time of the reservation
    /// </summary>
    public required DateTime EndTime { get; set; }

    /// <summary>
    /// Number of people who do not have an account
    /// </summary>
    public required int NumberOfGuests { get; init; }

    /// <summary>
    /// Optional tip
    /// </summary>
    public required decimal? Tip { get; init; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public required bool Takeaway { get; init; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; init; }

    /// <summary>
    /// People who visited the restaurant
    /// </summary>
    public required List<Guid> ParticipantIds { get; init; }

    /// <summary>
    /// Total number of people making the reservation, including the visit's creator
    /// </summary>
    public int TotalNumberOfPeople => NumberOfGuests + ParticipantIds.Count + 1;

    /// <summary>
    /// value of the deposit for the reservations that require it
    /// </summary>
    public decimal? Deposit { get; init; }
}
