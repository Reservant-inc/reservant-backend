using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;
using Reservant.Api.Models.Dtos.Visit;


namespace Reservant.Api.Controllers;

/// <summary>
/// Managing visits
/// </summary>
[ApiController, Route("/visits")]
public class VisitsController(VisitService visitService) : Controller
{
    /// <summary>
    /// Get visit of provided id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<VisitVM>> GetVisits(int id)
    {
        var result = await visitService.GetVisitByIdAsync(id);

        // if (result.IsError)
        // {
        //     return result.ToValidationProblem();
        // }
        if(result!=null)
            return Ok(result);
        return NotFound();
    }
}
