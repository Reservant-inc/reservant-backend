using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Debugging functions
/// </summary>
[ApiController, Route("/debug")]
public class DebugController(DebugService debugService) : StrictController
{
    /// <summary>
    /// Use this if you get a "no such column" error
    /// </summary>
    [HttpPost("recreate-database")]
    public async Task RecreateDatabase()
    {
        await debugService.RecreateDatabase();
    }
}
