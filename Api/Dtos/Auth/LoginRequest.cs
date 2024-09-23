using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.Auth;

public class LoginRequest
{
    /// <example>JD</example>
    public required string Login { get; init; }

    /// <example>Pa$$w0rd</example>
    public required string Password { get; init; }
}
