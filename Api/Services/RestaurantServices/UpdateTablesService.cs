using AutoMapper;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Tables;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.RestaurantServices;

/// <summary>
/// Update the list of tables of a restaurant
/// </summary>
public class UpdateTablesService(
    ApiDbContext context,
    AuthorizationService authorizationService,
    IMapper mapper)
{
    /// <summary>
    /// Update the list of tables of a restaurant
    /// </summary>
    /// <param name="restaurantId">ID of the restaurant to update the tables of</param>
    /// <param name="dto">Info about the new list of tables</param>
    /// <param name="userId">ID of the current user for permission checks</param>
    [ErrorCode(nameof(restaurantId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
    public async Task<Result<MyRestaurantVM>> UpdateTables(int restaurantId, UpdateTablesRequest dto, Guid userId)
    {
        var restaurant = await context.Restaurants
            .Include(restaurant => restaurant.Tables)
            .SingleOrDefaultAsync(restaurant => restaurant.RestaurantId == restaurantId);
        if (restaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(restaurantId),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = $"Restaurant with ID {restaurantId} not found",
            };
        }

        var authorization = await authorizationService.VerifyOwnerRole(restaurantId, userId);
        if (authorization.IsError) return authorization.Errors;

        restaurant.Tables = dto.Tables
            .Select(table => new Table
            {
                Number = table.TableId,
                Capacity = table.Capacity,
            })
            .ToList();
        await context.SaveChangesAsync();

        return mapper.Map<MyRestaurantVM>(restaurant);
    }
}
