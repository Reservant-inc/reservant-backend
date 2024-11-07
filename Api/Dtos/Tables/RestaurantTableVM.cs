using Reservant.Api.Dtos.Visits;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Tables;

/// <summary>
/// Information about a table in a restaurant
/// </summary>
public class RestaurantTableVM
{
    /// <summary>
    /// ID within the restaurant
    /// </summary>
    public required int TableId { get; init; }

    /// <summary>
    /// Capacity
    /// </summary>
    public required int Capacity { get; init; }

    /// <summary>
    /// Current visit id, if table is taken
    /// </summary>
    public int? VisitId { get; set; }

}
