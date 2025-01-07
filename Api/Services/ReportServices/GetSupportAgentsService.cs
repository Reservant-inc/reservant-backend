using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Service for retrieving Customer list of support agents
/// </summary>
/// <param name="context"></param>
public class GetSupportAgentsService(ApiDbContext context)
{
    /// <summary>
    /// Returns a list of Customer support Agents
    /// </summary>
    /// <returns></returns>
    public Result<User> GetSupportAgentWithLeastReportsAssigned() { 
    var CSAgents =
            from user in context.Users.Include("ReportProcessings")
            join userRole in context.UserRoles on user.Id equals userRole.UserId
            join role in context.Roles on userRole.RoleId equals role.Id
            where role.Name == Roles.CustomerSupportAgent
            select user;

        if (CSAgents is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NoSupportAgentFound,
                ErrorMessage = ErrorCodes.NoSupportAgentFound
            };
        }
        var agent = CSAgents.FirstOrDefault();

        if (agent is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NoSupportAgentFound,
                ErrorMessage = ErrorCodes.NoSupportAgentFound
            };
        }
        foreach (var user in CSAgents)
        {
            if (user.ReportProcessings!.Count < agent.ReportProcessings!.Count)
            {
                agent = user;
            }
        }

        return agent;
    }
}
