using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for creating and finding menu items
    /// </summary>
    /// <param name="context"></param>
    public class MenuItemsService(ApiDbContext context, FileUploadService uploadService)
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
            
            var photo = await uploadService.ProcessUploadNameAsync(
                req.PhotoFileName,
                user.Id,
                FileClass.Image,
                nameof(req.PhotoFileName));
            if (photo.IsError)
            {
                return photo.Errors;
            }

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
                PhotoFileName = req.PhotoFileName
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
                Photo = photo.Value
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
                Id= item.Id,
                Name = item.Name,
                Price = item.Price,
                AlcoholPercentage = item.AlcoholPercentage,
                Photo = uploadService.GetPathForFileName(item.PhotoFileName)
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
            
            var photo = await uploadService.ProcessUploadNameAsync(
                request.PhotoFileName,
                user.Id,
                FileClass.Image,
                nameof(request.PhotoFileName));
            if (photo.IsError)
            {
                return photo.Errors;
            }

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
            item.PhotoFileName = request.PhotoFileName;

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
                Photo = photo.Value,
            };
            
        }
    }
}
