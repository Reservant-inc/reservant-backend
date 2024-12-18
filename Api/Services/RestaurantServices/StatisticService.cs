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
public class StatisticService(
    ApiDbContext context,
    AuthorizationService authorizationService
)
{
    /// <summary>
    /// Function for retrivewing restaurant statistics byrestaurant id from given time period
    /// </summary>
    /// <param name="restaurantId">id of the restaurant</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RestaurantStatsRequest>]
    public async Task<Result<RestaurantStatsVM>> GetStatsByRestaurantIdAsync(int restaurantId, Guid userId, RestaurantStatsRequest request)
    {
        var result = await authorizationService.VerifyOwnerRole(restaurantId, userId);

        if (result.IsError)
        {
            return result.Errors;
        }

        var visits = await context.Visits
            .Include(v => v.Orders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
            .Where(v =>
                v.RestaurantId == restaurantId &&
                (request.dateTill == null || (v.EndTime.HasValue && DateOnly.FromDateTime(v.EndTime.Value) <= request.dateTill)) &&
                (request.dateSince == null || (v.StartTime.HasValue && DateOnly.FromDateTime(v.StartTime.Value) >= request.dateSince)))
            .Select(v => new
            {
                Visit = v,
                ParticipantCount = v.Participants.Count
            })
            .ToListAsync();

        var dailyStats = visits
            .GroupBy(v => DateOnly.FromDateTime(v.Visit.StartTime ?? v.Visit.EndTime ?? throw new InvalidOperationException("Visit must have either StartTime or EndTime.")))
            .Select(g =>
            {
                var revenue = g.SelectMany(v => v.Visit.Orders)
                    .SelectMany(o => o.OrderItems)
                    .Sum(oi => oi.Amount * oi.OneItemPrice);

                var customerCount = g.Sum(v => v.ParticipantCount) + 1;

                var popularItems = g.SelectMany(v => v.Visit.Orders)
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.MenuItem.Name)
                    .OrderByDescending(oi => oi.Sum(item => item.Amount))
                    .Take(request.popularItemMaxCount ?? RestaurantStatsRequest.defaultPopularItemMaxCount)
                    .Select(oi => oi.Key)
                    .ToList();

                return new DayStatsVM
                {
                    StatsReferenceDate = g.Key,
                    Revenue = revenue,
                    CustomerCount = customerCount,
                    PopularItems = string.Join(", ", popularItems)
                };
            })
            .ToList();

        var restaurantStats = new RestaurantStatsVM
        {
            RestaurantId = restaurantId,
            RestaurantStat = dailyStats
        };

        return restaurantStats;
    }


    /// <summary>
    /// Function for retrivewing restaurans of given restaurant by restaurant group id  statistics from given time period
    /// </summary>
    /// <param name="restaurantGroupId">id of the restaurant group</param>
    /// <param name="userId">id of the user</param>
    /// <param name="request">Restaurant statistic</param>
    /// <returns></returns>
    [ValidatorErrorCodes<RestaurantStatsRequest>]
    [ErrorCode(null, ErrorCodes.NotFound, "Restaurant group doesnt exist")]
    public async Task<Result<RestaurantGroupStatsVM>> GetStatsByRestaurantGroupIdAsync(int restaurantGroupId, Guid userId, RestaurantStatsRequest request)
    {
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

        var restaurantStatsList = new List<RestaurantStatsVM>();

        foreach (var restaurant in restaurantGroup.Restaurants)
        {
            var statsResult = await GetStatsByRestaurantIdAsync(restaurant.RestaurantId, userId, request);

            if (statsResult.IsError)
            {
                return statsResult.Errors; 
            }

            restaurantStatsList.Add(statsResult.Value);
        }

        var restaurantGroupStats = new RestaurantGroupStatsVM
        {
            RestaurantGroupId = restaurantGroupId,
            RestaurantGroupStats = restaurantStatsList
        };

        return restaurantGroupStats;
    }
}
