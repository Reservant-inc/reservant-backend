using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Debugging functions
/// </summary>
[ApiController, Route("/debug")]
public class DebugController(
    DebugService debugService,
    UserManager<User> userManager
    ) : StrictController
{
    /// <summary>
    /// Use this if you get a "no such column" error
    /// </summary>
    [HttpPost("recreate-database")]
    public async Task RecreateDatabase()
    {
        await debugService.RecreateDatabase();
    }


    /// <summary>
    /// Creates random visit in the future
    /// </summary>
    [HttpPost("add-future-visit")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<VisitSummaryVM>> AddFutureVisit()
    {
        var user = await userManager.GetUserAsync(User);
        
        var result = await debugService.AddFutureVisitAsync(user);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }
        return Ok(result.Value);
    }
    
    
}
