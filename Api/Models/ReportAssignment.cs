namespace Reservant.Api.Models;

/// <summary>
/// Assignment of a report to a customer support agent
/// </summary>
public class ReportAssignment
{
    /// <summary>
    /// ID of the report assigned
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// ID of the customer support agent the report is assigned to
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// When the report was assigned to the agent
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// When the report was unassigned from the agent
    /// </summary>
    public DateTime? Until { get; set; }

    /// <summary>
    /// Navigation property for the report assigned
    /// </summary>
    public Report Report { get; set; } = null!;

    /// <summary>
    /// Navigation property for the agent the report is assigned to
    /// </summary>
    public User Agent { get; set; } = null!;
}
