using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Information about a customer support agent assigned to a report
/// </summary>
public class AssignedAgentVM
{
    /// <summary>
    /// The agent assigned
    /// </summary>
    public required UserSummaryVM Agent { get; set; }

    /// <summary>
    /// When the agent was assigned to the report
    /// </summary>
    public required DateTime From { get; set; }

    /// <summary>
    /// When the agent was unassigned from the report
    /// </summary>
    public required DateTime? Until { get; set; }
}
