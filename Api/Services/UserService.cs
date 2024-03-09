using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(UserManager<User> userManager)
{
    /// <summary>
    /// Register a new restaurant owner.
    /// </summary>
    public async Task<Result<User>> RegisterRestaurantOwnerAsync(RegisterRestaurantOwnerRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RegisteredAt = DateTime.UtcNow
        };

        var errors = new List<ValidationResult>();
        if (!ValidationUtils.TryValidate(user, errors))
        {
            return errors;
        }

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        return user;
    }
    /// <summary>
    /// Service used for restaurant employee registration
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<User>> RegisterRestaurantEmployeeAsync(RegisterRestaurantEmployeeRequest request) { 
        var user = new User { 
            UserName = request.Email, 
            Email = request.Email, 
            PhoneNumber = request.PhoneNumber, 
            FirstName = request.FirstName, 
            LastName = request.LastName, 
            RegisteredAt = DateTime.UtcNow 
        };

        var errors = new List<ValidationResult>();
        if (!ValidationUtils.TryValidate(user, errors))
        {
            return errors;
        }

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        return user;
    }
}
