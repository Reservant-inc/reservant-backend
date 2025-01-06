namespace Reservant.Api.Models.Enums;

/// <summary>
/// Enum that can take value representing current state of the report
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// value for all reports
    /// </summary>
    All,

    /// <summary>
    /// Only reports that have not been resolved
    /// </summary>
    NotResolved,

    /// <summary>
    /// Only reports that have been resolved
    /// </summary>
    Resolved,
}
