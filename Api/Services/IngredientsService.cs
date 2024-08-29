using ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Ingredient;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing ingredients.
/// </summary>
public class IngredientService(
    ApiDbContext dbContext,
    ValidationService validationService,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Creates a new ingredient.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId">ID of the creator user</param>
    /// <returns></returns>
    [ValidatorErrorCodes<CreateIngredientRequest>]
    [ValidatorErrorCodes<Ingredient>]
    [ErrorCode(nameof(request.MenuItem), ErrorCodes.NotFound)]
    [ErrorCode(nameof(request.MenuItem), ErrorCodes.AccessDenied)]
    public async Task<Result<IngredientVM>> CreateIngredientAsync(CreateIngredientRequest request, string userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }
        var menuItem = await dbContext.MenuItems.Include(m => m.Ingredients).Where(m => m.Id == request.MenuItem.MenuItemId).FirstOrDefaultAsync();
        if (menuItem is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.MenuItem),
                ErrorMessage = ErrorCodes.NotFound,
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var restaurant = await dbContext.Restaurants.Where(r => r.Group.OwnerId == userId && r.MenuItems.Contains(menuItem)).AnyAsync();

        if (!restaurant)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.MenuItem),
                ErrorMessage = ErrorCodes.AccessDenied,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var ingredient = new Ingredient
        {
            PublicName = request.PublicName,
            UnitOfMeasurement = request.UnitOfMeasurement,
            MinimalAmount = request.MinimalAmount,
            AmountToOrder = request.AmountToOrder,
            Amount = request.Amount
        };

        var ingredientMenuItem = new IngredientMenuItem
        {
            MenuItemId = menuItem.Id,
            IngredientId = ingredient.Id,
            AmountUsed = ingredient.MinimalAmount,
            MenuItem = menuItem,
            Ingredient = ingredient
        };

        menuItem.Ingredients.Add(ingredientMenuItem);

        var validationResult = await validationService.ValidateAsync(ingredient, userId);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();

        return new IngredientVM
        {
            IngredientId = ingredient.Id,
            PublicName = ingredient.PublicName,
            UnitOfMeasurement = ingredient.UnitOfMeasurement,
            MinimalAmount = ingredient.MinimalAmount,
            AmountToOrder = ingredient.AmountToOrder,
            Amount = ingredient.Amount
        };
    }


    /// <summary>
    /// Update an ingredient.
    /// </summary>
    /// <param name="ingredientId"></param> 
    /// <param name="request"></param>
    /// <param name="userId">ID of the creator user</param>
    /// <returns></returns>
    [ValidatorErrorCodes<UpdateIngredientRequest>]
    [ValidatorErrorCodes<Ingredient>]
    [ErrorCode(nameof(ingredientId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(ingredientId), ErrorCodes.AccessDenied)]
    public async Task<Result<IngredientVM>> UpdateIngredientAsync(int ingredientId, UpdateIngredientRequest request, String userId)
    {
        var dtoValidationResult = await validationService.ValidateAsync(request, userId);
        if (!dtoValidationResult.IsValid)
        {
            return dtoValidationResult;
        }

        var ingredient = await dbContext.Ingredients
            .Include(i => i.MenuItems)
                .ThenInclude(mi => mi.MenuItem)
            .FirstOrDefaultAsync(i => i.Id == ingredientId);

        if (ingredient == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(ingredientId),
                ErrorMessage = ErrorCodes.NotFound,
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var menuItem = ingredient.MenuItems.FirstOrDefault()?.MenuItem;

        if (menuItem == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(ingredientId),
                ErrorMessage = ErrorCodes.AccessDenied,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var access = await authorizationService
                .VerifyRestaurantBackdoorAccess(menuItem.RestaurantId,userId );
        if (access.IsError)
        {
            return access.Errors;
        }

        ingredient.PublicName = request.PublicName;
        ingredient.UnitOfMeasurement = request.UnitOfMeasurement;
        ingredient.MinimalAmount = request.MinimalAmount;
        ingredient.AmountToOrder = request.AmountToOrder;

        var validationResult = await validationService.ValidateAsync(ingredient, userId);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        await dbContext.SaveChangesAsync();

        return new IngredientVM
        {
            IngredientId = ingredient.Id,
            PublicName = ingredient.PublicName,
            UnitOfMeasurement = ingredient.UnitOfMeasurement,
            MinimalAmount = ingredient.MinimalAmount,
            AmountToOrder = ingredient.AmountToOrder,
            Amount = ingredient.Amount
        };
    }

}