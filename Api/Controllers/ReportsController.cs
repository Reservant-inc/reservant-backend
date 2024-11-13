using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Identity;
using Reservant.Api.Services.ReportServices;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing reports
/// </summary>
[ApiController, Route("/reports")]
public class ReportsController : StrictController
{
    /// <summary>
    /// Report a customer as a restaurant employee
    /// </summary>
    [HttpPost("report-customer")]
    [Authorize(Roles = $"{Roles.RestaurantEmployee},{Roles.RestaurantOwner}")]
    public async Task<ActionResult<ReportVM>> ReportCustomer(
        ReportCustomerRequest dto,
        [FromServices] ReportCustomerService service)
    {
        return OkOrErrors(await service.ReportCustomer(User.GetUserId()!.Value, dto));
    }
}
