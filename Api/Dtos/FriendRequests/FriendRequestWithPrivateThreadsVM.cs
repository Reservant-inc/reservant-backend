using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.FriendRequests;

/// <summary>
/// DTO for friend requests with private chat threads between them
/// </summary>
public class FriendRequestWithPrivateThreadsVM
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

    /// <summary>
    /// Id of private message thread between users
    /// </summary>
    public required int? PrivateMessageThreadId { get; init; }
}
