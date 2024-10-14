using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Tables;

/// <summary>
/// Information about a table in a restaurant
/// </summary>
[AutoMap(typeof(Table))]
public class TableVM
{
    /// <summary>
    /// ID within the restaurant
    /// </summary>
    public required int TableId { get; init; }

    /// <summary>
    /// Capacity
    /// </summary>
    public required int Capacity { get; init; }
}
