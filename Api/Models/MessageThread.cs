using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Message thread
/// </summary>
public class MessageThread : ISoftDeletable
{
    /// <summary>
    /// Maximum length of the title
    /// </summary>
    public const int MaxTitleLength = 100;

    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int MessageThreadId { get; set; }

    /// <summary>
    /// Title of the message thread
    /// </summary>
    [StringLength(MaxTitleLength)]
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

    /// <summary>
    /// Determines if the thread can be edited by participants.
    /// </summary>
    public bool IsEditable { get; set; } = true; // Default to true for user-created threads

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
