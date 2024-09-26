using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Event;

/// <summary>
/// Meetup or something
/// </summary>
public class EventSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int EventId { get; set; }


    /// <summary>
    /// name of the event
    /// </summary>
    public required string Name { get; set; }


    /// <summary>
    /// Optional description
    /// </summary>
    public required string? Description { get; set; }

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
    /// ID of the user who created the event
    /// </summary>
    public required string CreatorId { get; set; }

    /// <summary>
    /// Navigational property for the creator
    /// </summary>
    public required string CreatorFullName { get; set; }

    /// <summary>
    /// ID of the restaurant where the event takes place
    /// </summary>
    public required int? RestaurantId { get; set; }

    /// <summary>
    /// Restaurant name
    /// </summary>
    public required string? RestaurantName { get; set; }

    /// <summary>
    /// Number of users interested
    /// </summary>
    public required int NumberInterested { get; set; }
}
