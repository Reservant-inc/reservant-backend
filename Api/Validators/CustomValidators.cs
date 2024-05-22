using System.Numerics;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using System.Reflection.Metadata.Ecma335;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;

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
            .MustAsync(async (_, value, context, _) =>
            {

                if (value.Item1 || value.Item2)
                {
                    return true;
                }
                return false;
            })
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
    /// Validates that the orderItem exists in the database.
    /// </summary>
    public static IRuleBuilderOptions<T, CreateOrderItemRequest> OrderItemExist<T>(
        this IRuleBuilder<T, CreateOrderItemRequest> builder, ApiDbContext context)
    {
        return builder
            .MustAsync(async (item, cancellationToken) =>
            {
                var itemExists = await context.MenuItems
                    .AnyAsync(m => m.Id == item.MenuItemId, cancellationToken);

                return itemExists;
            })
            .WithErrorCode(ErrorCodes.NotFound)
            .WithMessage("The order item with MenuItemId {PropertyValue} does not exist in the database.");
    }
    
    /// <summary>
    /// Validates that the order item belongs to the restaurant.
    /// </summary>
    public static IRuleBuilderOptions<T, CreateOrderRequest> OrderItemBelongsToRestaurant<T>(
        this IRuleBuilder<T, CreateOrderRequest> builder, ApiDbContext context)
    {
        return builder
            .MustAsync(async (request, cancellationToken) =>
            {

                var visit = await context.Visits
                    .FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken);
                
                if (visit == null)
                {
                    return false;
                }
                
                var restaurant = await context.Restaurants
                    .Include(r => r.MenuItems)
                    .FirstOrDefaultAsync(r => r.Id == visit.Id, cancellationToken);

                if (restaurant == null)
                {
                    return false;
                }

                foreach (var item in request.Items)
                {
                    if (!restaurant.MenuItems.Any(mi => mi.Id == item.MenuItemId))
                    {
                        return false;
                    }
                }

                return true;
            })
            .WithErrorCode(ErrorCodes.ItemsNotInRestaurant)
            .WithMessage("One or more order items do not belong to the specified restaurant.");
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
                    .AnyAsync(v => v.Id == visitId, cancellationToken);

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
}
