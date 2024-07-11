using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Restaurant tag
/// </summary>
public class RestaurantTag : ISoftDeletable
{
    /// <summary>
    /// Unique string ID
    /// </summary>
    [Key, StringLength(20)]
    public required string Name { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigational collection for the restaurants with this tag
    /// </summary>
    public ICollection<Restaurant> Restaurants { get; set; } = null!;
}
