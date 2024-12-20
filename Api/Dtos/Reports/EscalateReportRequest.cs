namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Request to escalate a report
/// </summary>
public class EscalateReportRequest
{
    /// <summary>
    /// Comment for the manager
    /// </summary>
    public required string EscalationComment { get; set; }
}
