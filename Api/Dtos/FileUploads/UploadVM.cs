using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.FileUploads;

/// <summary>
/// Information about an uploaded file
/// </summary>
public class UploadVM
{
    /// <summary>
    /// Path to use to download the file
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// File name of the file, used to include the file in requests
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// MIME content type of the file
    /// </summary>
    public required string ContentType { get; init; }
}
