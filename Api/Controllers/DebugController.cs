using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Data;
using Reservant.Api.Data.Seeding;
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
[DevelopmentOnly]
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
    /// Creates an example visit in the future for the current user
    /// </summary>
    [HttpPost("add-future-visit")]
    public async Task<ActionResult<VisitSummaryVM>> AddFutureVisit()
    {
        var result = await dbSeeder.AddFutureVisitAsync(User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Send a sample notification to the current user or to John Doe if the user is not logged in
    /// </summary>
    [HttpPost("send-test-notification")]
    public async Task<ActionResult> SendTestNotification([FromServices] NotificationService service)
    {
        var targetUserId = User.GetUserId() ?? Guid.Parse("e5779baf-5c9b-4638-b9e7-ec285e57b367");
        await service.NotifyNewRestaurantReview(targetUserId, 1);
        return Ok();
    }
}
