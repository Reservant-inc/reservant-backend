using System.ComponentModel.DataAnnotations;
using System.Net;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Reservant.Api.Validation;

/// <summary>
/// Static utility methods to help with validation.
/// </summary>
public static class ValidationUtils
{
    private static readonly CustomProblemDetailsFactory ProblemDetailsFactory = new();

    /// <summary>
    /// Convert C#'s default ValidationResults to Fluent Validation's ValidationFailures
    /// </summary>
    public static IEnumerable<ValidationFailure> ConvertToValidationFailures(IEnumerable<ValidationResult> errors) =>
        errors.SelectMany(failure =>
            failure.MemberNames.Any()
                ? failure.MemberNames.Select(memberName =>
                    new ValidationFailure
                    {
                        PropertyName = memberName,
                        ErrorMessage = failure.ErrorMessage
                    })
                : [
                    new ValidationFailure
                    {
                        ErrorMessage = failure.ErrorMessage
                    }
                ]);

    /// <summary>
    /// Validate an object using validation annotations. Store any errors in <paramref name="errors"/>.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="errors">The list to put the errors into.</param>
    /// <returns>Whether the validation was successful, i.e. there were no errors.</returns>
    public static bool TryValidate(object obj, List<ValidationResult> errors)
    {
        return Validator.TryValidateObject(obj, new ValidationContext(obj), errors, validateAllProperties: true);
    }

    /// <summary>
    /// Validate an object using validation annotations. Store any errors in <paramref name="errors"/>.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="errors">The list to put the errors into.</param>
    /// <returns>Whether the validation was successful, i.e. there were no errors.</returns>
    public static bool TryValidate(object obj, List<ValidationFailure> errors)
    {
        var oldErrors = new List<ValidationResult>();
        var result = Validator.TryValidateObject(
            obj, new ValidationContext(obj), oldErrors, validateAllProperties: true);
        if (!result)
        {
            errors.AddRange(ConvertToValidationFailures(oldErrors));
        }

        return result;
    }

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
    public static List<ValidationResult> AsValidationErrors(string member, IdentityResult result)
    {
        return result.Errors
            .Select(x => new ValidationResult(x.Description, [member]))
            .ToList();
    }
}
