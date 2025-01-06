using AutoMapper;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Service for escalating reports to customer support managers
/// </summary>
/// <param name="context"></param>
/// <param name="notificationService"></param>
/// <param name="mapper"></param>
public class EscalateReportService(
    ApiDbContext context,
    NotificationService notificationService,
    IMapper mapper)
{
    /// <summary>
    /// Function that escalates specified report to manager
    /// </summary>
    /// <param name="reportId">id of the report to escalate</param>
    /// <param name="user">user that escalates the report</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [ErrorCode(nameof(ErrorCodes.NotFound), "Report not found")]
    public async Task<Result<ReportVM>> EscalateReportAsync(int reportId, User user, EscalateReportRequest dto)
    {
        var report = await context.Reports
            .Include(r => r.Visit)
            .ThenInclude(v => v!.Restaurant)
            .SingleOrDefaultAsync(r => r.ReportId == reportId);
        if (report == null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Report not found",
            };
        }

        report.EscalatedBy = user;
        report.EscalatedById = user.Id;
        report.EscalationComment = dto.EscalationComment;
        await context.SaveChangesAsync();

        await notificationService.NotifyReportEscalated(report);

        return mapper.Map<ReportVM>(report);
    }
}
