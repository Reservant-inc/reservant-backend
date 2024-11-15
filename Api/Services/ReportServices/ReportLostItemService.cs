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
/// Creates a lost item report
/// </summary>
public class ReportLostItemService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Reports lost item report
    /// </summary>
    /// <param name="customerId">ID of the client reporting the lost item</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportLostItemRequest>]
    [ErrorCode(nameof(dto.VisitId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyVisitParticipant))]
    public async Task<Result<ReportVM>> ReportLostItem(Guid customerId, ReportLostItemRequest dto)
    {
        var validationResult = await validationService.ValidateAsync(dto, customerId);
        if (!validationResult.IsValid) return validationResult;

        var customer = await context.Users.FindAsync(customerId);
        if (customer is null)
        {
            throw new InvalidOperationException($"User with ID {customerId} authorized but cannot be found");
        }

        var visit = await context.Visits.FindAsync(dto.VisitId);
        if (visit is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.VisitId),
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var isVisitParticipant = await authorizationService.VerifyVisitParticipant(visit.VisitId, customer.Id);
        if (isVisitParticipant.IsError) return isVisitParticipant.Errors;

        var report = new Report
        {
            Description = dto.Description,
            Category = ReportCategory.LostItem,
            ReportedUser = null,
            ReportDate = DateTime.UtcNow,
            CreatedBy = customer,
            Visit = visit,
        };
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }
}
