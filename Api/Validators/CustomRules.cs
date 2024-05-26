using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Validators;

/// <summary>
/// Custom Fluent Validation rules
/// </summary>
public static class CustomRules
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
    public static IRuleBuilderOptions<T, string> CustomerExists<T>(
        this IRuleBuilder<T, string> builder,
        UserManager<Models.User> userManager)
    {
        return builder
            .MustAsync(async (userId, cancellation) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                return user != null && await userManager.IsInRoleAsync(user, Roles.Customer);
            })
            .WithErrorCode(ErrorCodes.MustBeCustomerId)
            .WithMessage("Customer with ID {PropertyValue} does not exist.");
    }

    /// <summary>
    /// Validates that the date is today or in the future.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> DateTimeInFuture<T>(this IRuleBuilder<T, DateTime> builder)
    {
        return builder
            .Must(date => date >= DateTime.Now)
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
    /// <summary>
    /// Checks if there is at least one role assigned to the employee
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, Tuple<bool, bool>> AtLeastOneEmployeeRole<T>(this IRuleBuilder<T, Tuple<bool, bool>> builder)
    {
        return builder
            .Must((_, value, _) => value.Item1 || value.Item2)
            .WithErrorCode(ErrorCodes.AtLeastOneRoleSelected)
            .WithMessage(ErrorCodes.AtLeastOneRoleSelected);
    }

    /// <summary>
    /// Validate that the given ID corresponds to current user's employee
    /// </summary>
    public static IRuleBuilderOptions<T, string> CurrentUsersEmployee<T>(this IRuleBuilder<T, string> builder, ApiDbContext db)
    {
        return builder.MustAsync(async (_, value, context, _) =>
        {
            if (value is null)
            {
                return false;
            }

            var user = await db.Users.FindAsync(value);
            if (user is null)
            {
                return false;
            }

            var userId = (string)context.RootContextData["UserId"];
            return userId is null || user.EmployerId == userId;
        })
        .WithErrorCode(ErrorCodes.MustBeCurrentUsersEmployee)
        .WithMessage(ErrorCodes.MustBeCurrentUsersEmployee);
    }

    /// <summary>
    /// Validates that the given Restaurant ID exists.
    /// </summary>
    public static IRuleBuilderOptions<T, int> RestaurantExists<T>(this IRuleBuilder<T, int> builder, ApiDbContext dbContext)
    {
        return builder
            .MustAsync(async (restaurantId, cancellationToken) =>
            {
                return await dbContext.Restaurants
                    .AnyAsync(r => r.Id == restaurantId, cancellationToken);
            })
            .WithMessage("The specified Restaurant ID does not exist.")
            .WithErrorCode(ErrorCodes.RestaurantDoesNotExist);
    }

    /// <summary>
    /// Validates that the given Table ID exists within the specified Restaurant ID.
    /// </summary>
    public static IRuleBuilderOptions<T, Tuple<int, int>> TableExistsInRestaurant<T>(this IRuleBuilder<T, Tuple<int, int>> builder, ApiDbContext dbContext)
    {
        return builder
            .MustAsync(async (tuple, cancellationToken) =>
            {
                var (restaurantId, tableId) = tuple;
                return await dbContext.Tables
                    .AnyAsync(t => t.Id == tableId && t.RestaurantId == restaurantId, cancellationToken);
            })
            .WithMessage("The specified Table ID does not exist within the given Restaurant ID.")
            .WithErrorCode(ErrorCodes.TableDoesNotExist);
    }

    /// <summary>
    /// Validates that the value is greater than or equal to zero.
    /// </summary>
    public static IRuleBuilderOptions<T, double> GreaterOrEqualToZero<T>(
        this IRuleBuilder<T, double> builder)
    {
        return builder
            .Must(value => value >= 0)
            .WithErrorCode(ErrorCodes.ValueLessThanZero)
            .WithMessage("The value must be greater than or equal to zero.");
    }
}
