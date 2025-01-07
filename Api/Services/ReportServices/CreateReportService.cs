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
/// Service responsible for creating new reports
/// </summary>
public class CreateReportService(
    ApiDbContext context,
    ValidationService validationService,
    AuthorizationService authorizationService,
    IMapper mapper,
    UserManager<User> userManager,
    AssignReportService assignReportService)
{
    /// <summary>
    /// Reports bug report
    /// </summary>
    /// <param name="userId">ID of the user reporting the bug</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportBugRequest>]
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
        await assignReportService.AssignToFreestAgent(report);
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }

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
        if (reportedUser is null || !await userManager.IsInRoleAsync(reportedUser, Roles.Customer))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.ReportedUserId),
                ErrorCode = ErrorCodes.MustBeCustomerId,
            };
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
        await assignReportService.AssignToFreestAgent(report);
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }

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
        if (reportedUser is null || !await userManager.IsInRoleAsync(reportedUser, Roles.RestaurantEmployee))
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
        await assignReportService.AssignToFreestAgent(report);
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }

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
        await assignReportService.AssignToFreestAgent(report);
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }
}
