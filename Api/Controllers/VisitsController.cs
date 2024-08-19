using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Visit;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

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
}
