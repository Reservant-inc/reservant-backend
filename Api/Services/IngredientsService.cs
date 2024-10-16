using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Ingredients;
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
    AuthorizationService authorizationService,
    FileUploadService fileUploadService)
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
    public async Task<Result<IngredientVM>> CreateIngredientAsync(CreateIngredientRequest request, Guid userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }
        var menuItem = await dbContext.MenuItems.Include(m => m.Ingredients).Where(m => m.MenuItemId == request.MenuItem.MenuItemId).FirstOrDefaultAsync();
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
            MenuItemId = menuItem.MenuItemId,
            IngredientId = ingredient.IngredientId,
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
            IngredientId = ingredient.IngredientId,
            PublicName = ingredient.PublicName,
            UnitOfMeasurement = ingredient.UnitOfMeasurement,
            MinimalAmount = ingredient.MinimalAmount,
            AmountToOrder = ingredient.AmountToOrder,
            Amount = ingredient.Amount
        };
    }

    /// <summary>
    /// Gets the change history of an ingredient with optional filters and pagination.
    /// </summary>
    /// <param name="request">Request with parameters</param>
    /// <param name="ingredientId">Ingredient id</param>
    /// <param name="userId">ID of the current user</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>Paginated list of ingredient amount corrections</returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<IngredientAmountCorrectionVM>>> GetIngredientHistoryAsync(
        IngredientHistoryRequest request,
        int ingredientId,
        Guid userId,
        int page, 
        int perPage
)
    {
        // Check if the ingredient exists
        var ingredient = await dbContext.Ingredients
            .Include(i => i.MenuItems)
            .ThenInclude(mi => mi.MenuItem)
            .FirstOrDefaultAsync(i => i.IngredientId == ingredientId);
        
        if (ingredient == null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
                PropertyName = null,
            };
        }
        
        // Check if the user has access to the restaurant
        var restaurantId = ingredient.MenuItems.First().MenuItem.RestaurantId;
        
        var access = await authorizationService.VerifyRestaurantBackdoorAccess(restaurantId, userId);
        if (access.IsError)
        {
            return access.Errors;
        }
        
        var query = dbContext.Entry(ingredient)
            .Collection(i => i.Corrections)
            .Query()
            .Include(c => c.User)
            .AsQueryable();

        if (request.DateFrom.HasValue)
        {
            query = query.Where(c => c.CorrectionDate >= request.DateFrom.Value);
        }

        if (request.DateUntil.HasValue)
        {
            query = query.Where(c => c.CorrectionDate <= request.DateUntil.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrEmpty(request.Comment))
        {
            query = query.Where(c => c.Comment.Contains(request.Comment));
        }
        
        query = query.OrderByDescending(c => c.CorrectionDate);
        
        var mappedQuery = query.Select(c => new IngredientAmountCorrectionVM
        {
            CorrectionId = c.Id,
            Ingredient = new IngredientVM
            {
                Amount = c.NewAmount,
                IngredientId = ingredient.IngredientId,
                PublicName = ingredient.PublicName,
                AmountToOrder = ingredient.AmountToOrder,
                UnitOfMeasurement = ingredient.UnitOfMeasurement,
                MinimalAmount = ingredient.MinimalAmount
            },
            OldAmount = c.OldAmount,
            NewAmount = c.NewAmount,
            CorrectionDate = c.CorrectionDate,
            User = new Dtos.Users.UserSummaryVM
            {
                FirstName = c.User.FirstName,
                LastName = c.User.LastName,
                UserId = c.User.Id,
                Photo = fileUploadService.GetPathForFileName(c.User.PhotoFileName)
            },
            Comment = c.Comment
        });
        
        return await mappedQuery.PaginateAsync(page, perPage, []);
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
    public async Task<Result<IngredientVM>> UpdateIngredientAsync(int ingredientId, UpdateIngredientRequest request, Guid userId)
    {
        var dtoValidationResult = await validationService.ValidateAsync(request, userId);
        if (!dtoValidationResult.IsValid)
        {
            return dtoValidationResult;
        }

        var ingredient = await dbContext.Ingredients
            .Include(i => i.MenuItems)
                .ThenInclude(mi => mi.MenuItem)
            .FirstOrDefaultAsync(i => i.IngredientId == ingredientId);

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
                .VerifyRestaurantBackdoorAccess(menuItem.RestaurantId, userId);
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
            IngredientId = ingredient.IngredientId,
            PublicName = ingredient.PublicName,
            UnitOfMeasurement = ingredient.UnitOfMeasurement,
            MinimalAmount = ingredient.MinimalAmount,
            AmountToOrder = ingredient.AmountToOrder,
            Amount = ingredient.Amount
        };
    }

    /// <summary>
    /// Update ingredient amount
    /// </summary>
    /// <param name="ingredientId">ID of the ingredient to update</param>
    /// <param name="userId">ID of the current user for permission checking</param>
    /// <param name="request">The DTO containing the new data</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    [ValidatorErrorCodes<IngredientAmountCorrectionRequest>]
    public async Task<Result<IngredientAmountCorrectionVM>> CorrectIngredientAmountAsync(
        int ingredientId, Guid userId, IngredientAmountCorrectionRequest request)
    {
        var validationResult = await validationService.ValidateAsync(request, userId);
        if (!validationResult.IsValid)
        {
            return validationResult.Errors;
        }

        var user = await dbContext.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException($"User with ID {userId} not found");

        var ingredient = await dbContext.Ingredients
            .Include(i => i.MenuItems)
            .ThenInclude(m => m.MenuItem)
            .FirstOrDefaultAsync(i => i.IngredientId == ingredientId);

        if (ingredient is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
                PropertyName = null
            };
        }

        var access = await authorizationService
                .VerifyRestaurantBackdoorAccess(ingredient.MenuItems.First().MenuItem.RestaurantId, userId);
        if (access.IsError)
        {
            return access.Errors;
        }

        var correction = new IngredientAmountCorrection
        {
            IngredientId = ingredientId,
            Ingredient = ingredient,
            OldAmount = ingredient.Amount,
            NewAmount = request.NewAmount,
            CorrectionDate = DateTime.UtcNow,
            UserId = userId,
            User = user,
            Comment = request.Comment
        };

        dbContext.Add(correction);

        ingredient.Amount = correction.NewAmount;

        await dbContext.SaveChangesAsync();

        return new IngredientAmountCorrectionVM
        {
            CorrectionId = correction.Id,
            Ingredient = new IngredientVM
            {
                Amount = ingredient.Amount,
                IngredientId = ingredient.IngredientId,
                PublicName = ingredient.PublicName,
                AmountToOrder = ingredient.AmountToOrder,
                UnitOfMeasurement = ingredient.UnitOfMeasurement,
                MinimalAmount = ingredient.MinimalAmount
            },
            OldAmount = correction.OldAmount,
            NewAmount = correction.NewAmount,
            CorrectionDate = correction.CorrectionDate,
            User = new Dtos.Users.UserSummaryVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserId = user.Id,
                Photo = fileUploadService.GetPathForFileName(user.PhotoFileName)
            },
            Comment = correction.Comment
        };
    }
}
