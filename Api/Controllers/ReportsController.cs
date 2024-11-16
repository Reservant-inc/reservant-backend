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

    /// <summary>
    /// Report a restaurant employee as a customer
    /// </summary>
    [HttpPost("report-employee")]
    [Authorize(Roles = $"{Roles.Customer}")]
    public async Task<ActionResult<ReportVM>> ReportEmployee(
        ReportEmployeeRequest dto,
        [FromServices] ReportEmployeeService service)
    {
        return OkOrErrors(await service.ReportEmpolyee(User.GetUserId()!.Value, dto));
    }


    /// <summary>
    /// Report a lost item as a customer
    /// </summary>
    [HttpPost("report-lost-item")]
    [Authorize(Roles = $"{Roles.Customer}")]
    public async Task<ActionResult<ReportVM>> ReportLostItem(
        ReportLostItemRequest dto,
        [FromServices] ReportLostItemService service)
    {
        return OkOrErrors(await service.ReportLostItem(User.GetUserId()!.Value, dto));
    }

    /// <summary>
    /// Report a bug
    /// </summary>
    [HttpPost("report-bug")]
    [Authorize(Roles = $"{Roles.Customer}")]
    public async Task<ActionResult<ReportVM>> ReportBug(
        ReportBugRequest dto,
        [FromServices] ReportBugService service)
    {
        return OkOrErrors(await service.ReportBug(User.GetUserId()!.Value, dto));
    }
}
