﻿using System.Security.Claims;
using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Dtos.Employment;
using Reservant.Api.Dtos.User;
using Reservant.Api.Dtos.Visit;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(
    UserManager<User> userManager,
    ApiDbContext dbContext,
    ValidationService validationService,
    FileUploadService uploadService)
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
    [ValidatorErrorCodes<User>]
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

        var validationResult = await validationService.ValidateAsync(user, null);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result);
        }

        if (request.IsManager)
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
    [ValidatorErrorCodes<User>]
    public async Task<Result<User>> RegisterRestaurantEmployeeAsync(
        RegisterRestaurantEmployeeRequest request, User employer, string? id = null)
    {
        var username = employer.UserName + RestaurantEmployeeLoginSeparator + request.Login.Trim();

        var employee = new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserName = username,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber.Trim(),
            RegisteredAt = DateTime.UtcNow,
            Employer = employer
        };

        var validationResult = await validationService.ValidateAsync(employee, employer.Id);
        if (!validationResult.IsValid)
        {
            return validationResult;
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
    [ValidatorErrorCodes<RegisterCustomerRequest>]
    [ValidatorErrorCodes<User>]
    public async Task<Result<User>> RegisterCustomerAsync(RegisterCustomerRequest request, string? id = null)
    {
        var result = await validationService.ValidateAsync(request, null);
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
            RegisteredAt = DateTime.UtcNow
        };

        result = await validationService.ValidateAsync(user, null);
        if (!result.IsValid)
        {
            return result;
        }

        var result2 = await userManager.CreateAsync(user, request.Password);
        if (!result2.Succeeded)
        {
            return ValidationUtils.AsValidationErrors("", result2);
        }

        await userManager.AddToRoleAsync(user, Roles.Customer);

        return user;
    }

    /// <summary>
    /// Add the RestaurantOwner role to a user
    /// </summary>
    /// <param name="id">ID of the user</param>
    public async Task<User?> MakeRestaurantOwnerAsync(string id)
    {
        var user = await dbContext.Users.Where(u => u.Id.Equals(id)).FirstOrDefaultAsync();
        if (user == null) { return null; }
        await userManager.AddToRoleAsync(user, Roles.RestaurantOwner);
        return user;
    }

    /// <summary>
    /// returns whether login provided is unique among registered users withought veryfying validity of a login
    /// </summary>
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
        var employeeVMs = await dbContext.Users
            .Where(u => u.EmployerId == userId)
            .Include(u => u.Employments)
            .ThenInclude(e => e.Restaurant)
            .Select(u => new UserEmployeeVM
            {
                UserId = u.Id,
                Login = u.UserName!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                BirthDate = u.BirthDate!.Value,
                PhoneNumber = u.PhoneNumber!,
                Employments = u.Employments
                    .Where(e => e.DateUntil == null)
                    .Select(e => new EmploymentVM
                    {
                        EmploymentId = e.Id,
                        RestaurantId = e.RestaurantId,
                        IsBackdoorEmployee = e.IsBackdoorEmployee,
                        IsHallEmployee = e.IsHallEmployee,
                        RestaurantName = e.Restaurant.Name,
                        DateFrom = e.DateFrom
                    })
                    .ToList(),
                Photo = uploadService.GetPathForFileName(u.PhotoFileName),
                FriendStatus =
                    (from fr in dbContext.FriendRequests
                     let isOutgoing = fr.SenderId == userId && fr.ReceiverId == u.Id
                     let isIncoming = fr.SenderId == u.Id && fr.ReceiverId == userId
                     let isAccepted = fr.DateAccepted != null
                     where isOutgoing || isIncoming
                     select isAccepted
                        ? FriendStatus.Friend
                        : isIncoming
                        ? FriendStatus.IncomingRequest
                        : isOutgoing
                        ? FriendStatus.OutgoingRequest
                        : FriendStatus.Stranger).FirstOrDefault()
            })
            .ToListAsync();

        return employeeVMs;
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
        return [.. await userManager.GetRolesAsync(user)];
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
        var owner = (await userManager.GetUserAsync(user))!;
        var employee = await userManager.FindByIdAsync(empId);

        if (employee is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Emp: {empId} not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (employee.EmployerId != owner.Id)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"Emp: {empId} is not employed by {owner.Id}",
                ErrorCode = ErrorCodes.MustBeCurrentUsersEmployee
            };
        }

        employee.PhoneNumber = request.PhoneNumber.Trim();
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.BirthDate = request.BirthDate;

        var result = await validationService.ValidateAsync(employee, empId);
        if (!result.IsValid)
        {
            return result;
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
        var result = await validationService.ValidateAsync(request, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        user.PhoneNumber = request.PhoneNumber.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.BirthDate = request.BirthDate;
        user.PhotoFileName = request.Photo;

        result = await validationService.ValidateAsync(user, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        await userManager.UpdateAsync(user);

        return user;
    }



    /// <summary>
    /// Gets the list of visists of user planned for the future
    /// </summary>
    /// <param name="user"></param>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<VisitSummaryVM>>> GetVisitsAsync(User user, int page, int perPage)
    {
        var query = dbContext.Visits
            .Include(r => r.Participants)
            .Include(r => r.Orders)
            .Where(x => x.ClientId == user.Id || x.Participants.Any(p => p.Id == user.Id))
            .Where(x => x.Date > DateTime.UtcNow)
            .OrderBy(x => x.Date);

        var result = await query.Select(visit => new VisitSummaryVM
        {
            VisitId = visit.Id,
            Date = visit.Date,
            NumberOfPeople = visit.NumberOfGuests + visit.Participants.Count + 1,
            Takeaway = visit.Takeaway,
            ClientId = visit.ClientId,
            RestaurantId = visit.RestaurantId,
            Deposit = visit.Deposit
        })
        .PaginateAsync(page, perPage, [], maxPerPage: 10);

        return result;
    }


    /// <summary>
    /// Gets the list of visists of user from the past
    /// </summary>
    /// <param name="user"></param>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<VisitSummaryVM>>> GetVisitHistoryAsync(User user, int page, int perPage)
    {
        var query = dbContext.Visits
            .Include(r => r.Participants)
            .Include(r => r.Orders)
            .Where(x => x.ClientId == user.Id || x.Participants.Any(p => p.Id == user.Id))
            .Where(x => x.Date < DateTime.UtcNow)
            .OrderByDescending(x => x.Date);

        var result = await query.Select(visit => new VisitSummaryVM
        {
            VisitId = visit.Id,
            Date = visit.Date,
            NumberOfPeople = visit.NumberOfGuests + visit.Participants.Count + 1,
            Takeaway = visit.Takeaway,
            ClientId = visit.ClientId,
            RestaurantId = visit.RestaurantId,
            Deposit = visit.Deposit
        })
        .PaginateAsync(page, perPage, [], maxPerPage: 10);

        return result;
    }

    /// <summary>
    /// Mark a user as deleted
    /// </summary>
    /// <param name="id">ID of the user</param>
    /// <param name="employerId">ID of the current user, must be the selected user's employer</param>
    /// <returns>Returned bool is meaningless</returns>
    public async Task<Result> ArchiveUserAsync(string id, string employerId)
    {
        var user = await dbContext.Users
            .Include(user => user.Employments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }

        if (user.EmployerId != employerId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = "Current user is must be the selected user's employer"
            };
        }

        user.BirthDate = null;
        user.Email = null;
        user.EmailConfirmed = false;
        user.NormalizedEmail = null;
        user.PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.FirstName = "DELETED";
        user.LastName = "DELETED";
        user.Photo = null;
        user.PhotoFileName = null;
        user.IsArchived = true;

        foreach (var employment in user.Employments)
        {
            employment.DateUntil = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await dbContext.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Find users
    /// </summary>
    /// <param name="name">Search by user's name</param>
    /// <param name="filter">Filter results</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FoundUserVM>>> FindUsersAsync(
        string name, UserSearchFilter filter, string currentUserId, int page, int perPage)
    {
        var query =
            from u in dbContext.Users
            join ur in dbContext.UserRoles on u.Id equals ur.UserId
            join r in dbContext.Roles on ur.RoleId equals r.Id
            where r.Name == Roles.Customer
            where u.Id != currentUserId && (u.FirstName + " " + u.LastName).Contains(name)
            select new FoundUserVM
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Photo = uploadService.GetPathForFileName(u.PhotoFileName),
                FriendStatus =
                    (from fr in dbContext.FriendRequests
                     let isOutgoing = fr.SenderId == currentUserId && fr.ReceiverId == u.Id
                     let isIncoming = fr.SenderId == u.Id && fr.ReceiverId == currentUserId
                     let isAccepted = fr.DateAccepted != null
                     where isOutgoing || isIncoming
                     select isAccepted
                        ? FriendStatus.Friend
                        : isIncoming
                        ? FriendStatus.IncomingRequest
                        : isOutgoing
                        ? FriendStatus.OutgoingRequest
                        : FriendStatus.Stranger)
                    .FirstOrDefault(),
            };

        query = filter switch
        {
            UserSearchFilter.NoFilter => query.OrderByDescending(u => u.FriendStatus),
            UserSearchFilter.FriendsOnly => query.Where(u => u.FriendStatus == FriendStatus.Friend),
            UserSearchFilter.StrangersOnly => query
                .Where(u => u.FriendStatus != FriendStatus.Friend)
                .OrderByDescending(u => u.FriendStatus),
            _ => throw new ArgumentOutOfRangeException(nameof(filter)),
        };

        return await query.PaginateAsync(page, perPage, []);
    }

    /// <summary>
    /// Retrieves detailed information about a user. If the user is an employee of the current user,
    /// returns full details. Otherwise, returns limited information.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <returns>A <see cref="UserEmployeeVM"/> with user details or a <see cref="ValidationFailure"/> if user is not found or other validation fails.</returns>
    [ErrorCode(null, ErrorCodes.AccessDenied, ErrorCodes.NotFound)]
    public async Task<Result<UserEmployeeVM>> GetUserDetailsAsync(string userId, string currentUserId)
    {
        // Pobierz żądanego użytkownika z bazy danych
        var requestedUser = await dbContext.Users
            .Include(u => u.Employments)
            .ThenInclude(e => e.Restaurant)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (requestedUser == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = $"User: {userId} not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Sprawdź, czy żądany użytkownik jest pracownikiem aktualnie zalogowanego użytkownika
        if (requestedUser.EmployerId == currentUserId)
        {
            // Zwrót pełnych danych dla pracownika
            return new UserEmployeeVM
            {
                UserId = requestedUser.Id,
                Login = requestedUser.UserName,
                FirstName = requestedUser.FirstName,
                LastName = requestedUser.LastName,
                BirthDate = requestedUser.BirthDate!.Value,
                PhoneNumber = requestedUser.PhoneNumber,
                Employments = requestedUser.Employments
                    .Where(e => e.DateUntil == null)
                    .Select(e => new EmploymentVM
                    {
                        EmploymentId = e.Id,
                        RestaurantId = e.RestaurantId,
                        IsBackdoorEmployee = e.IsBackdoorEmployee,
                        IsHallEmployee = e.IsHallEmployee,
                        RestaurantName = e.Restaurant.Name,
                        DateFrom = e.DateFrom
                    })
                    .ToList(),
                Photo = uploadService.GetPathForFileName(requestedUser.PhotoFileName),
                FriendStatus = await GetFriendStatusAsync(currentUserId, requestedUser.Id)
            };
        }

        // Jeżeli użytkownik nie jest pracownikiem, zwróć ograniczone dane
        if (await userManager.IsInRoleAsync(requestedUser, Roles.Customer)) {
            return new UserEmployeeVM
            {
                UserId = requestedUser.Id,
                FirstName = requestedUser.FirstName,
                LastName = requestedUser.LastName,
                BirthDate = requestedUser.BirthDate!.Value,
                Photo = uploadService.GetPathForFileName(requestedUser.PhotoFileName),
                FriendStatus = await GetFriendStatusAsync(currentUserId, requestedUser.Id),
                Login = null,
                PhoneNumber = null,
                Employments = null
            };
        }

        return new ValidationFailure
        {
            PropertyName = null,
            ErrorMessage = $"User: {userId} is not your emplyee or customer.",
            ErrorCode = ErrorCodes.AccessDenied
        };
    }

    /// <summary>
    /// Determines the friendship status between the current user and the target user.
    /// </summary>
    /// <param name="currentUserId">The ID of the current logged-in user.</param>
    /// <param name="targetUserId">The ID of the user whose friendship status is being checked.</param>
    /// <returns>A <see cref="FriendStatus"/> representing the friendship status between the users.</returns>
    private async Task<FriendStatus> GetFriendStatusAsync(string currentUserId, string targetUserId)
    {
        var status = await dbContext.FriendRequests
            .Where(fr => (fr.SenderId == currentUserId && fr.ReceiverId == targetUserId) ||
                         (fr.SenderId == targetUserId && fr.ReceiverId == currentUserId))
            .Select(fr => new
            {
                IsAccepted = fr.DateAccepted != null,
                IsOutgoing = fr.SenderId == currentUserId,
                IsIncoming = fr.ReceiverId == currentUserId
            })
            .FirstOrDefaultAsync();

        if (status == null)
        {
            return FriendStatus.Stranger;
        }

        if (status.IsAccepted)
        {
            return FriendStatus.Friend;
        }

        return status.IsOutgoing ? FriendStatus.OutgoingRequest : FriendStatus.IncomingRequest;
    }
}
