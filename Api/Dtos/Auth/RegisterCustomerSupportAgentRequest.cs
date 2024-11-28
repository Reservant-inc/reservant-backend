using Reservant.Api.Models;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.Auth;

public class RegisterCustomerSupportAgentRequest
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required PhoneNumber PhoneNumber { get; init; }

    public required string Password { get; init; }

    public bool IsManager { get; init; }
}
