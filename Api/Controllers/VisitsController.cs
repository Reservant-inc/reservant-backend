using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;
using Reservant.Api.Models.Dtos.Visit;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing visits
/// </summary>
[ApiController, Route("/visits")]
public class VisitsController(VisitService visitService, UserManager<User> userManager) : Controller
{
    /// <summary>
    /// Get visit of provided id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<VisitVM>> GetVisits(int id)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await visitService.GetVisitByIdAsync(id,user);

        if(result!=null)
            return Ok(result);
        return NotFound();
    }
}
