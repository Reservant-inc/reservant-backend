using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
/// Creates a emplyee report
/// </summary>
public class ReportEmployeeService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper,
    UserManager<User> roleManager,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Report a employee
    /// </summary>
    /// <param name="customerId">ID of the client reporting the emplyee</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportCustomerRequest>]
    [ErrorCode(nameof(dto.ReportedUserId), ErrorCodes.MustBeEmployeeId,
        "Can only report emplyees that have visited the restaurant")]
    [ErrorCode(nameof(dto.VisitId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyVisitParticipant))]
    public async Task<Result<ReportVM>> ReportEmployee(Guid customerId, ReportEmployeeRequest dto)
    {
        var validationResult = await validationService.ValidateAsync(dto, customerId);
        if (!validationResult.IsValid) return validationResult;

        var customer = await context.Users.FindAsync(customerId);
        if (customer is null)
        {
            throw new InvalidOperationException($"User with ID {customerId} authorized but cannot be found");
        }

        var visit = await context.Visits
            .Include(v => v.Restaurant)
            .SingleOrDefaultAsync(v => v.VisitId == dto.VisitId);
        if (visit is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.VisitId),
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var reportedUser = await context.Users.FindAsync(dto.ReportedUserId);
        if (reportedUser is null || !await roleManager.IsInRoleAsync(reportedUser, Roles.RestaurantEmployee))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.ReportedUserId),
                ErrorCode = ErrorCodes.MustBeEmployeeId,
            };
        }

        var isHallEmployee = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, reportedUser.Id);
        if (isHallEmployee.IsError)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.ReportedUserId),
                ErrorCode = ErrorCodes.MustBeEmployeeId,
            };
        }

        var isVisitParticipant = await authorizationService.VerifyVisitParticipant(visit.VisitId, customer.Id);
        if (isVisitParticipant.IsError) return isVisitParticipant.Errors;

        var report = new Report
        {
            Description = dto.Description,
            Category = ReportCategory.RestaurantEmployeeReport,
            ReportedUser = reportedUser,
            ReportDate = DateTime.UtcNow,
            CreatedBy = customer,
            Visit = visit,
        };
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }
}
