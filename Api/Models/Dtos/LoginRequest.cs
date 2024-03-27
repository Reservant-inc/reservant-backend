using System.ComponentModel.DataAnnotations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos;

public class LoginRequest
{
    [Required, StringLength(50)]
    public required string Login { get; init; }

    [Required, StringLength(50)]
    public required string Password { get; init; }
}
