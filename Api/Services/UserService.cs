using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;


namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(UserManager<User> userManager, ApiDbContext dbContext)
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

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        await userManager.AddToRolesAsync(user, [Roles.RestaurantOwner]);
        return user;
    }


    /// <summary>
    /// Register a new restaurant Customer Support Agent.
    /// </summary>
    public async Task<Result<User>> RegisterCustomerSupportAgentAsync(RegisterCustomerSupportAgentRequest request)
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
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<User>> RegisterRestaurantEmployeeAsync(RegisterRestaurantEmployeeRequest request, User user) {
        
        var errors = new List<ValidationResult>();
        
        var restaurant = await dbContext.Restaurants
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId);

        if (restaurant == null)
        {
            errors.Add(new ValidationResult($"Restaurant with id {request.RestaurantId} not found.", [nameof(request.RestaurantId)]));
            return errors;
        }
        if (restaurant.Group.OwnerId != user.Id)
        {
            errors.Add(new ValidationResult($"Not authorized to access restaurant with ID {request.RestaurantId}.", [nameof(request.RestaurantId)]));
            return errors;
        }
        
        var username = restaurant.Id + "+" + request.Login;
        
        var employee = new User {
            UserName = username, 
            FirstName = request.FirstName, 
            LastName = request.LastName, 
            PhoneNumber = request.PhoneNumber, 
            RegisteredAt = DateTime.UtcNow,
        };
        
        if (!request.IsBackdoorEmployee && !request.IsHallEmployee) { 
            errors.Add(new ValidationResult("At least one role must be set as true", ["IsBackdoorEmployee", "IsHallEmployee"]));
            return errors;
        }

        if (!ValidationUtils.TryValidate(employee, errors))
        {
            return errors;
        }

        var result = await userManager.CreateAsync(employee, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }
        if(request.IsHallEmployee && request.IsBackdoorEmployee)
            await userManager.AddToRolesAsync(employee, [Roles.RestaurantEmployee, Roles.RestaurantBackdoorsEmployee, Roles.RestaurantHallEmployee]);
        else if(request.IsHallEmployee)
            await userManager.AddToRolesAsync(employee, [Roles.RestaurantEmployee, Roles.RestaurantHallEmployee]);
        else
            await userManager.AddToRolesAsync(employee, [Roles.RestaurantEmployee, Roles.RestaurantBackdoorsEmployee]);
        return employee;
    }

    public async Task<Result<User>> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        var errors = new List<ValidationResult>();
        if(request.Login.Contains('+'))
        {
            errors.Add(new ValidationResult($"Login can't contain '+' sign.", [nameof(request.Login)]));
            return errors;
        }
        
        var user = new User
        {
            UserName = request.Login,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = DateOnly.FromDateTime(request.BirthDate),
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

    /// <summary>
    /// returns whether mail provided is unique among registered users
    /// </summary>
    /// <returns>Task<bool></returns>
    public async Task<bool> isUniqueAsync(string mail)
    {
        var result = await dbContext
            .Users
            .Where(r => r.Email == mail)
            .CountAsync();
        
        return (result==0);     
    }





}
