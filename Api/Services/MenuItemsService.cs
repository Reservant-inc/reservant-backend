using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for creating and finding menu items
    /// </summary>
    public class MenuItemsService(
        ApiDbContext context,
        FileUploadService uploadService,
        ValidationService validationService)
    {
        /// <summary>
        /// Validates and creates given menuItems
        /// </summary>
        /// <param name="user">The current user, must be a restaurant owner</param>
        /// <param name="req">MenuItems to be created</param>
        /// <returns>Validation results or the created menuItems</returns>
        [ErrorCode(nameof(CreateMenuItemRequest.RestaurantId), ErrorCodes.NotFound)]
        [ValidatorErrorCodes<CreateMenuItemRequest>]
        [ValidatorErrorCodes<MenuItem>]
        public async Task<Result<MenuItemVM>> CreateMenuItemsAsync(User user, CreateMenuItemRequest req)
        {
            Restaurant? restaurant;

            restaurant = await context.Restaurants.FindAsync(req.RestaurantId);

            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(req.RestaurantId),
                    ErrorMessage = $"Restaurant with ID {req.RestaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var result = await validationService.ValidateAsync(req, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            var menuItem = new MenuItem()
            {
                Price = req.Price,
                Name = req.Name.Trim(),
                AlternateName = req.AlternateName?.Trim(),
                AlcoholPercentage = req.AlcoholPercentage,
                RestaurantId = req.RestaurantId,
                PhotoFileName = req.Photo
            };


            result = await validationService.ValidateAsync(menuItem, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.MenuItems.AddRangeAsync(menuItem);
            await context.SaveChangesAsync();

            return new MenuItemVM()
            {
                MenuItemId = menuItem.Id,
                Name = menuItem.Name,
                AlternateName = menuItem.AlternateName,
                Price = menuItem.Price,
                AlcoholPercentage = menuItem.AlcoholPercentage,
                Photo = menuItem.PhotoFileName
            };
        }


        /// <summary>
        /// Validates and gets menu item by given id
        /// </summary>
        /// <returns>MenuItem</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<MenuItemVM>> GetMenuItemByIdAsync(User user, int menuItemId)
        {
            var item = await context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == menuItemId);

            if (item == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = $"MenuItem: {menuItemId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            return new MenuItemVM()
            {
                MenuItemId = item.Id,
                Name = item.Name,
                AlternateName = item.AlternateName,
                Price = item.Price,
                AlcoholPercentage = item.AlcoholPercentage,
                Photo = uploadService.GetPathForFileName(item.PhotoFileName)
            };
        }

        /// <summary>
        /// Check if the restaurant with the given ID exists and the given user is its owner
        /// </summary>
        /// <param name="user">User supposed to be the owner</param>
        /// <param name="restaurantId">ID of the restaurant to check</param>
        /// <returns>The bool returned is meaningless, errors are returned using the result</returns>
        public async Task<Result<bool>> ValidateRestaurant(User user, int restaurantId)
        {
            var restaurant = await context.Restaurants
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

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

            return true;
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
        [ValidatorErrorCodes<UpdateMenuItemRequest>]
        [ValidatorErrorCodes<MenuItem>]
        public async Task<Result<MenuItemVM>> PutMenuItemByIdAsync(User user, int id, UpdateMenuItemRequest request)
        {
            var item = await context.MenuItems
                .Include(r => r.Restaurant)
                .Include(r => r.Restaurant.Group)
                .FirstOrDefaultAsync(i => i.Id == id);


            if (item is null)
            {
                return new ValidationFailure
                {
                    ErrorMessage = $"MenuItem: {id} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            //check if menuitem belongs to a restaurant owned by user
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


            result = await validationService.ValidateAsync(item, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return new MenuItemVM()
            {
                MenuItemId = item.Id,
                Name = item.Name,
                AlternateName = item.AlternateName,
                Price = item.Price,
                AlcoholPercentage = item.AlcoholPercentage,
                Photo = item.PhotoFileName
            };
        }

        /// <summary>
        /// service that deletes a menu item
        /// </summary>
        /// <param name="id">id of the menu item</param>
        /// <param name="user">owner of the item</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Item does not belong to the user.")]
        public async Task<Result<bool>> DeleteMenuItemByIdAsync(int id, User user)
        {
            var menuItem = await context.MenuItems.Where(m => m.Id == id)
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

            context.Remove(menuItem);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
