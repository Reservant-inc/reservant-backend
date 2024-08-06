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
    public async Task<Result<IngredientVM>> CreateIngredientAsync(CreateIngredientRequest request, string userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }
        var MenuItemIDs = new List<int>();
        foreach (var item in request.MenuItems)
        {
            MenuItemIDs.Add(item.MenuItemId);
        }
        var restaurants = await dbContext.MenuItems.Where(m => MenuItemIDs.Contains(m.Id)).ToListAsync();
        if (restaurants.Count == 0)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.MenuItems),
                ErrorMessage = ErrorCodes.NotFound,
                ErrorCode = ErrorCodes.NotFound
            };
        }
        if (restaurants.Count > 1)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.MenuItems),
                ErrorCode = ErrorCodes.MaximumOneRestaurant,
                ErrorMessage = ErrorCodes.MaximumOneRestaurant
            };
        }

        var ingredient = new Ingredient
        {
            PublicName = request.PublicName,
            UnitOfMeasurement = request.UnitOfMeasurement,
            MinimalAmount = request.MinimalAmount,
            AmountToOrder = request.AmountToOrder,
        };

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
        };
    }
}