namespace Reservant.Api.Models.Dtos.Visit;

/// <summary>
/// Basic info about a Visit
/// </summary>
public class VisitSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int VisitId { get; set; }

    /// <summary>
    /// Date of the visit
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Number of guests + number of clients
    /// </summary>
    public int NumberOfPeople { get; set; }

    /// <summary>
    /// Zabrano na wynos
    /// </summary>
    public required bool Takeaway { get; set; }

    /// <summary>
    /// ID of the client who made the reservation
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; init; }
}
