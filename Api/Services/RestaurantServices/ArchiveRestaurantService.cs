using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.RestaurantServices;

/// <summary>
/// Service responsible for archiving restaurants
/// </summary>
public class ArchiveRestaurantService(ApiDbContext context)
{
    /// <summary>
    /// Function for soft deleting Restaurants that also deletes newly emptied restaurant groups
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    public async Task<Result> ArchiveRestaurant(int id, User user)
    {
        var restaurant = await context.Restaurants
            .AsSplitQuery()
            .Include(r => r.Group)
            .ThenInclude(g => g.Restaurants)
            .Include(restaurant => restaurant.Tables)
            .Include(restaurant => restaurant.Employments)
            .Include(restaurant => restaurant.Photos)
            .Include(restaurant => restaurant.Menus)
            .Include(restaurant => restaurant.MenuItems)
            .Where(r => r.RestaurantId == id && r.Group.OwnerId == user.Id)
            .FirstOrDefaultAsync();
        if (restaurant == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
            };
        }

        foreach (var table in restaurant.Tables)
        {
            table.IsDeleted = true;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var employment in restaurant.Employments)
        {
            employment.DateUntil = today;
        }

        foreach (var menuItem in restaurant.MenuItems)
        {
            menuItem.IsDeleted = true;
        }

        foreach (var menu in restaurant.Menus)
        {
            menu.IsDeleted = true;
        }

        restaurant.IsArchived = true;

        // We check if the restaurant was the last one (the collection was loaded before we deleted it)
        if (restaurant.Group.Restaurants.Count == 1)
        {
            restaurant.Group.IsDeleted = true;
        }

        await context.SaveChangesAsync();
        return Result.Success;
    }
}
