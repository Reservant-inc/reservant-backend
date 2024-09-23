using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Auth;

/// <summary>
/// Returned after creating a new user
/// </summary>
public class UserVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Login
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    public required IList<string> Roles { get; init; }
}
