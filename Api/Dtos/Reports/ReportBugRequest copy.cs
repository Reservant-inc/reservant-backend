namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Request to report bug
/// </summary>
public class ReportBugRequest
{
    /// <summary>
    /// Description of the report
    /// </summary>
    public required string Description { get; set; }
}
