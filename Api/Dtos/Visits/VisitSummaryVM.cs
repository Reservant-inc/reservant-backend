using Reservant.Api.Dtos.Restaurants;

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
    /// End time of the visit
    /// </summary>
    public required DateTime EndTime { get; set; }

    /// <summary>
    /// Actual start time of the visit
    /// </summary>
    public required DateTime? ActualStartTime { get; set; }

    /// <summary>
    /// Actual end time of the visit
    /// </summary>
    public required DateTime? ActualEndTime { get; set; }

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
    /// Restaurant where the visit took place
    /// </summary>
    public required RestaurantSummaryVM Restaurant { get; init; }
    
    /// <summary>
    /// Indicates if visit is cancelled
    /// </summary>
    public required bool IsCancelled { get; set; }
}
