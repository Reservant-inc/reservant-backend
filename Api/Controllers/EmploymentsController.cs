using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Employment;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Employment managing
/// </summary>
[ApiController, Route("/employments")]
public class EmploymentsController(UserManager<User> userManager, EmploymentService employmentService)
    : StrictController
{
    /// <summary>
    /// Terminate an employment by setting DateUntil to today's date.
    /// </summary>
    /// <param name="employmentId">ID of the employment to terminate.</param>
    /// <returns>A result indicating success or failure.</returns>
    [HttpDelete("{employmentId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<EmploymentService>(nameof(EmploymentService.DeleteEmploymentAsync))]
    public async Task<ActionResult> DeleteEmployment(int employmentId)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await employmentService.DeleteEmploymentAsync(employmentId, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Update multiple employments specified in a list.
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<EmploymentService>(nameof(EmploymentService.UpdateBulkEmploymentAsync))]
    public async Task<ActionResult> PutEmployments(List<UpdateEmploymentRequest> requests)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await employmentService.UpdateBulkEmploymentAsync(requests, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Terminate multiple employments by specifying a list of employment Ids.
    /// </summary>
    /// <param name="employmentIds"></param>
    /// <returns></returns>
    [HttpDelete]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<EmploymentService>(nameof(EmploymentService.DeleteBulkEmploymentAsync))]
    public async Task<ActionResult> BulkDeleteEmployment(List<int> employmentIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await employmentService.DeleteBulkEmploymentAsync(employmentIds, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Gets current users empmloyments, can be set to include terminated xor ongoing contracts
    /// </summary>
    /// <param name="returnTerminated">parameter in query that decides whether the search should include terminated employments or not</param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<EmploymentService>(nameof(EmploymentService.GetCurrentUsersEmploymentsAsync))]
    public async Task<ActionResult<List<EmploymentSummaryVM>>> GetCurrentUsersEmployments([FromQuery] bool returnTerminated)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await employmentService.GetCurrentUsersEmploymentsAsync(user.Id, returnTerminated);
        return OkOrErrors(result);
    }
}
