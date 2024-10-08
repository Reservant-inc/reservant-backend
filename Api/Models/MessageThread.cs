using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Message thread
/// </summary>
public class MessageThread : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int MessageThreadId { get; set; }

    /// <summary>
    /// Title of the message thread
    /// </summary>
    [StringLength(40)]
    public required string Title { get; set; }

    /// <summary>
    /// When the thread was created
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// ID of the user who created the thread
    /// </summary>
    public Guid CreatorId { get; set; }

    /// <summary>
    /// Navigational property for the user who created the thread
    /// </summary>
    public User Creator { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the participants
    /// </summary>
    public ICollection<User> Participants { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the messages
    /// </summary>
    public ICollection<Message> Messages { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
