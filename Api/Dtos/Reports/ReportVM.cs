using Reservant.Api.Dtos.Users;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// DTO of a report
/// </summary>
public class ReportVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int ReportId { get; set; }

    /// <summary>
    /// Description of the report
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Date when the report was created
    /// </summary>
    public required DateTime ReportDate { get; set; }

    /// <summary>
    /// Category of the report
    /// </summary>
    public required ReportCategory Category { get; set; }

    /// <summary>
    /// User who created the report
    /// </summary>
    public required UserSummaryVM CreatedBy { get; set; }

    /// <summary>
    /// User being reported
    /// </summary>
    public required UserSummaryVM? ReportedUser { get; set; }

    /// <summary>
    /// Related visit (if any).
    /// </summary>
    public VisitSummaryVM? Visit { get; set; }

    /// <summary>
    /// The comment provided by the support staff (nullable).
    /// </summary>
    public string? ResolutionComment { get; set; }

    /// <summary>
    /// The name of the support staff who resolved the report (nullable).
    /// </summary>
    public UserSummaryVM? ResolvedBy { get; set; }

    /// <summary>
    /// The date when the report was resolved (nullable).
    /// </summary>
    public DateTime? ResolutionDate { get; set; }

    /// <summary>
    /// Customer support agents assigned to the report
    /// </summary>
    public required  List<AssignedAgentVM> AssignedAgents { get; set; }
}
