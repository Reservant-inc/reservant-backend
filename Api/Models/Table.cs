using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

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
    public int Number { get; set; }

    /// <summary>
    /// Ilość miejsc
    /// </summary>
    public required int Capacity { get; set; }

    /// <summary>
    /// Restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;
}
