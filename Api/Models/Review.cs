using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Restaurant review
/// </summary>
public class Review : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int ReviewId { get; set; }

    /// <summary>
    /// Restaurant ID
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// ID of the user who created the review
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Number of stars (1-5)
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// When the review was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional comment
    /// </summary>
    [StringLength(200)]
    public string? Contents { get; set; }

    /// <summary>
    /// When the restaurant responded to the review
    /// </summary>
    public DateTime? AnsweredAt { get; set; }

    /// <summary>
    /// Restaurant's response
    /// </summary>
    [StringLength(200)]
    public string? RestaurantResponse { get; set; }

    /// <summary>
    /// Navigational property for the restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// Navigational property for the user who created the review
    /// </summary>
    public User Author { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
