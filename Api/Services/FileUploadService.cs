using Reservant.Api.Models.Dtos;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing file uploads.
/// </summary>
public class FileUploadService
{
    /// <summary>
    /// Saves the given file to disk.
    /// </summary>
    /// <returns>Path to the saved file</returns>
    public async Task<string> SaveFileAsync(UploadRequest request)
    {
        await using var disk = new FileStream("testfile.png", FileMode.Create);
        await request.File.CopyToAsync(disk);
        return "/testfile.png";
    }
}
