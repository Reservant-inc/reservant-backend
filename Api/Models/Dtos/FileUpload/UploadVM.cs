using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.FileUpload;

/// <summary>
/// Information about an uploaded file
/// </summary>
public class UploadVM
{
    /// <summary>
    /// Path to use to download the file
    /// </summary>
    [Required]
    public required string Path { get; init; }
}
