using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.RestaurantServices;

/// <summary>
/// Service responsible for managing restaurants statistics
/// </summary>
public class StatisticsService(
    ApiDbContext context
    ,AuthorizationService authorizationService
)
{
    /// <summary>
    /// Function that validates request
    /// </summary>
    /// <param name="request">request</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RestaurantStatsRequest>]
    public Result ValidateRestaurantStatsRequest(RestaurantStatsRequest request)
    {
        if (request.DateUntil < request.DateFrom)
        {
            return new ValidationFailure
            {
                PropertyName = request.DateUntil + " beofre " + request.DateFrom,
                ErrorCode = ErrorCodes.StartMustBeBeforeEnd,
                ErrorMessage = ErrorCodes.InvalidTimeSlot,
            };
        }

        if (request.PopularItemMaxCount < 0)
        {
            return new ValidationFailure
            {
                PropertyName = request.PopularItemMaxCount + "must be more than 0",
                ErrorCode = ErrorCodes.InvalidState,
                ErrorMessage = ErrorCodes.InvalidState,
            };
        }

        if (request.PopularItemMaxCount % 1 != 0)
        {
            return new ValidationFailure
            {
                PropertyName = request.PopularItemMaxCount + "must be a whole number",
                ErrorCode = ErrorCodes.InvalidState,
                ErrorMessage = ErrorCodes.InvalidState,
            };
        }

        return Result.Success;
    }



    /// <summary>
    /// Function for retrieving restaurant statistics by restaurant id from given time period
    /// </summary>
    /// <param name="restaurantId">id of the restaurant</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RestaurantStatsRequest>]
    public async Task<Result<RestaurantStatsVM>> GetStatsByRestaurantIdAsync(int restaurantId, Guid userId, RestaurantStatsRequest request)
    {
        var result = ValidateRestaurantStatsRequest(request);
        if(result.IsError)
        {
            return result.Errors;
        }

        var authorizationResult = await authorizationService.VerifyOwnerRole(restaurantId, userId);

        if (authorizationResult.IsError)
        {
            return authorizationResult.Errors;
        }

        var visits = await context.Visits
            .Include(v => v.Orders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
            .Where(v =>
                v.RestaurantId == restaurantId &&
                (request.DateUntil == null || (v.EndTime.HasValue && DateOnly.FromDateTime(v.EndTime.Value) <= request.DateUntil)) &&
                (request.DateFrom == null || (v.StartTime.HasValue && DateOnly.FromDateTime(v.StartTime.Value) >= request.DateFrom)))
            .ToListAsync();

        var groupedData = visits
            .GroupBy(v => DateOnly.FromDateTime(v.StartTime ?? v.EndTime ?? throw new InvalidOperationException("Visit must have either StartTime or EndTime.")))
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.SelectMany(v => v.Orders ?? Enumerable.Empty<Order>())
                    .SelectMany(o => o.OrderItems ?? Enumerable.Empty<OrderItem>())
                    .Where(oi => oi.MenuItem != null)
                    .Sum(oi => oi.Amount * oi.OneItemPrice),
                CustomerCount = g.Sum(v => v.Participants?.Count ?? 0) + 1,
                PopularItems = g.SelectMany(v => v.Orders ?? Enumerable.Empty<Order>())
                    .SelectMany(o => o.OrderItems ?? Enumerable.Empty<OrderItem>())
                    .Where(oi => oi.MenuItem != null)
                    .GroupBy(oi => oi.MenuItem!.Name)
                    .OrderByDescending(oi => oi.Sum(item => item.Amount))
                    .Take(request.PopularItemMaxCount ?? RestaurantStatsRequest.DefaultPopularItemMaxCount)
                    .ToDictionary(oi => oi.Key, oi => oi.Sum(item => item.Amount))
            })
            .ToList();

        var restaurantStats = new RestaurantStatsVM
        {
            DateList = groupedData.Select(d => d.Date).ToList(),
            RevenueList = groupedData.Select(d => d.Revenue).ToList(),
            CustomerCountList = groupedData.Select(d => d.CustomerCount).ToList(),
            PopularItems = groupedData
                .SelectMany(d => d.PopularItems)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value))
        };

        return restaurantStats;
    }

    /// <summary>
    /// Function for retrieving restaurants statistics by restaurant group id from given time period
    /// </summary>
    /// <param name="restaurantGroupId">id of the restaurant group</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RestaurantStatsRequest>]
    [ErrorCode(null, ErrorCodes.NotFound, "Restaurant group doesn't exist")]
    public async Task<Result<RestaurantStatsVM>> GetStatsByRestaurantGroupIdAsync(int restaurantGroupId, Guid userId, RestaurantStatsRequest request)
    {
        var result = ValidateRestaurantStatsRequest(request);
        if(result.IsError)
        {
            return result.Errors;
        }

        var restaurantGroup = await context.RestaurantGroups
            .Include(g => g.Restaurants)
            .FirstOrDefaultAsync(g => g.RestaurantGroupId == restaurantGroupId);

        if (restaurantGroup is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
            };
        }

        var firstRestaurant = restaurantGroup.Restaurants.FirstOrDefault();
        if (firstRestaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "No restaurants found in the group.",
            };
        }

        var authorizationResult = await authorizationService.VerifyOwnerRole(firstRestaurant.RestaurantId, userId);
        if (authorizationResult.IsError)
        {
            return authorizationResult.Errors;
        }

        var combinedDateRevenue = new Dictionary<DateOnly, decimal>();
        var combinedDateCustomerCount = new Dictionary<DateOnly, int>();
        var combinedPopularItems = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var restaurant in restaurantGroup.Restaurants)
        {
            var statsResult = await GetStatsByRestaurantIdAsync(restaurant.RestaurantId, userId, request);

            if (statsResult.IsError)
            {
                return statsResult.Errors;
            }

            var restaurantStats = statsResult.Value;

            for (int i = 0; i < restaurantStats.DateList.Count; i++)
            {
                var date = restaurantStats.DateList[i];

                if (!combinedDateRevenue.ContainsKey(date))
                    combinedDateRevenue[date] = 0;
                combinedDateRevenue[date] += restaurantStats.RevenueList[i];

                if (!combinedDateCustomerCount.ContainsKey(date))
                    combinedDateCustomerCount[date] = 0;
                combinedDateCustomerCount[date] += restaurantStats.CustomerCountList[i];
            }

            foreach (var (itemName, count) in restaurantStats.PopularItems)
            {
                if (!combinedPopularItems.ContainsKey(itemName))
                    combinedPopularItems[itemName] = 0;
                combinedPopularItems[itemName] += count;
            }
        }

        var combinedStats = new RestaurantStatsVM
        {
            DateList = combinedDateRevenue.Keys.OrderBy(d => d).ToList(),
            RevenueList = combinedDateRevenue.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList(),
            CustomerCountList = combinedDateCustomerCount.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList(),
            PopularItems = combinedPopularItems
                .OrderByDescending(kvp => kvp.Value)
                .Take(request.PopularItemMaxCount ?? RestaurantStatsRequest.DefaultPopularItemMaxCount)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        return combinedStats;
    }
}
