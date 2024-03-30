using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.FileUpload;
using Reservant.Api.Options;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing file uploads.
/// </summary>
public class FileUploadService(IOptions<FileUploadsOptions> options, ApiDbContext context)
{
    /// <summary>
    /// Saves the given file to disk.
    /// </summary>
    /// <returns>Path to the saved file</returns>
    public async Task<Result<UploadVM>> SaveFileAsync(UploadRequest request, string userId)
    {
        var fileSizeKb = request.File.Length / 1024;
        if (fileSizeKb > options.Value.MaxSizeKb)
        {
            return new List<ValidationResult>
            {
                new($"File too large ({fileSizeKb} Kb > {options.Value.MaxSizeKb} Kb)", [nameof(request.File)])
            };
        }

        var fileName = Guid.NewGuid() + ".png";

        var filePath = Path.Combine(options.Value.SavePath, fileName);
        await using var disk = new FileStream(filePath, FileMode.Create);
        await request.File.CopyToAsync(disk);

        context.FileUploads.Add(new FileUpload
        {
            FileName = fileName,
            UserId = userId
        });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (IOException) { }

            throw;
        }

        return new UploadVM
        {
            Path = Path.Combine(options.Value.ServePath, fileName)
        };
    }
}
