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
/// Used to restrict allowed file types in requests
/// </summary>
public enum FileClass
{
    /// <summary>
    /// Images (png, jpeg etc.)
    /// </summary>
    Image,

    /// <summary>
    /// Documents (pdf etc.)
    /// </summary>
    Document,

    /// <summary>
    /// All other file types
    /// </summary>
    Unknown
}

/// <summary>
/// Service for managing file uploads.
/// </summary>
public class FileUploadService(IOptions<FileUploadsOptions> options, ApiDbContext context)
{
    private static readonly Dictionary<string, string> FileExtensions = new()
    {
        { "image/png", ".png" },
        { "image/jpeg", ".jpg" },
        { "application/pdf", ".pdf" }
    };

    private static readonly Dictionary<string, FileClass> FileClasses = new()
    {
        { ".png", FileClass.Image },
        { ".pdf", FileClass.Document }
    };

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

        var contentType = request.File.ContentType;
        if (!FileExtensions.TryGetValue(contentType, out var fileExtension))
        {
            return new List<ValidationResult>
            {
                new($"File content type not accepted: {contentType}", [nameof(request.File)])
            };
        }

        var fileName = Guid.NewGuid() + fileExtension;

        var filePath = Path.Combine(options.Value.GetFullSavePath(), fileName);
        await using var disk = new FileStream(filePath, FileMode.Create);
        await request.File.CopyToAsync(disk);

        context.FileUploads.Add(new FileUpload
        {
            FileName = fileName,
            ContentType = contentType,
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
            Path = GetPathForFileName(fileName),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Validates that the given path leads to a valid upload that the user can use
    /// </summary>
    /// <param name="propertyName">Used as the member name in the returned errors</param>
    /// <param name="path">Value of the property to validate</param>
    /// <param name="expectedFileClass">See <see cref="FileClass"/></param>
    /// <param name="userId">User that has to have access to the file (current user)</param>
    /// <returns>Result containing the file name of the upload</returns>
    public async Task<Result<string>> ProcessUploadUriAsync(
        string path, string userId, FileClass expectedFileClass, string propertyName)
    {
        var errors = new List<ValidationResult>();

        if (!Path.GetDirectoryName(path.AsSpan()).SequenceEqual(RemoveFinalSlash(options.Value.ServePath)))
        {
            errors.Add(new ValidationResult(
                $"Upload file must be located in {options.Value.ServePath}",
                [propertyName]));
            return errors;
        }

        var fileName = Path.GetFileName(path);
        var upload = await context.FileUploads
            .Where(fu => fu.FileName == fileName && fu.UserId == userId)
            .FirstOrDefaultAsync();
        if (upload is null)
        {
            errors.Add(new ValidationResult("Upload not found", [propertyName]));
            return errors;
        }

        var fileClass = GetFileClass(Path.GetExtension(fileName));
        if (fileClass != expectedFileClass)
        {
            errors.Add(new ValidationResult(
                $"Expected {expectedFileClass}, got {fileClass}", [propertyName]));
            return errors;
        }

        return Path.GetFileName(path);
    }

    private static ReadOnlySpan<char> RemoveFinalSlash(ReadOnlySpan<char> path) =>
        path[^1] == '/' ? path[..^1] : path;

    private static FileClass GetFileClass(string extension) =>
        FileClasses.GetValueOrDefault(extension, FileClass.Unknown);

    /// <summary>
    /// Returns the URL path of the uploaded file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetPathForFileName(string fileName) =>
        options.Value.ServePath + '/' + fileName;
}
