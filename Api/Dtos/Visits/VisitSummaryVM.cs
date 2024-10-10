namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Basic info about a Visit
/// </summary>
public class VisitSummaryVM
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
    /// Number of guests + number of clients
    /// </summary>
    public required int NumberOfPeople { get; set; }

    /// <summary>
    /// Deposit
    /// </summary>
    public required decimal? Deposit { get; init; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public required bool Takeaway { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; init; }
}
