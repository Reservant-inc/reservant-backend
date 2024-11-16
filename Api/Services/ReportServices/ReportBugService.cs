using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Creates a bug report
/// </summary>
public class ReportBugService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper)
{
    /// <summary>
    /// Reports bug report
    /// </summary>
    /// <param name="userId">ID of the user reporting the bug</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportBugRequest>]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyVisitParticipant))]
    public async Task<Result<ReportVM>> ReportBug(Guid userId, ReportBugRequest dto)
    {
        var validationResult = await validationService.ValidateAsync(dto, userId);
        if (!validationResult.IsValid) return validationResult;

        var user = await context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID {userId} cannot be found");
        }

        var report = new Report
        {
            Description = dto.Description,
            Category = ReportCategory.Technical,
            ReportedUser = null,
            ReportDate = DateTime.UtcNow,
            CreatedBy = user,
            Visit = null,
        };
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }
}
