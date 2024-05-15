using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
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
            .WithErrorCode($"{ErrorCodes.FileName}.{expectedClass}")
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
            .WithErrorCode(ErrorCodes.RestaurantTag)
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
            .WithErrorCode(ErrorCodes.Nip)
            .WithMessage("Must be a valid NIP");
    }

    /// <summary>
    /// Validates that the user exists in the database.
    /// </summary>
    public static IRuleBuilderOptions<T, string> UserExists<T>(
        this IRuleBuilder<T, string> builder,
        UserManager<User> userManager)
    {
        return builder
            .MustAsync(async (userId, cancellation) => 
            {
                var user = await userManager.FindByIdAsync(userId);
                return user != null;
            })
            .WithErrorCode(ErrorCodes.Participants)
            .WithMessage("User with ID {PropertyValue} does not exist.");
    }
    
    /// <summary>
    /// Validates that the date is today or in the future.
    /// </summary>
    public static IRuleBuilderOptions<T, DateOnly> DateInFuture<T>(this IRuleBuilder<T, DateOnly> builder)
    {
        return builder
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Now))
            .WithErrorCode(ErrorCodes.DateMustBeInFuture)
            .WithMessage("The date must be today or in the future.");
    }
    
    /// <summary>
    /// Validates that the property contains a valid postal code (e.g. 00-000).
    /// </summary>
    public static IRuleBuilderOptions<T, string> PostalCode<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Matches(@"^\d{2}-\d{3}$")
            .WithErrorCode(ErrorCodes.PostalCode)
            .WithMessage("Must be a valid postal code");
    }
}
