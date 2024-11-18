using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Employments;
using Reservant.Api.Dtos.Events;
using Reservant.Api.Dtos.Threads;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Mapping;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Services.ReportServices;
using Reservant.Api.Dtos.Reports;

namespace Reservant.Api.Controllers;

/// <summary>
/// Manage the current user
/// </summary>
[ApiController, Route("/user")]
[Authorize]
public class UserController(
    UserManager<User> userManager,
    UserService userService,
    UrlService urlService,
    EventService eventService,
    ThreadService threadService,
    ReportCustomerService reportService
    ) : StrictController
{
    /// <summary>
    /// Get list of users employed by the current user
    /// </summary>
    /// <returns></returns>
    [HttpGet("employees")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<List<UserEmployeeVM>>> GetEmployees()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await userService.GetEmployeesAsync(userId.Value));
    }
    /// <summary>
    /// Gets information about the current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<UserDetailsVM>> GetCurrentUser()
    {

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserDetailsVM
        {
            UserId = user.Id,
            Login = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegisteredAt = user.RegisteredAt,
            BirthDate = user.BirthDate,
            Roles = await userService.GetRolesAsync(User),
            EmployerId = user.EmployerId,
            Photo = urlService.GetPathForFileName(user.PhotoFileName),
        });
    }
    /// <summary>
    /// Update information about the current user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<UserDetailsVM>> PutUser(UpdateUserDetailsRequest request)
    {

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await userService.PutUserAsync(request, user);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return Ok(new UserDetailsVM
        {
            UserId = user.Id,
            Login = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegisteredAt = user.RegisteredAt,
            BirthDate = user.BirthDate,
            Roles = await userService.GetRolesAsync(User),
            EmployerId = user.EmployerId,
            Photo = urlService.GetPathForFileName(user.PhotoFileName),
        });
    }



    /// <summary>
    /// Get list of future visits which the logged-in user participates in
    /// Sorted by Date from the closest to the farthest
    /// </summary>
    /// <returns></returns>
    [HttpGet("visits")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<VisitVM>>> GetVisits(int page = 0, int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.GetVisitsAsync(user, page, perPage);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Get list of past visits which the logged-in user participates in.
    /// Sorted by Date from the newest to the oldest
    /// </summary>
    /// <returns></returns>
    [HttpGet("visit-history")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<VisitSummaryVM>>> GetVisitHistory(int page = 0, int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.GetVisitHistoryAsync(user, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Mark an employee as deleted
    /// </summary>
    /// <param name="employeeId">ID of the employee</param>
    /// <returns></returns>
    [HttpDelete("{employeeId}")]
    [Authorize(Roles = $"{Roles.RestaurantOwner}")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    public async Task<ActionResult> ArchiveUser(Guid employeeId)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.ArchiveUserAsync(employeeId, user.Id);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get events that were either created by the user or the user is interested in them with pagination.
    /// </summary>
    /// <param name="category">Value that filters the search result</param>
    /// <param name="dateFrom">Value that specifies botton boundary of time for the search</param>
    /// <param name="dateUntil">Value that specifies upper boundary of time for the search</param>
    /// <param name="order">Value that specifies if the search should be ordered by a specific value</param>
    /// <param name="page">Page number to return.</param>
    /// <param name="perPage">Items per page.</param>
    /// <returns>Paginated list of events connected to the user.</returns>
    [HttpGet("events")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<EventService>(nameof(EventService.GetUserEventsAsync))]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<Pagination<EventSummaryVM>>> GetUserEvents(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateUntil,
        [FromQuery] EventParticipationCategory category = EventParticipationCategory.CreatedBy,
        [FromQuery] EventSorting order = EventSorting.DateCreatedDesc,
        [FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await eventService.GetUserEventsAsync(category, dateFrom, dateUntil, order, user,page,perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get threads the logged-in user participates in
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of threads the user participates in</returns>
    [HttpGet("threads")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<Pagination<ThreadVM>>> GetUserThreads([FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetUserThreadsAsync(userId.Value, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Gets current users empmloyments, can be set to include terminated xor ongoing contracts
    /// </summary>
    /// <param name="returnTerminated">parameter in query that decides whether the search should include terminated employments or not</param>
    /// <param name="employmentService"></param>
    /// <returns></returns>
    [HttpGet("employments")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<EmploymentService>(nameof(EmploymentService.GetCurrentUsersEmploymentsAsync))]
    public async Task<ActionResult<List<EmploymentVM>>> GetCurrentUsersEmployments(
        [FromQuery] bool returnTerminated, [FromServices] EmploymentService employmentService)
    {
        var result = await employmentService.GetCurrentUsersEmploymentsAsync(User.GetUserId()!.Value, returnTerminated);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get current user's settings
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<UserService>(nameof(UserService.GetSettings))]
    public async Task<ActionResult<SettingsDto>> GetSettings()
    {
        return OkOrErrors(await userService.GetSettings(User.GetUserId()!.Value));
    }

    /// <summary>
    /// Get current user's settings
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<UserService>(nameof(UserService.UpdateSettings))]
    public async Task<ActionResult<SettingsDto>> UpdateSettings(SettingsDto dto)
    {
        return OkOrErrors(await userService.UpdateSettings(User.GetUserId()!.Value, dto));
    }

    /// <summary>
    /// Gets a list of reports created by the user
    /// </summary>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <returns>list of reports created by the user</returns>
    [HttpGet("reports")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ReportCustomerService>(nameof(ReportCustomerService.GetReportsAsync))]
    [Authorize]
    public async Task<ActionResult<List<ReportVM>>> GetReports(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateUntil,
            [FromQuery] string? category,
            [FromQuery] string? reportedUserId,
            [FromQuery] int? restaurantId) {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }
        var role = "user";
        return OkOrErrors(await reportService.GetReportsAsync(user, role, dateFrom, dateUntil, category, reportedUserId, restaurantId));
    }
}
