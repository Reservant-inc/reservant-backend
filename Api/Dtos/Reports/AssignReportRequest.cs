namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Request to assign a report to a customer support agent
/// </summary>
public class AssignReportRequest
{
    /// <summary>
    /// The ID of the customer support agent to assign the report to
    /// </summary>
    public required Guid AgentId { get; init; }
}
