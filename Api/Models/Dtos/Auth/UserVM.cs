using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Auth;

/// <summary>
/// Returned after creating a new user
/// </summary>
public class UserVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Required]
    public required string Id { get; init; }

    /// <summary>
    /// Login
    /// </summary>
    [Required]
    public required string Login { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    [Required]
    public required IList<string> Roles { get; init; }
}
