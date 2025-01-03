using Reservant.Api.Dtos.MenuItems;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// One revenue data point
/// </summary>
public record PopularItem(MenuItemVM MenuItem, double AmountOrdered);

/// <summary>
/// One revenue data point
/// </summary>
public record DayRevenue(DateOnly Date, decimal Revenue);

/// <summary>
/// One customers data point
/// </summary>
public record DayCustomers(DateOnly Date, int Customers);

/// <summary>
/// Statistic of restaurants in a given day
/// </summary>
public class RestaurantStatsVM
{
    /// <summary>
    /// The most ordered menu items
    /// </summary>
    public required List<PopularItem> PopularItems { get; init; }

    /// <summary>
    /// Total number of customers on each day
    /// </summary>
    public required List<DayCustomers> CustomerCount { get; init; }

    /// <summary>
    /// Amount of money made on each day
    /// </summary>
    public required List<DayRevenue> Revenue { get; init; }

}
