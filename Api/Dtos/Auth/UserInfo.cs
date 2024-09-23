using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.Auth;

public class UserInfo
{
    public required string UserId { get; init; }

    public required string Token { get; init; }

    public required string Login { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required List<string> Roles { get; init; }
}
