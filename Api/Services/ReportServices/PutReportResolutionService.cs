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
    public class PutReportResolutionService(
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
        [ErrorCode(nameof(reportId), ErrorCodes.NotFound)]
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
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(reportId),
                    ErrorCode = ErrorCodes.NotFound,
                };
            }

            if (report.Resolution != null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(reportId),
                    ErrorMessage = "Report has already been resolved.",
                };
            }

            report.Resolution = new ReportResolution
            {
                SupportComment = dto.SupportComment,
                ResolvedBy = user,
                Date = DateTime.UtcNow,
            };

            context.Update(report);
            await context.SaveChangesAsync();

            return mapper.Map<ReportVM>(report);
        }
    }
}
