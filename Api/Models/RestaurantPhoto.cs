using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Photo of a restaurant
/// </summary>
public class RestaurantPhoto : ISoftDeletable
{
    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Determines the order of photos within one restaurant
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    [Required, StringLength(50)]
    public required string PhotoFileName { get; set; }

    /// <summary>
    /// Navigation property for the restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// Navigation property for the photo upload
    /// </summary>
    public FileUpload Photo { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
