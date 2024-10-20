using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Events;

/// <summary>
/// DTO of an event find using search
/// </summary>
public class NearEventVM
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
    /// User who created the event
    /// </summary>
    public required UserSummaryVM Creator { get; set; }

    /// <summary>
    /// Restaurant where the event takes place
    /// </summary>
    public required RestaurantSummaryVM? Restaurant { get; set; }

    /// <summary>
    /// Distance from the origin point
    /// </summary>
    public required double? Distance { get; set; }

    /// <summary>
    /// Number of users interested
    /// </summary>
    public required int NumberInterested { get; set; }

    /// <summary>
    /// Number of users participate (accepted requests)
    /// </summary>
    public required int NumberParticipants { get; set; }
}
