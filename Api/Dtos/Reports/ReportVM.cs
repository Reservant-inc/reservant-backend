using Reservant.Api.Dtos.Users;
using Reservant.Api.Models;
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
    /// The ID of the related visit (if any).
    /// </summary>
    public int? VisitId { get; set; }

    /// <summary>
    /// The comment provided by the support staff (nullable).
    /// </summary>
    public string? SupportComment { get; set; }

    /// <summary>
    /// The name of the support staff who resolved the report (nullable).
    /// </summary>
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// The date when the report was resolved (nullable).
    /// </summary>
    public DateTime? ResolutionDate { get; set; }
}
