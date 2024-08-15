using ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Ingredient;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing ingredients.
/// </summary>
public class IngredientService(
    UserManager<User> userManager,
    ApiDbContext dbContext,
    ValidationService validationService)
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
}