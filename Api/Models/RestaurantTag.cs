using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Restaurant tag
/// </summary>
public class RestaurantTag
{
    /// <summary>
    /// Unique string ID
    /// </summary>
    [Key, StringLength(20)]
    public required string Name { get; set; }
}
