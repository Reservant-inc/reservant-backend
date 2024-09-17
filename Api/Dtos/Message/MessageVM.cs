namespace Reservant.Api.Dtos.Message;

/// <summary>
/// Request to create a message
/// </summary>

public class MessageVM
{
    /// <summary>
    /// Unique ID od a message
    /// </summary>
    public required int MessageId { get; set; }
    /// <summary>
    /// Contents of the message
    /// </summary>
    public required string Contents { get; set; }
    /// <summary>
    /// DateSent of the message
    /// </summary>
    public required DateTime DateSent { get; set; }
    /// <summary>
    /// DateRead of the message
    /// </summary>
    public required DateTime? DateRead { get; set; }
    /// <summary>
    /// ID of the message's author
    /// </summary>
    public required string AuthorId { get; set; }
    /// <summary>
    /// MessageThreadId of the message
    /// </summary>
    public required int MessageThreadId { get; set; }
}

