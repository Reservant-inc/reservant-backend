using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Events;

/// <summary>
/// Meetup or something
/// </summary>
public class EventVM
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
    /// When the event was created
    /// </summary>
    public required DateTime CreatedAt { get; set; }

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
    /// File name of the photo
    /// </summary>
    public required string Photo { get; set; }

    /// <summary>
    /// The user who created the event
    /// </summary>
    public required UserSummaryVM Creator { get; set; }

    /// <summary>
    /// Restaurant where the event takes place
    /// </summary>
    public required RestaurantSummaryVM Restaurant { get; set; }

    /// <summary>
    /// ID of the actual visit
    /// </summary>
    public required int? VisitId { get; set; }

    /// <summary>
    /// Users interested
    /// </summary>
    public required List<UserSummaryVM> Participants { get; set; }

    /// <summary>
    /// Number of users interested
    /// </summary>
    public required int NumberInterested { get; set; }

    /// <summary>
    /// ID of the related message thread
    /// </summary>
    public required int? ThreadId { get; set; }
}
