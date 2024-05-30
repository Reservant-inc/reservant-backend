namespace Reservant.Api.Models.Dtos.Event;

/// <summary>
/// Request to create an Event
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the event is going to happen
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// People must join until this time
    /// </summary>
    public DateTime MustJoinUntil { get; set; }

    /// <summary>
    /// ID of the restaurant where the event takes place
    /// </summary>
    public int RestaurantId { get; set; }
}
