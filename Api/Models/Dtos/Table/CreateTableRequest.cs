using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Table;

/// <summary>
/// Request to create a table in a restaurant
/// </summary>
public class CreateTableRequest
{
    /// <summary>
    /// Table's capacity
    /// </summary>
    [Required, Range(1, 10)]
    public required int Capacity { get; init; }
}
