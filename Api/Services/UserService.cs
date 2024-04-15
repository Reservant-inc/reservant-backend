using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Validation;


namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(UserManager<User> userManager, ApiDbContext dbContext)
{
    /// <summary>
    /// Register a new restaurant Customer Support Agent.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id">ID of the new user, if null then generated automatically</param>
    public async Task<Result<User>> RegisterCustomerSupportAgentAsync(
        RegisterCustomerSupportAgentRequest request, string? id = null)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
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

        if(request.IsManager)
        {
        await userManager.AddToRolesAsync(user, [Roles.CustomerSupportManager]);
        }
        await userManager.AddToRolesAsync(user, [Roles.CustomerSupportAgent]);

        return user;
    }
    /// <summary>
    /// Service used for restaurant employee registration
    /// </summary>
    /// <param name="request"></param>
    /// <param name="employer"></param>
    /// <param name="id">ID of the new user, if null then generated automatically</param>
    /// <returns></returns>
    public async Task<Result<User>> RegisterRestaurantEmployeeAsync(
        RegisterRestaurantEmployeeRequest request, User employer, string? id = null)
    {

        var errors = new List<ValidationResult>();

        var username = employer.UserName + "+" + request.Login.Trim();

        var employee = new User {
            Id = id ?? Guid.NewGuid().ToString(),
            UserName = username,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            RegisteredAt = DateTime.UtcNow,
            Employer = employer
        };

        if (!ValidationUtils.TryValidate(employee, errors))
        {
            return errors;
        }

        var result = await userManager.CreateAsync(employee, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        await userManager.AddToRolesAsync(employee, [Roles.RestaurantEmployee]);
        return employee;
    }

    /// <summary>
    /// Register a user with Customer role
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id">ID of the new user, if null then generated automatically</param>
    /// <returns></returns>
    public async Task<Result<User>> RegisterCustomerAsync(RegisterCustomerRequest request, string? id = null)
    {
        var errors = new List<ValidationResult>();
        if(request.Login.Contains('+'))
        {
            errors.Add(new ValidationResult($"Login can't contain '+' sign.", [nameof(request.Login)]));
            return errors;
        }

        var user = new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserName = request.Login.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            RegisteredAt = DateTime.UtcNow
        };


        if (!ValidationUtils.TryValidate(user, errors))
        {
            return errors;
        }

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        await userManager.AddToRoleAsync(user, Roles.Customer);

        return user;
    }

    public async Task<Result<User>> MakeRestaurantOwnerAsync(string id) {
        var user = await dbContext.Users.Where(u => u.Id.Equals(id)).FirstOrDefaultAsync();
        if (user == null) { return user; }
        await userManager.AddToRoleAsync(user, Roles.RestaurantOwner);
        return user;
    }

    /// <summary>
    /// returns whether login provided is unique among registered users
    /// </summary>
    /// <returns>Task<bool></returns>
    public async Task<bool> IsUniqueLoginAsync(string login)
    {
        var result = await dbContext
            .Users
            .Where(r => r.UserName == login)
            .AnyAsync();

        return (!result);
    }





}
