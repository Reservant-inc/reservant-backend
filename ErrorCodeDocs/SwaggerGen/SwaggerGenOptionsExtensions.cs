using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Reservant.ErrorCodeDocs.SwaggerGen;

/// <summary>
/// Extension methods for SwaggerGenOptions
/// </summary>
public static class SwaggerGenOptionsExtensions
{
    /// <summary>
    /// Display possible error code descriptions in Swagger
    /// </summary>
    /// <param name="options">SwaggerGenOptions</param>
    /// <param name="getValidatorsFromAssembly">Assembly to get validators from</param>
    public static void IncludeErrorCodes(
        this SwaggerGenOptions options, Assembly getValidatorsFromAssembly)
    {
        options.AddOperationFilterInstance(
            new ErrorCodesOperationFilter(getValidatorsFromAssembly));
    }
}
