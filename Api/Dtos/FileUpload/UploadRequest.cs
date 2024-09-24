using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.FileUpload;

/// <summary>
/// Request to upload a file to the server
/// </summary>
public class UploadRequest
{
    /// <summary>
    /// The file to upload
    /// </summary>
    public required IFormFile File { get; init; }
}
