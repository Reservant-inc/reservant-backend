using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ErrorCodeDocs.SwaggerGen;

/// <summary>
/// Extension methods for SwaggerGenOptions
/// </summary>
public static class SwaggerGenOptionsExtensions
{
    /// <summary>
    /// Display possible error code descriptions in Swagger
    /// </summary>
    /// <param name="options">SwaggerGenOptions</param>
    public static void IncludeErrorCodes(this SwaggerGenOptions options)
    {
        options.OperationFilter<ErrorCodesOperationFilter>();
    }
}
