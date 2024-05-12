using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for creating and finding menu items
    /// </summary>
    /// <param name="context"></param>
    public class MenuItemsService(ApiDbContext context)
    {
        /// <summary>
        /// Validates and creates given menuItems
        /// </summary>
        /// <param name="user">The current user, must be a restaurant owner</param>
        /// <param name="req">MenuItems to be created</param>
        /// <returns>Validation results or the created menuItems</returns>
        public async Task<Result<MenuItemVM>> CreateMenuItemsAsync(User user, CreateMenuItemRequest req)
        {
            var errors = new List<ValidationResult>();

            var isRestaurantValid = await ValidateRestaurant(user, req.RestaurantId);

            if (isRestaurantValid.IsError)
            {
                return isRestaurantValid.Errors;
            }

            var menuItem = new MenuItem()
            {
                Price = req.Price,
                Name = req.Name,
                AlcoholPercentage = req.AlcoholPercentage,
                RestaurantId = req.RestaurantId,
            };



            if (!ValidationUtils.TryValidate(menuItem, errors))
            {
                return errors;
            }


            await context.MenuItems.AddRangeAsync(menuItem);
            await context.SaveChangesAsync();

            return new MenuItemVM()
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                AlcoholPercentage = menuItem.AlcoholPercentage,
            };

        }


        /// <summary>
        /// Validates and gets menu item by given id
        /// </summary>
        /// <param name="user"></param>
        /// <param name="restaurantId"></param>
        /// <param name="menuItemId"></param>
        /// <returns>MenuItem</returns>
        public async Task<Result<MenuItemVM>> GetMenuItemByIdAsync(User user, int menuItemId)
        {
            var errors = new List<ValidationResult>();

            var item = await context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == menuItemId);

            if (item == null)
            {
                errors.Add(new ValidationResult(
                   $"MenuItem: {menuItemId} not found"
                ));
                return errors;
            }

            return new MenuItemVM()
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                AlcoholPercentage = item.AlcoholPercentage,
            };

        }

        public async Task<Result<bool>> ValidateRestaurant(User user, int restaurantId)
        {
            var errors = new List<ValidationResult>();

            var restaurant = await context.Restaurants
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                errors.Add(new ValidationResult(
                    $"Restaurant: {restaurantId} not found."
                ));
                return errors;
            }

            if (restaurant.Group!.OwnerId != user.Id)
            {
                errors.Add(new ValidationResult(
                    $"Restaurant: {restaurantId} doesn't belong to the restautantOwner."
                ));
                return errors;
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
        public async Task<Result<MenuItemVM>> PutMenuItemByIdAsync(User user, int id, UpdateMenuItemRequest request)
        {
            var errors = new List<ValidationResult>();

            var item = await context.MenuItems
                .Include(r => r.Restaurant)
                .Include(r => r.Restaurant!.Group)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                errors.Add(new ValidationResult(
                   $"MenuItem: {id} not found"
                ));
                return errors;
            }

            //check if menuitem belongs to a restaurant owned by user
            if (item.Restaurant!.Group!.OwnerId != user.Id)
            {
                errors.Add(new ValidationResult(
                   $"MenuItem: {id} doesn't belong to a restaurant owned by the user"
                ));
                return errors;
            }

            item.Price = request.Price;
            item.Name = request.Name.Trim();
            item.AlcoholPercentage = request.AlcoholPercentage;

            if (!ValidationUtils.TryValidate(item, errors))
            {
                return errors;
            }

            await context.SaveChangesAsync();

            return new MenuItemVM()
            {
                Id = item.Id,
                Price = item.Price,
                Name = item.Name,
                AlcoholPercentage = item.AlcoholPercentage,
            };

        }
        /// <summary>
        /// service that deletes a menu item
        /// </summary>
        /// <param name="id">id of the menu item</param>
        /// <param name="user">owner of the item</param>
        /// <returns></returns>
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
            if (menuItem.Restaurant.Group.OwnerId != user.Id) {
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
