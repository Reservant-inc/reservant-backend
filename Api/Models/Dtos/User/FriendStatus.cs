namespace Reservant.Api.Models.Dtos.User;

/// <summary>
/// User friend status
/// </summary>
public enum FriendStatus
{
    /// <summary>
    /// The user is not connected to the current user
    /// </summary>
    Stranger = 0,

    /// <summary>
    /// The user has a pending request from the current user
    /// </summary>
    OutgoingRequest,

    /// <summary>
    /// The current user has a pending request from the user
    /// </summary>
    IncomingRequest,

    /// <summary>
    /// The user is friends with the current user
    /// </summary>
    Friend,
}
