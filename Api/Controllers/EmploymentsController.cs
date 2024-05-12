using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Employment managing
/// </summary>
[ApiController, Route("/employments")]
public class EmploymentsController(UserManager<User> userManager, EmploymentService employmentService) : Controller
{
    /// <summary>
    /// Terminate an employment by setting DateUntil to today's date.
    /// </summary>
    /// <param name="employmentId">ID of the employment to terminate.</param>
    /// <returns>A result indicating success or failure.</returns>
    [HttpDelete("{employmentId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> DeleteEmployment(int employmentId)
    {
        var userId = userManager.GetUserId(User);
        var result = await employmentService.DeleteEmploymentAsync(employmentId, userId);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }
}
