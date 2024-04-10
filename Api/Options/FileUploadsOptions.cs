using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Options;

/// <summary>
/// File upload configuration
/// </summary>
public class FileUploadsOptions
{
    /// <summary>
    /// Configuration section to read the options from
    /// </summary>
    public const string ConfigSection = "FileUploads";

    /// <summary>
    /// Folder to save file uploads to
    /// </summary>
    /// <remarks>
    /// Must exist at startup and end with /
    /// </remarks>
    [Required]
    public required string SavePath { get; init; }

    /// <summary>
    /// Path to serve the uploaded files on
    /// </summary>
    [Required]
    public required string ServePath { get; init; }

    /// <summary>
    /// Maximum file size in kilobytes
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxSizeKb { get; init; }
}