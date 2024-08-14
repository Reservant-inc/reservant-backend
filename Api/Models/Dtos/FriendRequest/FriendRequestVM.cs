namespace Reservant.Api.Models.Dtos.FriendRequest;

/// <summary>
/// Information about a friend request
/// </summary>
public class FriendRequestVM
{
    /// <summary>
    /// Date and time created
    /// </summary>
    public required DateTime DateSent { get; init; }

    /// <summary>
    /// Date and time when the request was read by the receiver
    /// </summary>
    public required DateTime? DateRead { get; init; }

    /// <summary>
    /// Date and time when the request was answered by the receiver
    /// </summary>
    public required DateTime? DateAnswered { get; init; }

    /// <summary>
    /// Indicates whether the request was accepted
    /// </summary>
    public required bool? IsAccepted { get; init; }

    /// <summary>
    /// ID of the sender
    /// </summary>
    public required string SenderId { get; init; }

    /// <summary>
    /// ID of the target user
    /// </summary>
    public required string ReceiverId { get; init; }

    /// <summary>
    /// Full name of the sender
    /// </summary>
    public required string SenderName { get; init; }

    /// <summary>
    /// Full name of the receiver
    /// </summary>
    public required string ReceiverName { get; init; }
}