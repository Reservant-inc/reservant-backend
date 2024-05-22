using FluentValidation.Results;
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

    /// <summary>
    /// Create problem details reporting Fluent Validation errors with error codes
    /// </summary>
    public ProblemDetails CreateFluentValidationProblemDetails(
        List<ValidationFailure> failures,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        var errors = new Dictionary<string, List<string>>();
        var errorCodes = new Dictionary<string, List<string>>();

        foreach (var error in failures)
        {
            var key = Utils.PropertyPathToCamelCase((error.PropertyName ?? ""));

            errors.TryAdd(key, []);
            errors[key].Add(error.ErrorMessage!);

            if (error.ErrorCode is not null)
            {
                errorCodes.TryAdd(key, []);
                errorCodes[key].Add(error.ErrorCode!);
            }
        }

        return new ProblemDetails
        {
            Status = statusCode ?? 400,
            Type = type,
            Title = title,
            Detail = detail,
            Instance = instance,
            Extensions =
            {
                ["errors"] = errors,
                ["errorCodes"] = errorCodes
            }
        };
    }
}
