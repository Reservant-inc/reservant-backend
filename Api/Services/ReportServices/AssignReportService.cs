using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Service responsible for assigning reports to customer support agents
/// </summary>
public class AssignReportService(
    ApiDbContext context,
    RoleManager<IdentityRole<Guid>> roleManager,
    NotificationService notificationService,
    UserManager<User> userManager)
{
    /// <summary>
    /// Assign a report to a customer support agent
    /// </summary>
    /// <param name="agentId">ID of the customer support agent to assign the report to</param>
    /// <param name="reportId">ID of the report to assign</param>
    [ErrorCode(nameof(agentId), ErrorCodes.MustBeCustomerSupportAgent)]
    [ErrorCode(nameof(reportId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(agentId), ErrorCodes.Duplicate)]
    public async Task<Result> AssignReportToAgent(Guid agentId, int reportId)
    {
        var agent = await context.Users.FindAsync(agentId);
        if (agent is null || !await userManager.IsInRoleAsync(agent, Roles.CustomerSupportAgent))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(agentId),
                ErrorCode = ErrorCodes.MustBeCustomerSupportAgent,
                ErrorMessage = $"User with ID {agentId} not found or is not a customer support agent",
            };
        }

        var report = await context.Reports
            .AsSplitQuery()
            .Include(r => r.AssignedAgents)
            .ThenInclude(ra => ra.Agent)
            .Include(r => r.Thread)
            .ThenInclude(mt => mt!.Participants)
            .SingleOrDefaultAsync(r => r.ReportId == reportId);
        if (report is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(reportId),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = $"Report with ID {reportId} not found",
            };
        }

        if (IsAssignedTo(report, agent))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(agentId),
                ErrorCode = ErrorCodes.Duplicate,
                ErrorMessage = $"The report is already assigned to the given customer support agent",
            };
        }

        await ReassignToAgent(report, agent);
        return Result.Success;
    }

    private static bool IsAssignedTo(Report report, User agent) =>
        report.AssignedAgents.Any(ra => ra.AgentId == agent.Id && ra.Until == null);

    /// <summary>
    /// Assign the report to the agent that currently has the least reports assigned
    /// </summary>
    public async Task AssignToFreestAgent(Report report)
    {
        var customerSupportAgentRole =
            await roleManager.FindByNameAsync(Roles.CustomerSupportAgent)
            ?? throw new InvalidOperationException($"Role {Roles.CustomerSupportAgent} not found");

        var freestAgent =
            await AllUsersInRole(customerSupportAgentRole)
                .GroupJoin(
                    context.Set<ReportAssignment>()
                        .Where(assignment => assignment.Until == null),
                    user => user.Id,
                    assignment => assignment.AgentId,
                    (user, assignments) => new
                    {
                        User = user,
                        ReportCount = assignments.Count(),
                    })
                .OrderBy(group => group.ReportCount)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Cannot assign the report as there are no customer support agents");

        await ReassignToAgent(report, freestAgent.User);
    }

    /// <summary>
    /// Assign the report to the given user
    /// </summary>
    private async Task ReassignToAgent(Report report, User agent)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        report.AssignedAgents ??= [];

        if (report.AssignedAgents.Count > 0)
        {
            var lastAgentAssignment = report.AssignedAgents.Last();
            lastAgentAssignment.Until = DateTime.UtcNow;
            report.Thread?.Participants.Remove(lastAgentAssignment.Agent);
        }

        report.AssignedAgents.Add(
            new ReportAssignment
            {
                From = DateTime.UtcNow,
                Agent = agent,
            }
        );

        report.Thread?.Participants.Add(agent);

        await context.SaveChangesAsync();
        await notificationService.NotifyReportAssigned(agent.Id, report);
    }

    private IQueryable<User> AllUsersInRole(IdentityRole<Guid> role) =>
        context.Users
            .Join(
                context.UserRoles.Where(ur => ur.RoleId == role.Id),
                user => user.Id, userRole => userRole.UserId,
                (user, userRole) => user);
}
