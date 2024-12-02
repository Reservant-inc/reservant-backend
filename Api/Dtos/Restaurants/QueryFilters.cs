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
    public static IQueryable<Restaurant> OnlyActiveRestaurants(this IQueryable<Restaurant> restaurants, bool allowUnverifed=false, bool allowArchived=false)
    {
        return restaurants.Where(restaurant => allowArchived?true:restaurant.IsArchived && allowUnverifed?true:restaurant.VerifierId != null);
    }
}
