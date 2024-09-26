namespace Reservant.Api.Dtos.Event;

/// <summary>
/// Request to create an Event
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Name of the event
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the event is going to happen
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Max number of people that can attend event - only accepted, excluding creator
    /// </summary>
    public int MaxPeople { get; set; }

    /// <summary>
    /// People must join until this time
    /// </summary>
    public DateTime MustJoinUntil { get; set; }

    /// <summary>
    /// ID of the restaurant where the event takes place
    /// </summary>
    public int? RestaurantId { get; set; }
}
