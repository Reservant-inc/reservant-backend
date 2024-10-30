using System.Globalization;
using System.Numerics;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Dtos.OrderItems;
using NetTopologySuite.Geometries;
using Reservant.Api.Dtos.Location;
using System.Text.RegularExpressions;
using Reservant.Api.Dtos.Restaurants;

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

                var userId = (Guid?)context.RootContextData["UserId"];
                var result = await uploadService.ProcessUploadNameAsync(
                    value, userId!.Value, expectedClass, context.PropertyPath);
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
            .Must(Utils.IsValidNip)
            .WithErrorCode(ErrorCodes.Nip)
            .WithMessage("Must be a valid NIP");
    }

    /// <summary>
    /// Validates that the user exists in the database.
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> CustomerExists<T>(
        this IRuleBuilder<T, Guid> builder,
        UserManager<Models.User> userManager)
    {
        return builder
            .MustAsync(async (userId, _) =>
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
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
            .Must(date => date >= DateTime.UtcNow)
            .WithErrorCode(ErrorCodes.DateMustBeInFuture)
            .WithMessage("The date must be today or in the future.");
    }

    /// <summary>
    /// Validates that the date is today or in the future, or is null (implying it will be added in the future).
    /// </summary>
    public static IRuleBuilderOptions<T, DateOnly?> DateInFuture<T>(this IRuleBuilder<T, DateOnly?> builder)
    {
        return builder
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode(ErrorCodes.DateMustBeInFuture)
            .WithMessage("The date must be today or in the future, or be null.");
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
    public static IRuleBuilderOptions<T, Guid> CurrentUsersEmployee<T>(this IRuleBuilder<T, Guid> builder, ApiDbContext db)
    {
        return builder.MustAsync(async (_, value, context, cancelToken) =>
        {
            var user = await db.Users.FindAsync([value], cancelToken);
            if (user is null)
            {
                return false;
            }

            var userId = (Guid?)context.RootContextData["UserId"];
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
                    .OnlyActiveRestaurants()
                    .AnyAsync(r => r.RestaurantId == restaurantId, cancellationToken);
            })
            .WithMessage("The specified Restaurant ID does not exist.")
            .WithErrorCode(ErrorCodes.RestaurantDoesNotExist);
    }

    /// <summary>
    /// Validates that the restaurant with the given ID exists, if the ID is not null.
    /// </summary>
    public static IRuleBuilderOptions<T, int?> RestaurantExists<T>(this IRuleBuilder<T, int?> builder, ApiDbContext dbContext)
    {
        return builder
            .MustAsync(async (restaurantId, cancellationToken) =>
            {
                if (restaurantId is not null)
                {
                    return true;
                }

                return await dbContext.Restaurants
                    .OnlyActiveRestaurants()
                    .AnyAsync(r => r.RestaurantId == restaurantId, cancellationToken);
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
                    .AnyAsync(t => t.TableId == tableId && t.RestaurantId == restaurantId, cancellationToken);
            })
            .WithMessage("The specified Table ID does not exist within the given Restaurant ID.")
            .WithErrorCode(ErrorCodes.TableDoesNotExist);
    }

    /// <summary>
    /// Validates that the orderItem exists in the database.
    /// </summary>
    public static IRuleBuilderOptions<T, CreateOrderItemRequest> OrderItemExist<T>(
        this IRuleBuilder<T, CreateOrderItemRequest> builder, ApiDbContext context)
    {
        return builder
            .MustAsync(async (item, cancellationToken) =>
            {
                var itemExists = await context.MenuItems
                    .AnyAsync(m => m.MenuItemId == item.MenuItemId, cancellationToken);

                return itemExists;
            })
            .WithErrorCode(ErrorCodes.NotFound)
            .WithMessage("The order item with MenuItemId {PropertyValue} does not exist in the database.");
    }

    /// <summary>
    /// Validates that the visit exists in the database.
    /// </summary>
    public static IRuleBuilderOptions<T, int> VisitExist<T>(
        this IRuleBuilder<T, int> builder, ApiDbContext context)
    {
        return builder
            .MustAsync(async (visitId, cancellationToken) =>
            {
                var visitExists = await context.Visits
                    .AnyAsync(v => v.VisitId == visitId, cancellationToken);

                return visitExists;
            })
            .WithErrorCode(ErrorCodes.NotFound)
            .WithMessage("Visit with Id {PropertyValue} does not exist in the database.");
    }


    /// <summary>
    /// Validates that the value is greater than or equal to zero.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> GreaterOrEqualToZero<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder) where TProperty : INumber<TProperty>
    {
        return builder
            .GreaterThanOrEqualTo(TProperty.Zero)
            .WithErrorCode(ErrorCodes.ValueLessThanZero)
            .WithMessage("The value must be greater than or equal to zero.");
    }

    /// <summary>
    /// Validates that the value is greater than or equal to zero.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty?> GreaterOrEqualToZero<T, TProperty>(
        this IRuleBuilder<T, TProperty?> builder) where TProperty : struct, INumber<TProperty>
    {
        return builder
            .Must(value => value is null || value.Value! >= TProperty.Zero)
            .WithErrorCode(ErrorCodes.ValueLessThanZero)
            .WithMessage("The value must be greater than or equal to zero.");
    }

    /// <summary>
    /// Validates that the value is greater than or equal to one.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> GreaterOrEqualToOne<T, TProperty>(
        this IRuleBuilder<T, TProperty> builder) where TProperty : INumber<TProperty>
    {
        return builder
            .GreaterThanOrEqualTo(TProperty.One)
            .WithErrorCode(ErrorCodes.ValueLessThanOne)
            .WithMessage("The value must be greater than or equal to one.");
    }

    /// <summary>
    /// Validates that the value is greater than or equal to one.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty?> GreaterOrEqualToOne<T, TProperty>(
        this IRuleBuilder<T, TProperty?> builder) where TProperty : struct, INumber<TProperty>
    {
        return builder
            .Must(value => value is null || value.Value! >= TProperty.One)
            .WithErrorCode(ErrorCodes.ValueLessThanOne)
            .WithMessage("The value must be greater than or equal to one.");
    }

    /// <summary>
    /// Validates that the list is not empty.
    /// </summary>
    public static IRuleBuilderOptions<T, List<TElement>> NotEmptyList<T, TElement>(
        this IRuleBuilder<T, List<TElement>> builder)
    {
        return builder
            .NotEmpty()
            .WithErrorCode(ErrorCodes.EmptyList)
            .WithMessage("List cannot be empty.");
    }

    /// <summary>
    /// Validates that the string is a valid name
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidName<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(name => name != null && name.All(
                c => char.IsLetter(c)|| c == ' ' || c == '-' || c == '\'' || c == '.'))
            .WithErrorCode(ErrorCodes.MustBeValidName)
            .WithMessage("The name must contain only letters, spaces, hyphens, apostrophes, or periods.");
    }

    /// <summary>
    /// Validates that the string is a valid city
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidCity<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(name => name != null && name.All(
                c => char.IsLetter(c) || c == ' ' || c == '-'))
            .WithErrorCode(ErrorCodes.MustBeValidCity)
            .WithMessage("The city must contain only letters, spaces, hyphens.");
    }

    /// <summary>
    /// Validates that the string is a valid login
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidLogin<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(login => login != null && login.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_'))
            .WithErrorCode(ErrorCodes.MustBeValidLogin)
            .WithMessage("The login must contain only ASCII letters, numbers, '-', and '_'.");
    }

    /// <summary>
    /// Validates that the string is a valid address.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidAddress<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(address => address != null && address.All(
                c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == ',' || c == '.' || c == '/'))
            .WithErrorCode(ErrorCodes.MustBeValidAddress)
            .WithMessage("The address must contain only letters, numbers, spaces, hyphens, commas, periods, or slashes.");
    }


    /// <summary>
    /// Validates that the location has valid coordinates
    /// </summary>
    public static IRuleBuilderOptions<T, Point> IsValidLocation<T>(
        this IRuleBuilder<T, Point> builder)
    {
        return builder
            .Must(loc => loc is { X: >= -180 and <= 180, Y: >= -90 and <= 90 })
            .WithErrorCode(ErrorCodes.MustBeValidCoordinates)
            .WithMessage("The longitude and latitude must be between -180, 180 and -90, 90 respectively");
    }

    /// <summary>
    /// Validates that the location has valid coordinates
    /// </summary>
    public static IRuleBuilderOptions<T, Geolocation> IsValidGeolocation<T>(
        this IRuleBuilder<T, Geolocation> builder)
    {
        return builder
            .Must(loc => loc is { Longitude: >= -180 and <= 180, Latitude: >= -90 and <= 90 })
            .WithErrorCode(ErrorCodes.MustBeValidCoordinates)
            .WithMessage("The longitude and latitude must be between -180, 180 and -90, 90 respectively");
    }

    /// <summary>
    /// Validates if string has a syntax of a phone number
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidPhoneNumber<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder
            .Matches(@"^\+\d+$")
            .WithErrorCode(ErrorCodes.MustBeValidPhoneNumber)
            .WithMessage("The phone number must start with '+' followed by digits.");
    }

    /// <summary>
    /// Validates that the date is today or in the past
    /// </summary>
    public static IRuleBuilderOptions<T, DateOnly> DateInPast<T>(this IRuleBuilder<T, DateOnly> builder)
    {
        return builder
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode(ErrorCodes.DateMustBeInPast)
            .WithMessage("The date must be today or in the past");
    }

     /// <summary>
    /// Validates that the date is today or in the past, or is null
    /// </summary>
    public static IRuleBuilderOptions<T, DateOnly?> DateInPast<T>(this IRuleBuilder<T, DateOnly?> builder)
    {
        return builder
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode(ErrorCodes.DateMustBeInPast)
            .WithMessage("The date must be today or in the past, or can be null");
    }

    /// <summary>
    /// Validates that the string is a valid locale identifier
    /// </summary>
    public static IRuleBuilderOptions<T, string> CultureInfoString<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .Must(locale =>
            {
                try
                {
                    _ = CultureInfo.GetCultureInfo(locale);
                    return true;
                }
                catch (CultureNotFoundException)
                {
                    return false;
                }
            })
            .WithErrorCode(ErrorCodes.MustBeLocaleId)
            .WithMessage("Must be a valid locale identifier (e.g. pl, en_GB)");
    }
}
