using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.ReportServices
{
    /// <summary>
    /// Service to resolve reports.
    /// </summary>
    public class ResolveReportService(
        ApiDbContext context,
        ValidationService validationService,
        IMapper mapper,
        UserManager<User> userManager)
    {
        /// <summary>
        /// Resolve a report.
        /// </summary>
        /// <param name="userId">ID of the support staff resolving the report.</param>
        /// <param name="reportId">ID of the report to resolve.</param>
        /// <param name="dto">Resolution details provided by the support staff.</param>
        [ValidatorErrorCodes<ResolveReportRequest>]
        [ErrorCode(nameof(reportId), ErrorCodes.NotFound, "The specified report does not exist.")]
        [ErrorCode(nameof(reportId), ErrorCodes.AlreadyResolved, "The report has already been resolved.")]
        public async Task<Result<ReportVM>> ResolveReport(Guid userId, int reportId, ResolveReportRequest dto)
        {
            var validationResult = await validationService.ValidateAsync(dto, userId);
            if (!validationResult.IsValid) return validationResult;

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new InvalidOperationException($"User with ID {userId} authorized but cannot be found");
            }

            var report = await context.Reports
                .Include(r => r.Resolution)
                .Include(r => r.CreatedBy)
                .Include(r => r.ReportedUser)
                .Include(r => r.Visit)
                .ThenInclude(v => v!.Restaurant)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);


            if (report is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(reportId),
                    ErrorMessage = "The specified report does not exist.",
                    ErrorCode = ErrorCodes.NotFound,
                };
            }

            if (report.Resolution != null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(reportId),
                    ErrorMessage = "The report has already been resolved.",
                    ErrorCode = ErrorCodes.AlreadyResolved,
                };
            }

            report.Resolution = new ReportDecision
            {
                SupportComment = dto.SupportComment,
                ResolvedBy = user,
                Date = DateTime.UtcNow,
                IsDecisionPositive = dto.IsResolutionPositive,
            };

            context.Update(report);
            await context.SaveChangesAsync();

            return mapper.Map<ReportVM>(report);
        }
    }
}
