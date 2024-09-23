using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.Auth;


public class RegisterRestaurantEmployeeRequest
{
    public required string Login { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required string PhoneNumber { get; init; }

    public required string Password { get; init; }
}
