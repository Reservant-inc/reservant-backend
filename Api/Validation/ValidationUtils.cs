using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Reservant.Api.Validation;

/// <summary>
/// Static utility methods to help with validation.
/// </summary>
public static class ValidationUtils
{
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
    /// Add errors to ModelState.
    /// </summary>
    public static ActionResult ToValidationProblem<T>(this Result<T> result)
    {
        var errors = new Dictionary<string, List<string>>();
        foreach (var error in result.Errors.Where(x => x.ErrorMessage is not null))
        {
            if (!error.MemberNames.Any())
            {
                errors.TryAdd("", []);
                errors[""].Add(error.ErrorMessage!);
                continue;
            }

            foreach (var name in error.MemberNames)
            {
                errors.TryAdd(name, []);
                errors[name].Add(error.ErrorMessage!);
            }
        }

        return new ObjectResult(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Extensions =
            {
                ["errors"] = errors.ToDictionary()
            }
        });
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
