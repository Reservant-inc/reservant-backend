using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.FileUpload;

/// <summary>
/// Request to upload a file to the server
/// </summary>
public class UploadRequest
{
    /// <summary>
    /// The file to upload
    /// </summary>
    [Required]
    public required IFormFile File { get; init; }
}
