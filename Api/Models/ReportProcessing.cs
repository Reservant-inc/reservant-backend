using Reservant.Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Report is being processed by a customer support employee
/// </summary>
public class ReportProcessing : ISoftDeletable
{
    /// <summary>
    /// Id of the report that gets processed
    /// </summary>
    [Key]
    public required int ReportId { get; set; }
    /// <summary>
    /// Navigational property of the Report
    /// </summary>
    public required Report Report { get; set; }
    /// <summary>
    /// Id of the customer support employee
    /// </summary>
    public required Guid CustommerSupportEmployeeId { get; set; }
    /// <summary>
    /// Navigational property of the Customer support Employee
    /// </summary>
    public required User CustomerSupportEmployee {  get; set; }
    /// <summary>
    /// Date the report processing started
    /// </summary>
    public required DateTime ProcessingStartTime { get; set; }
    /// <summary>
    /// Date the report processing ended
    /// </summary>
    public DateTime? ProcessingEndTime { get; set; }
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
