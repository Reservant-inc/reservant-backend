using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// File upload
/// </summary>
public class FileUpload : ISoftDeletable
{
    /// <summary>
    /// Name of the file on disk
    /// </summary>
    [Key, StringLength(50)]
    public required string FileName { get; set; }

    /// <summary>
    /// MIME type of the contents of the file
    /// </summary>
    [StringLength(20)]
    public required string ContentType { get; set; }

    /// <summary>
    /// ID of the user who uploaded the file
    /// </summary>
    [Required, StringLength(36)]
    public required string UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who uploaded the file
    /// </summary>
    public User User { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
