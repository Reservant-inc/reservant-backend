using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing visits
/// </summary>
[ApiController, Route("/visits")]
public class VisitsController(VisitService visitService) : Controller
{

    [HttpPost()]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VisitVM>> CreateVisit(CreateVisitRequest request)
    {
        var result = await visitService.CreateVisitAsync(request);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }
    
    
}
