using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

public class RestaurantGroup
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
    [Required, StringLength(36)]
    public required string OwnerId { get; set; }

    /// <summary>
    /// Navigation property for the owner
    /// </summary>
    public User? Owner { get; set; }

    /// <summary>
    /// Navigation collection for the restaurants
    /// </summary>
    public ICollection<Restaurant>? Restaurants { get; set; }

    /// <summary>
    /// Property for handling soft deletes
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
