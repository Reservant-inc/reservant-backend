using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Dtos.Restaurants;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for creating and finding menu items
    /// </summary>
    public class MenuItemsService(
        ApiDbContext context,
        ValidationService validationService,
        AuthorizationService authorizationService,
        IMapper mapper)
    {
        /// <summary>
        /// Validates and creates given menuItems
        /// </summary>
        /// <param name="userId">The Id of current user, must be a restaurant owner</param>
        /// <param name="req">MenuItems to be created</param>
        /// <returns>Validation results or the created menuItems</returns>
        [ErrorCode(nameof(CreateMenuItemRequest.RestaurantId), ErrorCodes.NotFound)]
        [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
        [ValidatorErrorCodes<CreateMenuItemRequest>]
        [ValidatorErrorCodes<MenuItem>]
        public async Task<Result<MenuItemVM>> CreateMenuItemsAsync(Guid userId, CreateMenuItemRequest req)
        {
            var restaurant = await context.Restaurants
                .OnlyActiveRestaurants()
                .FirstOrDefaultAsync(r => r.RestaurantId == req.RestaurantId);

            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(req.RestaurantId),
                    ErrorMessage = $"Restaurant with ID {req.RestaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var authResult = await authorizationService.VerifyOwnerRole(req.RestaurantId, userId);
            if (authResult.IsError)
            {
                return authResult.Errors;
            }

            var result = await validationService.ValidateAsync(req, userId);
            if (!result.IsValid)
            {
                return result;
            }

            var ingredients = await context.Ingredients
                .Where(i => req.Ingredients.Select(ir => ir.IngredientId).Contains(i.IngredientId))
                .ToListAsync();

            if (ingredients.Count != req.Ingredients.Count)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(req.Ingredients),
                    ErrorMessage = "One or more ingredients were not found in the database",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var ingredientMenuItems = req.Ingredients.Select(i => new IngredientMenuItem
            {
                IngredientId = i.IngredientId,
                AmountUsed = i.AmountUsed,
                Ingredient = ingredients.First(ing => ing.IngredientId == i.IngredientId)
            }).ToList();

            var menuItem = new MenuItem()
            {
                Price = req.Price,
                Name = req.Name.Trim(),
                AlternateName = req.AlternateName?.Trim(),
                AlcoholPercentage = req.AlcoholPercentage,
                RestaurantId = req.RestaurantId,
                PhotoFileName = req.Photo,
                Ingredients = ingredientMenuItems
            };

            result = await validationService.ValidateAsync(menuItem, userId);
            if (!result.IsValid)
            {
                return result;
            }

            await context.MenuItems.AddRangeAsync(menuItem);
            await context.SaveChangesAsync();

            return mapper.Map<MenuItemVM>(menuItem);
        }


        /// <summary>
        /// Validates and gets menu item by given id
        /// </summary>
        /// <returns>MenuItem</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<MenuItemVM>> GetMenuItemByIdAsync(User user, int menuItemId)
        {
            var item = await context.MenuItems
                .Include(i => i.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(i => i.MenuItemId == menuItemId);

            if (item == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"MenuItem: {menuItemId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return mapper.Map<MenuItemVM>(item);
        }


        /// <summary>
        /// Check if the restaurant with the given ID exists and the given user is its owner
        /// </summary>
        /// <param name="user">User supposed to be the owner</param>
        /// <param name="restaurantId">ID of the restaurant to check</param>
        public async Task<Result> ValidateRestaurant(User user, int restaurantId)
        {
            var restaurant = await context.Restaurants
                .OnlyActiveRestaurants()
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant: {restaurantId} not found.",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (restaurant.Group.OwnerId != user.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"Restaurant: {restaurantId} doesn't belong to the restautantOwner.",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            return Result.Success;
        }

        /// <summary>
        /// changes the given menuitem
        /// </summary>
        /// <param name="user">current user, must be restaurantowner</param>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "MenuItem doesn't belong to a restaurant owned by the user")]
        [ErrorCode(null, ErrorCodes.NotFound, "One or more ingredients were not found in the current restaurant")]
        [ValidatorErrorCodes<MenuItem>]
        public async Task<Result<MenuItemVM>> PutMenuItemByIdAsync(User user, int id, UpdateMenuItemRequest request)
        {
            var item = await context.MenuItems
                .Include(r => r.Restaurant)
                .Include(r => r.Restaurant.Group)
                .Include(i => i.Ingredients)
                .FirstOrDefaultAsync(i => i.MenuItemId == id);

            if (item is null)
            {
                return new ValidationFailure
                {
                    ErrorMessage = $"MenuItem: {id} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (item.Restaurant.Group.OwnerId != user.Id)
            {
                return new ValidationFailure
                {
                    ErrorMessage = $"MenuItem: {id} doesn't belong to a restaurant owned by the user",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var result = await validationService.ValidateAsync(request, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            item.Price = request.Price;
            item.Name = request.Name.Trim();
            item.AlternateName = request.AlternateName?.Trim();
            item.AlcoholPercentage = request.AlcoholPercentage;
            item.PhotoFileName = request.Photo;

            var restaurantId = item.RestaurantId;
            var requestedIngredientIds = request.Ingredients.Select(ir => ir.IngredientId).ToArray();
            var ingredients = await context.Ingredients
                .Where(ingredient =>
                    ingredient.MenuItems
                        .Select(imi => imi.MenuItem.RestaurantId)
                        .FirstOrDefault() == restaurantId
                    && requestedIngredientIds.Contains(ingredient.IngredientId))
                .ToListAsync();

            if (ingredients.Count != request.Ingredients.Count)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.Ingredients),
                    ErrorMessage = "One or more ingredients were not found in the current restaurant",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var oldIngredients = item.Ingredients;
            item.Ingredients = new List<IngredientMenuItem>();
            foreach (var reqIngredient in request.Ingredients)
            {
                var ingredient =
                    oldIngredients.FirstOrDefault(i => i.IngredientId == reqIngredient.IngredientId)
                    ?? new IngredientMenuItem
                    {
                        IngredientId = reqIngredient.IngredientId
                    };

                ingredient.AmountUsed = reqIngredient.AmountUsed;
                item.Ingredients.Add(ingredient);
            }

            result = await validationService.ValidateAsync(item, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return mapper.Map<MenuItemVM>(item);
        }


        /// <summary>
        /// service that deletes a menu item
        /// </summary>
        /// <param name="id">id of the menu item</param>
        /// <param name="user">owner of the item</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Item does not belong to the user.")]
        public async Task<Result> DeleteMenuItemByIdAsync(int id, User user)
        {
            var menuItem = await context.MenuItems.Where(m => m.MenuItemId == id)
                .Include(item => item.Restaurant)
                .ThenInclude(restaurant => restaurant.Group)
                .FirstOrDefaultAsync();

            if (menuItem == null)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "No item found."
                };
            }

            if (menuItem.Restaurant.Group.OwnerId != user.Id)
            {
                return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.AccessDenied,
                    ErrorMessage = "Item does not belong to the user."
                };
            }

            menuItem.IsDeleted = true;
            await context.SaveChangesAsync();
            return Result.Success;
        }
    }
}
