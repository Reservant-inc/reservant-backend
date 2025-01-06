using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services.ReportServices;
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
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ReportCustomerService>(nameof(ReportCustomerService.ReportCustomer))]
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
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ReportEmployeeService>(nameof(ReportEmployeeService.ReportEmployee))]
    public async Task<ActionResult<ReportVM>> ReportEmployee(
        ReportEmployeeRequest dto,
        [FromServices] ReportEmployeeService service)
    {
        return OkOrErrors(await service.ReportEmployee(User.GetUserId()!.Value, dto));
    }


    /// <summary>
    /// Report a lost item as a customer
    /// </summary>
    [HttpPost("report-lost-item")]
    [Authorize(Roles = $"{Roles.Customer}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ReportLostItemService>(nameof(ReportLostItemService.ReportLostItem))]
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
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ReportBugService>(nameof(ReportBugService.ReportBug))]
    public async Task<ActionResult<ReportVM>> ReportBug(
        ReportBugRequest dto,
        [FromServices] ReportBugService service)
    {
        return OkOrErrors(await service.ReportBug(User.GetUserId()!.Value, dto));
    }

    /// <summary>
    /// As a custommer support employee get reports
    /// </summary>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <param name="createdById">id of the user who created the report</param>
    /// <param name="status"></param>
    /// <param name="service"></param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>list of reports accessible by customer support employees</returns>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<GetReportsService>(nameof(GetReportsService.GetReportsAsync))]
    [Authorize(Roles = $"{Roles.CustomerSupportManager}, {Roles.CustomerSupportAgent}")]
    public async Task<ActionResult<Pagination<ReportVM>>> GetReports(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateUntil,
        [FromQuery] ReportCategory? category,
        [FromQuery] Guid? reportedUserId,
        [FromQuery] int? restaurantId,
        [FromQuery] Guid? createdById,
        [FromServices] GetReportsService service,
        [FromQuery] ReportStatus status = ReportStatus.All,
        [FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }
        return OkOrErrors(await service.GetReportsAsync(
            dateFrom, dateUntil, category, reportedUserId, restaurantId, createdById, status, page, perPage));
    }

    /// <summary>
    /// As a customer support agent escalate report to customer support manager level
    /// </summary>
    /// <param name="reportId">ID of the report to escalate</param>
    /// <param name="dto">Body of the request</param>
    /// <param name="service"></param>
    /// <returns></returns>
    [HttpPost("{reportId:int}/escalate")]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<EscalateReportService>(nameof(EscalateReportService.EscalateReportAsync))]
    public async Task<ActionResult<ReportVM>> EscalateReport(
        int reportId,
        [FromBody] EscalateReportRequest dto,
        [FromServices] EscalateReportService service)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }
        return OkOrErrors(await service.EscalateReportAsync(reportId, user, dto));
    }

    /// <summary>
    /// Resolve a report as customer support staff.
    /// </summary>
    /// <param name="reportId">The ID of the report to resolve.</param>
    /// <param name="dto">The resolution details.</param>
    /// <param name="service">The service handling the resolution.</param>
    [HttpPut("{reportId:int}/resolution")]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ResolveReportService>(nameof(ResolveReportService.ResolveReport))]
    public async Task<ActionResult<ReportVM>> ResolveReport(
        int reportId,
        ResolveReportRequest dto,
        [FromServices] ResolveReportService service)
    {
        return OkOrErrors(await service.ResolveReport(User.GetUserId()!.Value, reportId, dto));
    }
}
