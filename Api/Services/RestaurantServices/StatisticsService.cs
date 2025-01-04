using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.MenuItems;
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
    ApiDbContext context,
    AuthorizationService authorizationService,
    IMapper mapper)
{
    /// <summary>
    /// Function for retrieving restaurant statistics by restaurant id from given time period
    /// </summary>
    /// <param name="restaurantId">id of the restaurant</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [MethodErrorCodes<StatisticsService>(nameof(ValidateRestaurantStatsRequest))]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
    public async Task<Result<RestaurantStatsVM>> GetStatsByRestaurantIdAsync(
        int restaurantId, Guid userId, RestaurantStatsRequest request)
    {
        var result = ValidateRestaurantStatsRequest(request);
        if (result.IsError)
        {
            return result.Errors;
        }

        var authorizationResult = await authorizationService.VerifyOwnerRole(restaurantId, userId);
        if (authorizationResult.IsError)
        {
            return authorizationResult.Errors;
        }

        return await GetStatisticsForRestaurant(restaurantId, request);
    }

    /// <summary>
    /// Function for retrieving restaurants statistics by restaurant group id from given time period
    /// </summary>
    /// <param name="restaurantGroupId">id of the restaurant group</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [MethodErrorCodes<StatisticsService>(nameof(ValidateRestaurantStatsRequest))]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyOwnerRole))]
    [ErrorCode(null, ErrorCodes.NotFound, "Restaurant group does not exist")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User is not the owner of the group")]
    public async Task<Result<RestaurantStatsVM>> GetStatsByRestaurantGroupIdAsync(int restaurantGroupId, Guid userId, RestaurantStatsRequest request)
    {
        var result = ValidateRestaurantStatsRequest(request);
        if (result.IsError)
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
                ErrorMessage = $"Restaurant group with ID {restaurantGroupId} does not exist",
            };
        }

        if (restaurantGroup.OwnerId != userId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = $"User is not the owner of the restaurant group with ID {restaurantGroupId}",
            };
        }

        return await GetStatisticsForGroup(restaurantGroup, request);
    }

    private static Task<RestaurantStatsVM> GetStatisticsForGroup(
        RestaurantGroup restaurantGroup, RestaurantStatsRequest request)
    {
        var combinedDateRevenue = new Dictionary<DateOnly, decimal>();
        var combinedDateCustomerCount = new Dictionary<DateOnly, int>();
        var combinedPopularItems = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // foreach (var restaurant in restaurantGroup.Restaurants)
        // {
        //     var restaurantStats = await GetStatisticsForRestaurant(restaurant.RestaurantId, request);
        //
        //     for (int i = 0; i < restaurantStats.DateList.Count; i++)
        //     {
        //         var date = restaurantStats.DateList[i];
        //
        //         if (!combinedDateRevenue.ContainsKey(date))
        //             combinedDateRevenue[date] = 0;
        //         combinedDateRevenue[date] += restaurantStats.RevenueList[i];
        //
        //         if (!combinedDateCustomerCount.ContainsKey(date))
        //             combinedDateCustomerCount[date] = 0;
        //         combinedDateCustomerCount[date] += restaurantStats.CustomerCountList[i];
        //     }
        //
        //     foreach (var (itemName, count) in restaurantStats.PopularItems)
        //     {
        //         if (!combinedPopularItems.ContainsKey(itemName))
        //             combinedPopularItems[itemName] = 0;
        //         combinedPopularItems[itemName] += count;
        //     }
        // }

        return null!;
    }

    private async Task<RestaurantStatsVM> GetStatisticsForRestaurant(
        int restaurantId, RestaurantStatsRequest request)
    {
        var visits = context.Visits
            .Where(v => v.RestaurantId == restaurantId && v.StartTime != null);

        if (request.DateFrom is not null)
        {
            var startDateTime = new DateTime(request.DateFrom.Value, default);
            visits = visits.Where(v => v.StartTime >= startDateTime);
        }

        if (request.DateUntil is not null)
        {
            var endDateTime = new DateTime(request.DateUntil.Value, default).AddDays(1);
            visits = visits.Where(v => v.StartTime < endDateTime);
        }

        var visitStatistics = await visits
            .GroupBy(v => v.StartTime!.Value.Date)
            .Select(group => new
            {
                Date = DateOnly.FromDateTime(group.Key),
                Revenue = group
                    .SelectMany(v => v.Orders)
                    .SelectMany(o => o.OrderItems)
                    .Sum(oi => oi.OneItemPrice * oi.Amount),
                CustomerCount =
                    group.SelectMany(v => v.Participants).Count()
                    + group.Sum(v => v.NumberOfGuests + 1),
            })
            .ToListAsync();

        var popularItemIds = visits
            .SelectMany(v => v.Orders)
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.MenuItemId)
            .Select(group => new
            {
                MenuItemId = group.Key,
                AmountOrdered = group.Sum(oi => oi.Amount),
            })
            .OrderByDescending(popularItem => popularItem.AmountOrdered)
            .AsQueryable();

        if (request.PopularItemMaxCount is not null)
        {
            popularItemIds = popularItemIds.Take(request.PopularItemMaxCount.Value);
        }

        var popularItems = await context.MenuItems
            .Where(mi => mi.RestaurantId == restaurantId)
            .ProjectTo<MenuItemVM>(mapper.ConfigurationProvider)
            .Join(
                popularItemIds,
                menuItem => menuItem.MenuItemId,
                orderItem => orderItem.MenuItemId,
                (menuItem, popularItem) => new
                {
                    MenuItem = menuItem,
                    AmountOrdered = popularItem.AmountOrdered,
                })
            .OrderByDescending(popularItem => popularItem.AmountOrdered)
            .ToListAsync();

        return new RestaurantStatsVM
        {
            Revenue = visitStatistics
                .Select(p => new DayRevenue(p.Date, p.Revenue))
                .ToList(),
            CustomerCount = visitStatistics
                .Select(p => new DayCustomers(p.Date, p.CustomerCount))
                .ToList(),
            PopularItems = popularItems
                .Select(p => new PopularItem(p.MenuItem, p.AmountOrdered))
                .ToList(),
        };
    }

    /// <summary>
    /// Function that validates request
    /// </summary>
    /// <param name="request">request</param>
    /// <returns></returns>
    [ErrorCode(nameof(request.DateUntil), ErrorCodes.StartMustBeBeforeEnd,
        "dateFrom must be before dateUntil")]
    [ErrorCode(nameof(request.PopularItemMaxCount), ErrorCodes.InvalidSearchParameters,
        "popularItemMaxCount must be greater than 0")]
    private static Result ValidateRestaurantStatsRequest(RestaurantStatsRequest request)
    {
        if (request.DateUntil < request.DateFrom)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.DateUntil),
                ErrorCode = ErrorCodes.StartMustBeBeforeEnd,
                ErrorMessage = $"{request.DateFrom} must be before {request.DateUntil}",
            };
        }

        if (request.PopularItemMaxCount < 0)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.PopularItemMaxCount),
                ErrorCode = ErrorCodes.InvalidSearchParameters,
                ErrorMessage = $"{request.PopularItemMaxCount} must be greater than 0",
            };
        }

        return Result.Success;
    }
}
