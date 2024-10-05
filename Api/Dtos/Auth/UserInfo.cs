using System.Globalization;

namespace Reservant.Api.Dtos.Auth;

/// <summary>
/// Information about the current user, returned after a successful login
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User's ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// API token used to authorize requests
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// User's login
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// User's first name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// User's last name
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// User's preferred language
    /// </summary>
    public required string Language { get; init; }

    /// <summary>
    /// User's roles
    /// </summary>
    public required List<string> Roles { get; init; }
}
