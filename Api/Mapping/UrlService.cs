using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Reservant.Api.Options;

namespace Reservant.Api.Mapping;

/// <summary>
/// Methods for getting URLs of resources in the API
/// </summary>
/// <param name="options"></param>
public class UrlService(IOptions<FileUploadsOptions> options)
{
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
    public Uri? GetUrlForFileName(string? fileName) =>
        fileName == null ? null : new Uri(options.Value.ServeUrlBase, fileName);
}
