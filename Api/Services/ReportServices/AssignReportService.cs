using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Service responsible for assigning reports to customer support agents
/// </summary>
public class AssignReportService(
    ApiDbContext context,
    RoleManager<IdentityRole<Guid>> roleManager,
    NotificationService notificationService)
{
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
            report.AssignedAgents.Last().Until = DateTime.UtcNow;
        }

        report.AssignedAgents.Add(
            new ReportAssignment
            {
                From = DateTime.UtcNow,
                Agent = agent,
            }
        );

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
