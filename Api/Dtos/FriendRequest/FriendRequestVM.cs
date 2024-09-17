using Reservant.Api.Dtos.User;

namespace Reservant.Api.Dtos.FriendRequest;

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
    /// Date and time when the request was accepted by the receiver
    /// </summary>
    public required DateTime? DateAccepted { get; init; }

    /// <summary>
    /// Info about the other user, be it the sender or the receiver of the request
    /// </summary>
    public required UserSummaryVM OtherUser { get; init; }
}
