using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Reusable parts of queries
/// </summary>
public static class QueryFilters
{
    /// <summary>
    /// Filter out not verified and archived restaurants
    /// </summary>
    public static IQueryable<Restaurant> OnlyActiveRestaurants(this IQueryable<Restaurant> restaurants)
    {
        return restaurants.Where(restaurant => !restaurant.IsArchived && restaurant.VerifierId != null);
    }

    /// <summary>
    /// Filter out verified and archived restaurants
    /// </summary>
    public static IQueryable<Restaurant> OnlyUnverifiedRestaurants(this IQueryable<Restaurant> restaurants)
    {
        return restaurants.Where(restaurant => !restaurant.IsArchived && restaurant.VerifierId == null);
    }
}
