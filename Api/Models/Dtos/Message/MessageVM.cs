namespace Reservant.Api.Models.Dtos.Message;

/// <summary>
/// Request to create a message
/// </summary>

public class MessageVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Contents of the message
    /// </summary>
    public string Contents { get; set; }
    /// <summary>
    /// DateSent of the message
    /// </summary>
    public DateTime DateSent { get; set; }
    /// <summary>
    /// DateRead of the message
    /// </summary>
    public DateTime DateRead { get; set; }
    /// <summary>
    /// AuthorId of the message
    /// </summary>
    public string AuthorId { get; set; }
    /// <summary>
    /// MessageThreadId of the message
    /// </summary>
    public int MessageThreadId { get; set; }
}

