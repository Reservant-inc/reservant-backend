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
