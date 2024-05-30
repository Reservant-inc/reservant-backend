using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.User;

namespace Reservant.Api.Models.Dtos.Event;

/// <summary>
/// Meetup or something
/// </summary>
public class EventVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public required int Id { get; set; }

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
    public required int RestaurantId { get; set; }

    /// <summary>
    /// Restaurant name
    /// </summary>
    public required string RestaurantName { get; set; }

    /// <summary>
    /// ID of the actual visit
    /// </summary>
    public required int? VisitId { get; set; }

    /// <summary>
    /// Users interested
    /// </summary>
    public required List<UserSummaryVM> Interested { get; set; }
}
