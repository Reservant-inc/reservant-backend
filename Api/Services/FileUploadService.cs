using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Options;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing file uploads.
/// </summary>
public class FileUploadService(IOptions<FileUploadsOptions> options)
{
    /// <summary>
    /// Saves the given file to disk.
    /// </summary>
    /// <returns>Path to the saved file</returns>
    public async Task<Result<string>> SaveFileAsync(UploadRequest request)
    {
        var fileSizeKb = request.File.Length / 1024;
        if (fileSizeKb > options.Value.MaxSizeKb)
        {
            return new List<ValidationResult>
            {
                new($"File too large ({fileSizeKb} Kb > {options.Value.MaxSizeKb} Kb)", [nameof(request.File)])
            };
        }

        var filePath = Path.Combine(options.Value.Path, "testfile.png");
        await using var disk = new FileStream(filePath, FileMode.Create);
        await request.File.CopyToAsync(disk);
        return filePath;
    }
}
