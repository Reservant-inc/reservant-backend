namespace Reservant.Api.Models.Dtos.User;

/// <summary>
/// Basic info about a user
/// </summary>
public class UserSummaryVM
{
    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; init; }
}
