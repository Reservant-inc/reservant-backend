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
    /// Must end with /
    /// </remarks>
    [Required]
    public required string SavePath { get; init; }

    /// <summary>
    /// Path to serve the uploaded files on
    /// </summary>
    /// <remarks>
    /// Must not end with /
    /// </remarks>
    [Required]
    public required string ServePath { get; init; }

    /// <summary>
    /// URL base at which the uploaded files are accessible
    /// </summary>
    [Required]
    public required Uri ServeUrlBase { get; init; }

    /// <summary>
    /// Maximum file size in kilobytes
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxSizeKb { get; init; }

    /// <summary>
    /// Return absolute to SavePath
    /// </summary>
    public string GetFullSavePath() => Path.GetFullPath(SavePath);
}
