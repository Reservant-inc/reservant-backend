using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Dtos.FileUpload;
using Reservant.Api.Models;
using Reservant.Api.Options;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using System.Diagnostics.CodeAnalysis;

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
        { ".jpg", FileClass.Image },
        { ".pdf", FileClass.Document }
    };

    /// <summary>
    /// Saves the given file to disk.
    /// </summary>
    /// <returns>Path to the saved file</returns>
    [ErrorCode(nameof(UploadRequest.File), ErrorCodes.FileTooBig, "File too large")]
    [ErrorCode(nameof(UploadRequest.File), ErrorCodes.UnacceptedContentType, "File content type not accepted")]
    public async Task<Result<UploadVM>> SaveFileAsync(UploadRequest request, Guid userId)
    {
        var fileSizeKb = request.File.Length / 1024;
        if (fileSizeKb > options.Value.MaxSizeKb)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.File),
                ErrorMessage = $"File too large ({fileSizeKb} Kb > {options.Value.MaxSizeKb} Kb)",
                ErrorCode = ErrorCodes.FileTooBig
            };
        }

        var contentType = request.File.ContentType;
        if (!FileExtensions.TryGetValue(contentType, out var fileExtension))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.File),
                ErrorMessage = $"File content type not accepted: {contentType}",
                ErrorCode = ErrorCodes.UnacceptedContentType
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
            FileName = fileName,
            ContentType = contentType
        };
    }

    /// <summary>
    /// Validates that the file name is of a valid upload that the user can use
    /// </summary>
    /// <param name="propertyName">Used as the member name in the returned errors</param>
    /// <param name="fileName">Value of the property to validate</param>
    /// <param name="expectedFileClass">See <see cref="FileClass"/></param>
    /// <param name="userId">User that has to have access to the file (current user)</param>
    /// <returns>Result containing the file name of the upload</returns>
    public async Task<Result<string>> ProcessUploadNameAsync(
        string fileName, Guid userId, FileClass expectedFileClass, string propertyName)
    {
        var upload = await context.FileUploads
            .Where(fu => fu.FileName == fileName && fu.UserId == userId)
            .FirstOrDefaultAsync();
        if (upload is null)
        {
            return new ValidationFailure
            {
                PropertyName = propertyName,
                ErrorMessage = "Upload not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var fileClass = GetFileClass(Path.GetExtension(fileName));
        if (fileClass != expectedFileClass)
        {
            return new ValidationFailure
            {
                PropertyName = propertyName,
                ErrorMessage = $"Expected {expectedFileClass}, got {fileClass}",
                ErrorCode = $"{ErrorCodes.FileName}.{expectedFileClass}"
            };
        }

        return fileName;
    }

    private static FileClass GetFileClass(string extension) =>
        FileClasses.GetValueOrDefault(extension, FileClass.Unknown);

    /// <summary>
    /// Returns the URL path of the uploaded file or null if fileName is null
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(fileName))]
    public string? GetPathForFileName(string? fileName) =>
        fileName == null ? null : $"{options.Value.ServePath}/{fileName}";

    /// <summary>
    /// Returns the URL of the uploaded file or null if fileName is null
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(fileName))]
    public string? GetUrlForFileName(string? fileName) =>
        fileName == null ? null : $"{options.Value.ServeUrlBase}/{fileName}";
}
