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
/// Creates a customer report
/// </summary>
public class ReportCustomerService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper,
    UserManager<User> roleManager,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Report a customer
    /// </summary>
    /// <param name="employeeId">ID of the restaurant employee reporting the client</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportCustomerRequest>]
    [ErrorCode(nameof(dto.ReportedUserId), ErrorCodes.MustBeCustomerId,
        "Can only report customers that have visited the restaurant")]
    [ErrorCode(nameof(dto.VisitId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantHallAccess))]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyVisitParticipant))]
    public async Task<Result<ReportVM>> ReportCustomer(Guid employeeId, ReportCustomerRequest dto)
    {
        var validationResult = await validationService.ValidateAsync(dto, employeeId);
        if (!validationResult.IsValid) return validationResult;

        var employee = await context.Users.FindAsync(employeeId);
        if (employee is null)
        {
            throw new InvalidOperationException($"User with ID {employeeId} authorized but cannot be found");
        }

        var reportedUser = await context.Users.FindAsync(dto.ReportedUserId);
        if (reportedUser is null || !await roleManager.IsInRoleAsync(reportedUser, Roles.Customer))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.ReportedUserId),
                ErrorCode = ErrorCodes.MustBeCustomerId,
            };
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

        var isHallEmployee = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, employeeId);
        if (isHallEmployee.IsError) return isHallEmployee.Errors;

        var isVisitParticipant = await authorizationService.VerifyVisitParticipant(visit.VisitId, reportedUser.Id);
        if (isVisitParticipant.IsError) return isVisitParticipant.Errors;

        var report = new Report
        {
            Description = dto.Description,
            Category = ReportCategory.CustomerReport,
            ReportedUser = reportedUser,
            ReportDate = DateTime.UtcNow,
            CreatedBy = employee,
            Visit = visit,
        };
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }
}
