#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Auth;

public class RegisterCustomerRequest
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Login { get; init; }

    public required string Email { get; init; }

    public required PhoneNumber? PhoneNumber { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required string Password { get; init; }
}
