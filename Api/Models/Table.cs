using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Stolik
/// </summary>
public class Table
{
    /// <summary>
    /// ID of the table's restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Unique ID within the restaurant
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ilość miejsc
    /// </summary>
    [Range(1, 10)]
    public required int Capacity { get; set; }
}
