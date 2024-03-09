using System.ComponentModel.DataAnnotations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos;

public class UserInfo
{
    [Required, StringLength(50), EmailAddress]
    public required string Username { get; init; }

    [Required]
    public required List<string> Roles { get; init; }
}
