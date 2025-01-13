namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Filtering options when searching for users
/// </summary>
public enum UserSearchFilter
{
    /// <summary>
    /// Return all users
    /// </summary>
    NoFilter = 0,

    /// <summary>
    /// Return only users that the current user is friends with
    /// </summary>
    FriendsOnly,

    /// <summary>
    /// Return only users that the current user is not friends with
    /// </summary>
    StrangersOnly,

    /// <summary>
    /// Return only customer support agents
    /// </summary>
    CustomerSupportAgents,
}
