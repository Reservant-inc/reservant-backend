using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Services.VisitServices;
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
    [MethodErrorCodes<MakeReservationService>(nameof(MakeReservationService.MakeReservation))]
    public async Task<ActionResult<VisitSummaryVM>> CreateVisit(
        MakeReservationRequest request, [FromServices] MakeReservationService service)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.MakeReservation(request, user);
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
