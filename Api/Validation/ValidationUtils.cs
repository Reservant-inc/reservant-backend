using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    public static void AddErrorsToModel(IEnumerable<ValidationResult> errors, ModelStateDictionary modelState)
    {
        foreach (var error in errors.Where(x => x.ErrorMessage is not null))
        {
            foreach (var name in error.MemberNames)
            {
                modelState.AddModelError(name, error.ErrorMessage!);
            }
        }
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
