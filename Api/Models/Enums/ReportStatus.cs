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
    /// value only for reports that are not escalated
    /// </summary>
    NotEscalated,
    /// <summary>
    /// value only for already escalated reports
    /// </summary>
    Escalated
}
