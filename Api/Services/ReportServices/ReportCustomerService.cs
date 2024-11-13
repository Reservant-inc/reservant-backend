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
/// Creates a customer report
/// </summary>
public class ReportCustomerService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper,
    UserManager<User> roleManager)
{
    /// <summary>
    /// Report a customer
    /// </summary>
    /// <param name="employeeId">ID of the restaurant employee reporting the client</param>
    /// <param name="dto">DTO of the report</param>
    [ValidatorErrorCodes<ReportCustomerRequest>]
    [ErrorCode(nameof(dto.ReportedUserId), ErrorCodes.HasNotVisitedRestaurant)]
    [ErrorCode(nameof(dto.ReportedUserId), ErrorCodes.MustBeCustomerId,
        "Can only report customers that have visited the restaurant")]
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

        var canReport = await HasVisitedARestaurantTheEmployeeWorksAt(dto.ReportedUserId, employeeId);
        if (!canReport)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.ReportedUserId),
                ErrorCode = ErrorCodes.HasNotVisitedRestaurant,
                ErrorMessage = "Can only report clients that have visited the restaurant",
            };
        }

        var report = new Report
        {
            Description = dto.Description,
            Category = ReportCategory.CustomerReport,
            ReportedUser = reportedUser,
            ReportDate = DateTime.UtcNow,
            CreatedBy = employee,
        };
        context.Add(report);
        await context.SaveChangesAsync();

        return mapper.Map<ReportVM>(report);
    }

    /// <summary>
    /// Check if the given client has visited a restaurant the employee works at
    /// </summary>
    private async Task<bool> HasVisitedARestaurantTheEmployeeWorksAt(Guid clientId, Guid employeeId)
    {
        return await context.Visits
            .Where(visit => visit.ClientId == clientId || visit.Participants.Any(p => p.Id == clientId))
            .AnyAsync(visit =>
                visit.Restaurant.Employments.Any(
                    employment => employment.EmployeeId == employeeId && employment.DateUntil == null)
                || visit.Restaurant.Group.OwnerId == employeeId);
    }
}
