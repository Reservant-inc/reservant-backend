using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Reservant.Api.Validation;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Custom problem details factory that converts property names to camel case to match JSON
/// </summary>
public class CustomProblemDetailsFactory : ProblemDetailsFactory
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        return new ProblemDetails
        {
            Status = statusCode ?? 500,
            Type = type,
            Title = title,
            Detail = detail,
            Instance = instance
        };
    }

    /// <inheritdoc />
    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext, ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        var errors = modelStateDictionary.ToDictionary(
            pair => Utils.PropertyPathToCamelCase(pair.Key),
            pair => pair.Value!.Errors
                .Select(e => e.ErrorMessage)
                .ToArray()
        );

        return new ValidationProblemDetails
        {
            Status = statusCode ?? 400,
            Type = type,
            Title = title,
            Detail = detail,
            Instance = instance,
            Errors = errors
        };
    }
}
