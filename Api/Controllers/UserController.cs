using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Models.Dtos.Event;

namespace Reservant.Api.Controllers;

/// <summary>
/// Manage the current user
/// </summary>
[ApiController, Route("/user")]
public class UserController(
    UserManager<User> userManager,
    UserService userService,
    FileUploadService uploadService,
    EventService eventService
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
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await userService.GetEmployeesAsync(userId));
    }
    /// <summary>
    /// Gets information about the current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<UserDetailsVM>> GetUser()
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
            Photo = user.PhotoFileName == null ? null : uploadService.GetPathForFileName(user.PhotoFileName)
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
            Photo = user.PhotoFileName == null ? null : uploadService.GetPathForFileName(user.PhotoFileName)
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
    public async Task<ActionResult<Pagination<VisitSummaryVM>>> GetVisits(int page = 0, int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.GetVisitsAsync(user, page, perPage);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }
        else
        {
            return Ok(result.Value);
        }
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

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }
        else
        {
            return Ok(result.Value);
        }
    }

    /// <summary>
    /// Get list of events created by the current user
    /// </summary>
    /// <returns></returns>
    [HttpGet("events-created")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<List<EventSummaryVM>>> GetEventsCreated()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await eventService.GetEventsCreatedAsync(user);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Mark an employee as deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.RestaurantOwner}")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    public async Task<ActionResult> ArchiveUser(string id)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.ArchiveUserAsync(id, user.Id);

        if (result.IsError) {
            return result.ToValidationProblem();
        }

        return NoContent();
    }
}
