namespace Reservant.Api.Dtos.Reports;

/// <summary>
/// Request to report a customer
/// </summary>
public class ReportCustomerRequest
{
    /// <summary>
    /// Description of the report
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// ID of the reported user
    /// </summary>
    public required Guid ReportedUserId { get; set; }

    /// <summary>
    /// ID of the related visit
    /// </summary>
    public required int VisitId { get; set; }
}
