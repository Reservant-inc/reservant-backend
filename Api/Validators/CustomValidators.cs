using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Validators;

/// <summary>
/// Custom Fluent Validation rules
/// </summary>
public static class CustomValidators
{
    /// <summary>
    /// Validates that the property contains a valid file upload name.
    /// Validation will fail if there is no upload with the file name or
    /// if the user does not own it.
    /// </summary>
    /// <param name="builder">Rule builder</param>
    /// <param name="expectedClass">Expected file class</param>
    /// <param name="uploadService">FileUploadService</param>
    public static IRuleBuilderOptions<T, string?> FileUploadName<T>(
        this IRuleBuilder<T, string?> builder,
        FileClass expectedClass,
        FileUploadService uploadService)
    {
        return builder
            .MustAsync(async (_, value, context, _) =>
            {
                if (value is null)
                {
                    return true;
                }

                var userId = (string)context.RootContextData["UserId"];
                var result = await uploadService.ProcessUploadNameAsync(
                    value, userId, expectedClass, context.PropertyPath);
                return !result.IsError;
            })
            .WithErrorCode($"FileName.{expectedClass}")
            .WithMessage($"Must be a valid {expectedClass} file upload name");
    }

    /// <summary>
    /// Validates that the property contains a valid restaurant tag.
    /// </summary>
    public static IRuleBuilderOptions<T, string> RestaurantTag<T>(
        this IRuleBuilder<T, string> builder,
        ApiDbContext dbContext)
    {
        return builder
            .MustAsync(async (_, value, cancellationToken) =>
                await dbContext.RestaurantTags.AnyAsync(rt => rt.Name == value, cancellationToken))
            .WithErrorCode("RestaurantTag")
            .WithMessage("Must be a valid restaurant tag");
    }

    /// <summary>
    /// Validates that the property contains a valid
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a>.
    /// </summary>
    public static IRuleBuilderOptions<T, string> Nip<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(NipAttribute.IsValidNip)
            .WithErrorCode("Nip")
            .WithMessage("Must be a valid NIP");
    }

    /// <summary>
    /// Validates that the property contains a valid postal code (e.g. 00-000).
    /// </summary>
    public static IRuleBuilderOptions<T, string> PostalCode<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Matches(@"^\d{2}-\d{3}$")
            .WithErrorCode("PostalCode")
            .WithMessage("Must be a valid postal code");
    }
}
