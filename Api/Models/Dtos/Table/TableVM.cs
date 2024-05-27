using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Table;

/// <summary>
/// Information about a table in a restaurant
/// </summary>
public class TableVM
{
    /// <summary>
    /// ID within the restaurant
    /// </summary>
    [Required]
    public required int TableId { get; init; }

    /// <summary>
    /// Capacity
    /// </summary>
    [Required, Range(1, 10)]
    public required int Capacity { get; init; }
}
