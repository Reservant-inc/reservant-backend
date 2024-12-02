using AutoMapper;
using FluentValidation.Results;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Service for escalating reports to customer support managers
/// </summary>
/// <param name="context"></param>
/// <param name="notificationService"></param>
/// <param name="mapper"></param>
public class ReportEscalatingService(
    ApiDbContext context,
    NotificationService notificationService,
    IMapper mapper)
{
    /// <summary>
    /// Function that escalates specified report to manager
    /// </summary>
    /// <param name="reportId">id of the report to escalate</param>
    /// <param name="user">user that escalates the report</param>
    /// <returns></returns>
    public async Task<Result<ReportVM>> EscalateReportAsync(int reportId, User user) { 
        var report = await context.Reports.FindAsync(reportId);
        if (report == null) {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Report not found"
            };
        }

        report.EscalatedBy = user;
        report.EscalatedById = user.Id;
        await context.SaveChangesAsync();

        await notificationService.NotifyNewEscalatedReport(reportId);

        return mapper.Map<ReportVM>(report);
    }
}
