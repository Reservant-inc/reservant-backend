using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Friend request
/// </summary>
public class FriendRequest : ISoftDeletable
{
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
    /// ID of the sender
    /// </summary>
    public string SenderId { get; set; }

    /// <summary>
    /// ID of the target user
    /// </summary>
    public string ReceiverId { get; set; }

    /// <summary>
    /// Navigational property for the user that sent the request
    /// </summary>
    public User? Sender { get; set; }

    /// <summary>
    /// Navigational property for the target user
    /// </summary>
    public User? Receiver { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
