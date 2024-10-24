using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing visits
/// </summary>
[ApiController, Route("/visits")]
public class VisitsController(
    VisitService visitService,
    UserManager<User> userManager
    ) : StrictController
{
    /// <summary>
    /// Get visit with the provided ID
    /// </summary>
    /// <returns></returns>
    [HttpGet("{visitId:int}")]
    [ProducesResponseType(200),ProducesResponseType(400)]
    [Authorize]
    public async Task<ActionResult<VisitVM>> GetVisits(int visitId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await visitService.GetVisitByIdAsync(visitId, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Create a new visit
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<VisitService>(nameof(VisitService.CreateVisitAsync))]
    public async Task<ActionResult<VisitSummaryVM>> CreateVisit(CreateVisitRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await visitService.CreateVisitAsync(request, user);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Approves visit
    /// </summary>
    [HttpPost("{visitId}/approve")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [Authorize(Roles = Roles.RestaurantOwner + "," + Roles.RestaurantEmployee)]
    public async Task<ActionResult> ApproveVisitRequest(int visitId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return OkOrErrors(await visitService.ApproveVisitRequestAsync(visitId, user));
    }

    /// <summary>
    /// Decline visit
    /// </summary>
    [HttpPost("{visitId}/decline")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [Authorize(Roles = Roles.RestaurantOwner + "," + Roles.RestaurantEmployee)]
    public async Task<ActionResult> DeclineVisitRequest(int visitId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return OkOrErrors(await visitService.DeclineVisitRequestAsync(visitId, user));
    }
}
