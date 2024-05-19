using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Visit;
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
    ) : Controller
{
    /// <summary>
    /// Get visit of provided id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200),ProducesResponseType(400)]
    public async Task<ActionResult<VisitVM>> GetVisits(int id)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await visitService.GetVisitByIdAsync(id, user);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

    [HttpPost()]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VisitSummaryVM>> CreateVisit(CreateVisitRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await visitService.CreateVisitAsync(request, user);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }
}
