using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Validators;

namespace Reservant.Api.Validation;

/// <summary>
/// Static utility methods to help with validation.
/// </summary>
public static class ValidationUtils
{
    private static readonly CustomProblemDetailsFactory ProblemDetailsFactory = new();

    /// <summary>
    /// Add errors to ModelState.
    /// </summary>
    public static ActionResult ToValidationProblem<T>(this Result<T> result)
    {
        return new ObjectResult(
            ProblemDetailsFactory.CreateFluentValidationProblemDetails(result.Errors));
    }

    /// <summary>
    /// Convert an IdentityResult to a list of validation errors.
    /// </summary>
    public static List<ValidationFailure> AsValidationErrors(string member, IdentityResult result)
    {
        return result.Errors
            .Select(x => new ValidationFailure
            {
                PropertyName = member,
                ErrorMessage = x.Description,
                ErrorCode = ErrorCodes.IdentityError
            })
            .ToList();
    }
}
