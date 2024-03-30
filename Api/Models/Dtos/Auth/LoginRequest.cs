using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos.Auth;

public class LoginRequest
{
    /// <example>john@doe.pl</example>
    [Required, StringLength(50)]
    public required string Login { get; init; }

    /// <example>Pa$$w0rd</example>
    [Required, StringLength(50)]
    public required string Password { get; init; }
}
