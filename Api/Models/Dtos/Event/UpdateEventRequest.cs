namespace Reservant.Api.Models.Dtos.Event;

/// <summary>
/// Request to update an Event
/// </summary>
public class UpdateEventRequest
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
    public int RestaurantId { get; set; }
}
