using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services.ReportServices;
using Reservant.Api.Validation;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing reports
/// </summary>
[ApiController, Route("/reports")]
public class ReportsController(UserManager<User> userManager) : StrictController
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

    /// <summary>
    /// As a custommer support employee get reports
    /// </summary>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <param name="service"></param>
    /// <returns>list of reports accessible by customer support employees</returns>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<GetReportsService>(nameof(GetReportsService.GetReportsAsync))]
    [Authorize(Roles = $"{Roles.CustomerSupportManager}, {Roles.CustomerSupportAgent}")]
    public async Task<ActionResult<List<ReportVM>>> GetReports(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateUntil,
        [FromQuery] ReportCategory? category,
        [FromQuery] Guid? reportedUserId,
        [FromQuery] int? restaurantId,
        [FromServices] GetReportsService service)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }
        return OkOrErrors(await service.GetReportsAsync(dateFrom, dateUntil, category, reportedUserId, restaurantId));
    }
}
