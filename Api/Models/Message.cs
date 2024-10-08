using Reservant.Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Message sent in a message thread
/// </summary>
public class Message : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Contents of the message
    /// </summary>
    [StringLength(200)]
    public required string Contents { get; set; }

    /// <summary>
    /// Time when the message was sent
    /// </summary>
    public DateTime DateSent { get; set; }

    /// <summary>
    /// Time when the message was read
    /// </summary>
    public DateTime? DateRead { get; set; }

    /// <summary>
    /// ID of the user who sent the message
    /// </summary>
    public required Guid AuthorId { get; set; }

    /// <summary>
    /// ID of the message thread
    /// </summary>
    public int MessageThreadId { get; set; }

    /// <summary>
    /// Navigation property for the user who sent the message
    /// </summary>
    public User Author { get; set; } = null!;

    /// <summary>
    /// Navigation collection for the message thread
    /// </summary>
    public MessageThread MessageThread { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }
}
