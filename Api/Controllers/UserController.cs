using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.Visit;

namespace Reservant.Api.Controllers;

/// <summary>
/// Manage the current user
/// </summary>
[ApiController, Route("/user")]
public class UserController(UserManager<User> userManager, UserService userService) : Controller
{
    /// <summary>
    /// Get list of users employed by the current user. For restaurant owners only
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
            Id = user.Id,
            Login = user.UserName!,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegisteredAt = user.RegisteredAt,
            BirthDate = user.BirthDate,
            Roles = await userService.GetRolesAsync(User),
            EmployerId = user.EmployerId,
        });
    }
    /// <summary>
    /// Updates current user
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
            Id = user.Id,
            Login = user.UserName!,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegisteredAt = user.RegisteredAt,
            BirthDate = user.BirthDate,
            Roles = await userService.GetRolesAsync(User),
            EmployerId = user.EmployerId,
        });
    }



    /// <summary>
    /// Get list of visits of logged in user
    /// </summary>
    /// <returns></returns>
    [HttpGet("visits")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<List<VisitSummaryVM>>> getVisits()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await userService.getVisitsAsync(userId);

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
    /// Get visit of provided id
    /// </summary>
    /// <returns></returns>
    [HttpGet("visits/{id:int}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<List<VisitSummaryVM>>> getVisits(int id)
    {
        var result = await userService.GetVisitByIdAsync(id);

        // if (result.IsError)
        // {
        //     return result.ToValidationProblem();
        // }
        if(result!=null)
            return Ok(result);
        return NotFound();
    }
}
