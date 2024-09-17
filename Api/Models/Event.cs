using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Meetup or something
/// </summary>
public class Event : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// When the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    [StringLength(200)]
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
    /// ID of the user who created the event
    /// </summary>
    [StringLength(36)]
    public string CreatorId { get; set; } = null!;

    /// <summary>
    /// ID of the restaurant where the event takes place
    /// </summary>
    public int? RestaurantId { get; set; }

    /// <summary>
    /// Max number of people that can attend at the event
    /// </summary>
    public int MaxPeople { get; set; }

    /// <summary>
    /// ID of the actual visit
    /// </summary>
    public int? VisitId { get; set; }

    /// <summary>
    /// Navigational property for the creator
    /// </summary>
    public User Creator { get; set; } = null!;

    /// <summary>
    /// Navigational property for the restaurant where the event takes place
    /// </summary>
    public Restaurant? Restaurant { get; set; }

    /// <summary>
    /// Navigational property for the users interested
    /// </summary>
    public ICollection<User> Interested { get; set; } = null!;
    
    /// <summary>
    /// Navigational property for the users interested
    /// </summary>
    public ICollection<User> Participants { get; set; } = null!;

    /// <summary>
    /// Navigational property for the actual visit
    /// </summary>
    public Visit? Visit { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
