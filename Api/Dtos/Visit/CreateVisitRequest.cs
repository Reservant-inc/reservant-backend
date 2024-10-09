namespace Reservant.Api.Dtos.Visit;

/// <summary>
/// Request to create a Visit
/// </summary>
public class CreateVisitRequest
{
    /// <summary>
    /// Date of the visit
    /// </summary>
    public required DateTime Date { get; init; }

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
    /// ID of the table within the restaurant
    /// </summary>
    public required int TableId { get; init; }

    /// <summary>
    /// People who visited the restaurant
    /// </summary>
    public required List<Guid> ParticipantIds { get; init; }
}
