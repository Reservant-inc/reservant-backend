using System.ComponentModel.DataAnnotations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos;


public class RegisterRestaurantEmployeeRequest
{
    [Required, StringLength(30)]
    public required string FirstName { get; init; }

    [Required, StringLength(30)]
    public required string LastName { get; init; }

    [Required, StringLength(50), EmailAddress]
    public required string Email { get; init; }

    [Required, StringLength(15), Phone]
    public required string PhoneNumber { get; init; }

    [Required, StringLength(50)]
    public required string Password { get; init; }

    [Required]
    public required  bool IsBackdoorEmployee { get; init; }
    [Required]
    public required  bool IsHallEmployee { get; init; }

}
