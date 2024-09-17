using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Visit;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Debugging functions
/// </summary>
[ApiController, Route("/debug")]
public class DebugController(
    DebugService debugService,
    DbSeeder dbSeeder
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
    /// Creates example visit in the future
    /// </summary>
    [HttpPost("add-future-visit")]
    public async Task<ActionResult<VisitSummaryVM>> AddFutureVisit()
    {
        var result = await dbSeeder.AddFutureVisitAsync();
        return Ok(result);
    }
    
    
}
