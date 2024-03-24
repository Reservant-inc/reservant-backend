using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Lokal
/// </summary>
public class Restaurant
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
    /// Adres
    /// </summary>
    [Required, StringLength(70)]
    public required string Address { get; set; }

    /// <summary>
    /// Restaurant group ID
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Navigation collection for the tables
    /// </summary>
    public ICollection<Table>? Tables { get; set; }

    /// <summary>
    /// Navigation property for the restaurant group
    /// </summary>
    public RestaurantGroup? Group { get; set; }
}
