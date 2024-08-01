using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Web;

namespace ErrorCodeDocs.SwaggerGen;

/// <summary>
/// Operation filter that adds error code information to Swagger
/// </summary>
/// <param name="getValidatorsFromAssembly">Assembly to get validators from</param>
internal class ErrorCodesOperationFilter(Assembly getValidatorsFromAssembly) : IOperationFilter
{
    private readonly ErrorCodesAggregator _aggregator = new(getValidatorsFromAssembly);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var errorCodes = _aggregator.GetErrorCodes(context.MethodInfo);

        var hasTitle = false;
        var description = new StringBuilder(operation.Description);
        
        foreach (var errorCode in errorCodes)
        {
            if (!hasTitle)
            {
                description.AppendLine($"\n### Possible error codes:");
                hasTitle = true;
            }

            var propertyName = HttpUtility.HtmlEncode(errorCode.PropertyName);
            var codeDescription = HttpUtility.HtmlEncode(errorCode.Description);

            description.Append(
                $"- **\"{propertyName}\": {errorCode.ErrorCode}**");
            if (errorCode.Description is not null)
            {
                description.Append($"<br>_{codeDescription}_");
            }

            description.AppendLine();
        }

        if (hasTitle)
        {
            operation.Description = description.ToString();
        }
    }
}
