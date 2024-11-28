using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models;

/// <summary>
/// User report
/// </summary>
public class Report : ISoftDeletable
{
    /// <summary>
    /// Maximum length of a report description
    /// </summary>
    public const int MaxDescriptionLength = 1000;

    /// <summary>
    /// Unique ID
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// Description of the report
    /// </summary>
    [StringLength(MaxDescriptionLength)]
    public required string Description { get; set; }

    /// <summary>
    /// Date when the report was created
    /// </summary>
    public DateTime ReportDate { get; set; }

    /// <summary>
    /// Category of the report
    /// </summary>
    public ReportCategory Category { get; set; }

    /// <summary>
    /// ID of the user who created the report
    /// </summary>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// ID of the reported user
    /// </summary>
    public Guid? ReportedUserId { get; set; }

    /// <summary>
    /// ID of the related visit
    /// </summary>
    public int? VisitId { get; set; }

    /// <summary>
    /// User who created the report
    /// </summary>
    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// User being reported
    /// </summary>
    public User? ReportedUser { get; set; }

    /// <summary>
    /// Related visit
    /// </summary>
    public Visit? Visit { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
