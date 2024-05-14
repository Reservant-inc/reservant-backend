using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Models.Dtos.Employment;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Validation;


namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(UserManager<User> userManager, ApiDbContext dbContext,
    FileUploadService uploadService,
    ValidationService validationService)
{
    /// <summary>
    /// Used to generate restaurant employee's logins:
    /// '{employer's login}{separator}{employees's login}'
    /// </summary>
    private const string RestaurantEmployeeLoginSeparator = "+";

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

        var username = employer.UserName + RestaurantEmployeeLoginSeparator + request.Login.Trim();

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
        var result = await validationService.ValidateAsync(request);
        if (!result.IsValid)
        {
            return result;
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
            RegisteredAt = DateTime.UtcNow,
            PhotoFileName = request.PhotoFileName
        };
        
        result = await validationService.ValidateAsync(user);
        if (!result.IsValid)
        {
            return result;
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

    /// <summary>
    /// Return employees of the given user
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserEmployeeVM>> GetEmployeesAsync(string userId)
    {
        return await dbContext.Users
            .Where(u => u.EmployerId == userId)
            .Select(u => new UserEmployeeVM
            {
                Id = u.Id,
                Login = u.UserName!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber!,
                Employments = u.Employments!
                    .Where(e => e.DateUntil == null)
                    .Select(e => new EmploymentVM
                    {
                        RestaurantId = e.RestaurantId,
                        IsBackdoorEmployee = e.IsBackdoorEmployee,
                        IsHallEmployee = e.IsHallEmployee
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gets the employee with the given id, provided he works for the current user
    /// </summary>
    /// <param name="empId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<User>> GetEmployeeAsync(string empId, ClaimsPrincipal user)
    {

        var errors = new List<ValidationResult>();

        var owner = (await userManager.GetUserAsync(user))!;
        var emp = await userManager.FindByIdAsync(empId);

        if (emp is null)
        {
            errors.Add(new ValidationResult($"Emp: {empId} not found"));
            return errors;
        }

        if (emp.EmployerId != owner.Id)
        {
            errors.Add(new ValidationResult($"Emp: {empId} is not employed by {owner.Id}"));
            return errors;
        }

        return emp;
    }
    /// <summary>
    /// Gets roles for the given ClaimsPrincipal
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public async Task<List<string>> GetRolesAsync(ClaimsPrincipal claims)
    {
        var user = await userManager.GetUserAsync(claims);
        return [.. await userManager.GetRolesAsync(user!)];
    }

    /// <summary>
    /// Gets roles for the given User
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<List<string>> GetRolesAsync(User user)
    {
        return [.. await userManager.GetRolesAsync(user!)];
    }

    /// <summary>
    /// Updates a specified employee, provided he works for the current user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="empId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<User>> PutEmployeeAsync(UpdateUserDetailsRequest request, string empId, ClaimsPrincipal user)
    {
        var errors = new List<ValidationResult>();

        var owner = (await userManager.GetUserAsync(user))!;
        var employee = await userManager.FindByIdAsync(empId);

        if (employee is null)
        {
            errors.Add(new ValidationResult($"Emp: {empId} not found"));
            return errors;
        }

        if (employee.EmployerId != owner.Id)
        {
            errors.Add(new ValidationResult($"Emp: {empId} is not employed by {owner.Id}"));
            return errors;
        }

        employee.Email = request.Email.Trim();
        employee.PhoneNumber = request.PhoneNumber.Trim();
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.BirthDate = request.BirthDate;

        if (!ValidationUtils.TryValidate(employee, errors))
        {
            return errors;
        }

        await userManager.UpdateAsync(employee);

        return employee;
    }

    /// <summary>
    /// Updates the specified user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<User>> PutUserAsync(UpdateUserDetailsRequest request, User user)
    {
        var result = await validationService.ValidateAsync(request);
        if (!result.IsValid)
        {
            return result;
        }

        user.Email = request.Email.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.BirthDate = request.BirthDate;

        result = await validationService.ValidateAsync(user);
        if (!result.IsValid)
        {
            return result;
        }

        await userManager.UpdateAsync(user);

        return user;
    }
}
