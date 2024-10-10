using System.Globalization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Web;

namespace Reservant.ErrorCodeDocs.SwaggerGen;

/// <summary>
/// Operation filter that adds error code information to Swagger
/// </summary>
/// <param name="getValidatorsFromAssembly">Assembly to get validators from</param>
internal class ErrorCodesOperationFilter(Assembly getValidatorsFromAssembly) : IOperationFilter
{
    private readonly ErrorCodesAggregator _aggregator = new(getValidatorsFromAssembly);

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var errorCodes = _aggregator.GetErrorCodes(context.MethodInfo);

        var hasTitle = false;
        var description = new StringBuilder(operation.Description);

        foreach (var errorCode in errorCodes)
        {
            if (!hasTitle)
            {
                description.AppendLine($"\n<details><summary>Possible error codes</summary>");
                hasTitle = true;
            }

            var propertyName = HttpUtility.HtmlEncode(
                errorCode.PropertyName is null ? null : PropertyPathToCamelCase(errorCode.PropertyName));
            var codeDescription = HttpUtility.HtmlEncode(errorCode.Description);

            description.Append(CultureInfo.InvariantCulture,
                $"- **\"{propertyName}\": {errorCode.ErrorCode}**");
            if (errorCode.Description is not null)
            {
                description.Append(CultureInfo.InvariantCulture, $"<br>_{codeDescription}_");
            }

            description.AppendLine();
        }

        if (hasTitle)
        {
            description.AppendLine("</details>");
            operation.Description = description.ToString();
        }
    }

    /// <summary>
    /// Convert a property path to camel case (example: 'PropertyName.Test' -> 'propertyName.test')
    /// </summary>
    /// <param name="str">The string to convert</param>
    public static string PropertyPathToCamelCase(string str) =>
        string.Join('.',
            str.Split('.')
                .Select(name => name.Length == 0
                    ? name
                    : char.ToLowerInvariant(name[0]) + name[1..]));
}
