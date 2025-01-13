using System.Globalization;
using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Dtos.Employments;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
using Reservant.Api.Mapping;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.Orders;


namespace Reservant.Api.Services;

/// <summary>
/// Stuff for working with user records.
/// </summary>
public class UserService(
    UserManager<User> userManager,
    ApiDbContext dbContext,
    ValidationService validationService,
    UserManager<User> roleManager,
    UrlService urlService,
    IMapper mapper)
{
    /// <summary>
    /// Used to generate restaurant employee's logins:
    /// '{employer's login}{separator}{employees's login}'
    /// </summary>
    public const string RestaurantEmployeeLoginSeparator = "+";

    /// <summary>
    /// Register a new restaurant Customer Support Agent.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id">ID of the new user, if null then generated automatically</param>
    /// <param name="registrationTime">Overwrite registration time</param>
    [ValidatorErrorCodes<User>]
    public async Task<Result<User>> RegisterCustomerSupportAgentAsync(
        RegisterCustomerSupportAgentRequest request, Guid? id = null, DateTime? registrationTime = null)
    {

        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullPhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            RegisteredAt = registrationTime ?? DateTime.UtcNow,
            BannedUntil = null
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
    /// <param name="registrationTime">Overwrite registration time</param>
    /// <returns></returns>
    [ValidatorErrorCodes<User>]
    public async Task<Result<User>> RegisterRestaurantEmployeeAsync(
        RegisterRestaurantEmployeeRequest request, User employer, Guid? id = null, DateTime? registrationTime = null)
    {
        var username = employer.UserName + RestaurantEmployeeLoginSeparator + request.Login.Trim();

        var employee = new User
        {
            Id = id ?? Guid.NewGuid(),
            UserName = username,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            FullPhoneNumber = request.PhoneNumber,
            RegisteredAt = registrationTime ?? DateTime.UtcNow,
            Employer = employer,
            BannedUntil = null
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
    /// <param name="registrationTime">Overwrite the registration time</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RegisterCustomerRequest>]
    [ValidatorErrorCodes<User>]
    public async Task<Result<User>> RegisterCustomerAsync(
        RegisterCustomerRequest request, Guid? id = null, DateTime? registrationTime = null)
    {
        var result = await validationService.ValidateAsync(request, null);
        if (!result.IsValid)
        {
            return result;
        }

        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            UserName = request.Login.Trim(),
            Email = request.Email.Trim(),
            FullPhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            RegisteredAt = registrationTime ?? DateTime.UtcNow,
            BannedUntil = null
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
    public async Task<User?> MakeRestaurantOwnerAsync(Guid id)
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
    public async Task<List<UserEmployeeVM>> GetEmployeesAsync(Guid userId)
    {
        var employeeVMs = await dbContext.Users
            .Where(u => u.EmployerId == userId)
            .Include(u => u.Employments)
            .ThenInclude(e => e.Restaurant)
            .ProjectTo<UserEmployeeVM>(mapper.ConfigurationProvider)
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
    public async Task<Result<User>> PutEmployeeAsync(UpdateUserDetailsRequest request, Guid empId, ClaimsPrincipal user)
    {
        var owner = (await userManager.GetUserAsync(user))!;
        var employee = await userManager.FindByIdAsync(empId.ToString());

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

        employee.FullPhoneNumber = request.PhoneNumber;
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

        user.FullPhoneNumber = request.PhoneNumber;
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
    public async Task<Result<Pagination<VisitVM>>> GetVisitsAsync(User user, int page, int perPage)
    {
        return await dbContext.Visits
            .Where(x => x.ClientId == user.Id || x.Participants.Any(p => p.Id == user.Id))
            .Where(x => x.Reservation!.StartTime > DateTime.UtcNow)
            .OrderBy(x => x.Reservation!.StartTime)
            .ProjectTo<VisitVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, [], maxPerPage: 10, true);
    }


    /// <summary>
    /// Gets the list of visists of user from the past
    /// </summary>
    /// <param name="user"></param>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<VisitVM>>> GetVisitHistoryAsync(User user, int page, int perPage)
    {
        var query = dbContext.Visits
            .Include(r => r.Participants)
            .Include(r => r.Orders)
            .Where(x => x.ClientId == user.Id || x.Participants.Any(p => p.Id == user.Id))
            .Where(x => x.Reservation!.StartTime < DateTime.UtcNow)
            .OrderByDescending(x => x.Reservation!.StartTime);

        var result = await query
            .ProjectTo<VisitVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, [], maxPerPage: 10);

        return result;
    }

    /// <summary>
    /// Mark a user as deleted
    /// </summary>
    /// <param name="id">ID of the user to ban</param>
    /// <param name="requesterId">Id of user requesting account deletion</param>
    /// <returns>Returned bool is meaningless</returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    public async Task<Result> ArchiveUserAsync(Guid id, Guid requesterId)
    {
        var user = await dbContext.Users
            .Where(x => x.Id == id)
            .Include(u =>
                u.Employments.Where(e => e.EmployeeId == id)
                )
            .FirstOrDefaultAsync();
        if (user is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }

        var isCurrentUser = id == requesterId;
        if (!isCurrentUser && user.EmployerId != requesterId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = "Can only delete the current user or a current user's employee",
            };
        }

        user.BirthDate = null;
        user.Email = null;
        user.EmailConfirmed = false;
        user.NormalizedEmail = null;
        user.FullPhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.FirstName = "DELETED";
        user.LastName = "DELETED";
        user.Photo = null;
        user.PhotoFileName = null;
        user.IsArchived = true;
        user.PasswordHash = "";

        foreach (var employment in user.Employments)
        {
            if(employment.EmployeeId == user.Id && employment.DateUntil == null)
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
        string name, UserSearchFilter filter, Guid currentUserId, int page, int perPage)
    {
        var query = QueryUsersInRole(Roles.Customer)
            .Where(u => u.Id != currentUserId && (u.FirstName + " " + u.LastName).Contains(name))
            .Select(u => new FoundUserVM
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Photo = urlService.GetPathForFileName(u.PhotoFileName),
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
                PhoneNumber = u.FullPhoneNumber
            });

        query = filter switch
        {
            UserSearchFilter.NoFilter => query.OrderByDescending(u => u.FriendStatus),
            UserSearchFilter.FriendsOnly => query.Where(u => u.FriendStatus == FriendStatus.Friend),
            UserSearchFilter.StrangersOnly => query
                .Where(u => u.FriendStatus != FriendStatus.Friend)
                .OrderByDescending(u => u.FriendStatus),
            UserSearchFilter.CustomerSupportAgents => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter)),
        };

        return await query.PaginateAsync(page, perPage, [], 100, true);
    }

    /// <summary>
    /// Find customer support agents
    /// </summary>
    /// <param name="name">Search by agent's name</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FoundCustomerSupportAgentVM>>> FindCustomerSupportAgents(
        string? name, Guid currentUserId, int page, int perPage)
    {
        var query = QueryUsersInRole(Roles.CustomerSupportAgent)
            .Where(u => u.Id != currentUserId);

        if (name is not null)
        {
            query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(name));
        }

        return await query
            .Select(u => new FoundCustomerSupportAgentVM
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Photo = urlService.GetPathForFileName(u.PhotoFileName),
                PhoneNumber = u.FullPhoneNumber
            })
            .PaginateAsync(page, perPage, [], 100, true);
    }

    private IQueryable<User> QueryUsersInRole(string roleName) =>
        from u in dbContext.Users
        join ur in dbContext.UserRoles on u.Id equals ur.UserId
        join r in dbContext.Roles on ur.RoleId equals r.Id
        where r.Name == roleName
        select u;

    /// <summary>
    /// Retrieves detailed information about a user. If the user is an employee of the current user,
    /// returns full details. Otherwise, returns limited information.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <returns>A <see cref="UserEmployeeVM"/> with user details or a <see cref="ValidationFailure"/> if user is not found or other validation fails.</returns>
    [ErrorCode(null, ErrorCodes.AccessDenied, ErrorCodes.NotFound)]
    public async Task<Result<UserEmployeeVM>> GetUserDetailsAsync(Guid userId, Guid currentUserId)
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

        if (requestedUser.BannedUntil.HasValue && requestedUser.BannedUntil.Value < DateTime.UtcNow)
        {
            requestedUser.BannedUntil = null;
            await dbContext.SaveChangesAsync();
        }

        User? currentUser = await dbContext.Users.FindAsync(currentUserId);
        var userDto = mapper.Map<UserEmployeeVM>(requestedUser);

        // Sprawdź, czy żądany użytkownik jest pracownikiem aktualnie zalogowanego użytkownika
        if (requestedUser.EmployerId == currentUserId || currentUser!=null && await roleManager.IsInRoleAsync(currentUser, Roles.CustomerSupportAgent))
        {
            // Zwrót pełnych danych dla pracownika
            return userDto;
        }

        // Jeżeli użytkownik nie jest pracownikiem, zwróć ograniczone dane
        if (await userManager.IsInRoleAsync(requestedUser, Roles.Customer))
        {
            userDto.Login = null;
            userDto.PhoneNumber = null;
            userDto.Employments = null;
            userDto.FriendStatus = await GetFriendStatusAsync(currentUserId, userId);
            return userDto;
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
    private async Task<FriendStatus> GetFriendStatusAsync(Guid currentUserId, Guid targetUserId)
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

    /// <summary>
    /// Get a user's settings
    /// </summary>
    /// <param name="userId">ID of the user</param>
    public async Task<Result<SettingsDto>> GetSettings(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        return new SettingsDto
        {
            Language = user.Language.ToString(),
        };
    }

    /// <summary>
    /// Update a user's settings
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="dto">New settings</param>
    [ValidatorErrorCodes<SettingsDto>]
    public async Task<Result<SettingsDto>> UpdateSettings(Guid userId, SettingsDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        var result = await validationService.ValidateAsync(dto, userId);
        if (!result.IsValid)
        {
            return result;
        }

        user.Language = CultureInfo.GetCultureInfo(dto.Language);
        await dbContext.SaveChangesAsync();

        return new SettingsDto
        {
            Language = user.Language.ToString(),
        };
    }

    /// <summary>
    /// unban user based on id
    /// </summary>
    /// <param name="userId">ID of the user to be unbanned</param>
    /// <param name="currentUser">current user</param>
    [ErrorCode(nameof(userId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(userId), ErrorCodes.InvalidState, "User is not banned")]
    public async Task<Result> UnbanUserAsync(User currentUser, Guid userId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(userId),
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        if (user.BannedUntil == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(userId),
                ErrorCode = ErrorCodes.InvalidState,
            };
        }

        user.BannedUntil = null;
        await dbContext.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Reset BannedUntil for a user after the ban time has expierd
    /// </summary>
    /// <param name="user">User to be unbanned</param>
    [ErrorCode(nameof(user), ErrorCodes.InvalidState)]
    private async Task<Result> RemoveExpiredBan(User user)
    {
        if (user.BannedUntil == null || user.BannedUntil > DateTime.UtcNow)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(user),
                ErrorCode = ErrorCodes.InvalidState,
            };
        }

        user.BannedUntil = null;
        await dbContext.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Check if the user is banned and correct BannedUntil if it is outdated
    /// </summary>
    /// <returns>True if the user is banned, false otherwise</returns>
    public async ValueTask<bool> IsUserBanned(User user)
    {
        if (user.BannedUntil == null)
        {
            return false;
        }

        await RemoveExpiredBan(user);
        return false;
    }

    /// <summary>
    /// Retrieves detailed information about a user's visits and their orders.
    /// </summary>
    /// <param name="userId">The ID of the user whose visits are to be retrieved.</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    [ErrorCode(nameof(userId), ErrorCodes.NotFound, "User not found")]
    public async Task<Result<Pagination<VisitVM>>> GetUsersVisitsWithOrdersById(Guid userId, int page, int perPage)
    {
        if (!dbContext.Users.Any(u => u.Id == userId))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(userId),
                ErrorMessage = $"User: {userId} not found.",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        return await dbContext.Visits
            .Where(v => v.ClientId == userId)
            .ProjectTo<VisitVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, []);
    }

}
