using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Events;
/// <summary>
/// DTO responsible for taking parameters to search the events by.
/// </summary>
public class GetEventsRequest
{
    /// <summary>
    /// The original latitude of the user
    /// </summary>
    public double? OrigLat {  get; set; }
    /// <summary>
    /// The original latitude of the user
    /// </summary>
    public double? OrigLon { get; set; }
    /// <summary>
    /// ID of the restaurant that is hosting the event
    /// </summary>
    public int? RestaurantId { get; set; }
    /// <summary>
    /// Name of the event
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Date to start the search from
    /// </summary>
    public DateTime? DateFrom { get; set; }
    /// <summary>
    /// Date to end the search
    /// </summary>
    public DateTime? DateUntil { get; set; }
    /// <summary>
    /// Value to specify the events status
    /// </summary>
    public EventStatus? EventStatus { get; set; }
}
