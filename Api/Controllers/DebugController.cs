using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Push;
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

    /// <summary>
    /// Send a sample notification to a user
    /// </summary>
    [HttpPost("send-test-notification")]
    public async Task<ActionResult> SendTestNotification([FromServices] NotificationService service)
    {
        await service.NotifyNewFriendRequest(
            Guid.Parse("28b618d7-2f32-4f0c-823d-e63ffa56e47f"),
            Guid.Parse("e5779baf-5c9b-4638-b9e7-ec285e57b367"));
        return Ok();
    }
}
