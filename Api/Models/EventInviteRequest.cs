using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Event invite
/// </summary>
public class EventInviteRequest : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Unique Event ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Date and time created
    /// </summary>
    public DateTime DateSent { get; set; }

    /// <summary>
    /// Date and time when the request was read by the receiver
    /// </summary>
    public DateTime? DateRead { get; set; }

    /// <summary>
    /// Date and time when the request was accepted by the receiver
    /// </summary>
    public DateTime? DateAccepted { get; set; }

    /// <summary>
    /// Date the event request was deleted
    /// </summary>
    public DateTime? DateDeleted { get; set; }

    /// <summary>
    /// ID of the sender
    /// </summary>
    [StringLength(36)]
    public string SenderId { get; set; } = null!;

    /// <summary>
    /// ID of the target user
    /// </summary>
    [StringLength(36)]
    public string ReceiverId { get; set; } = null!;

    /// <summary>
    /// Navigational property for the user that sent the invite
    /// </summary>
    public User Sender { get; set; } = null!;
    

    /// <summary>
    /// Navigational property for the event
    /// </summary>
    public Event Event { get; set; } = null!;
    
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}