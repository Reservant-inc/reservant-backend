using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.Auth;

public class UserInfo
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    public required string Token { get; init; }

    [Required, StringLength(50)]
    public required string Login { get; init; }

    [Required, StringLength(30)]
    public required string FirstName { get; init; }

    [Required, StringLength(30)]
    public required string LastName { get; init; }

    [Required]
    public required List<string> Roles { get; init; }
}
