﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos.Auth;

public class RegisterCustomerRequest
{
    public required string FirstName { get; init; }
    
    public required string LastName { get; init; }
    
    public required string Login { get; init; }
    
    public required string Email { get; init; }
    
    public required string PhoneNumber { get; init; }
    
    public DateOnly BirthDate { get; init; }
    
    public required string Password { get; init; }
}
