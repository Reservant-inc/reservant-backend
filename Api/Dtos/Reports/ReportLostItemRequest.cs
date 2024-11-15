namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Request to report a lost item
/// </summary>
public class ReportLostItemRequest
{
    /// <summary>
    /// Description of the report
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// ID of the related visit
    /// </summary>
    public required int VisitId { get; set; }
}
