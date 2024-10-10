namespace Reservant.Api.Dtos.Events;

/// <summary>
/// Request to create an Event
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Name of the event
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the event is going to happen
    /// </summary>
    public required DateTime Time { get; set; }

    /// <summary>
    /// Max number of people that can attend event - only accepted, excluding creator
    /// </summary>
    public required int MaxPeople { get; set; }

    /// <summary>
    /// People must join until this time
    /// </summary>
    public required DateTime MustJoinUntil { get; set; }

    /// <summary>
    /// ID of the restaurant where the event takes place
    /// </summary>
    public int? RestaurantId { get; set; }
}
