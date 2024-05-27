using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Group of restaurants
/// </summary>
public class RestaurantGroup : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [Required, StringLength(50)]
    public required string Name { get; set; }

    /// <summary>
    /// Owner ID
    /// </summary>
    [Required]
    public required string OwnerId { get; set; }

    /// <summary>
    /// Navigation property for the owner
    /// </summary>
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Navigation collection for the restaurants
    /// </summary>
    public ICollection<Restaurant> Restaurants { get; set; } = null!;

    ///  <inheritdoc />
    public bool IsDeleted { get; set; }
}
