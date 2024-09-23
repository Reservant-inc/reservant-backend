using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Table;

/// <summary>
/// Request to create a table in a restaurant
/// </summary>
public class CreateTableRequest
{
    /// <summary>
    /// Table's capacity
    /// </summary>
    public required int Capacity { get; init; }
}
