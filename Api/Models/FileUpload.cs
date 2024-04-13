using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// File upload
/// </summary>
public class FileUpload
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
    [Required]
    public required string UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who uploaded the file
    /// </summary>
    public User? User { get; set; }
}
