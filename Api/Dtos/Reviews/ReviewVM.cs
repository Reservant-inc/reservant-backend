namespace Reservant.Api.Dtos.Reviews;

/// <summary>
/// Information about a review
/// </summary>
public class ReviewVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int ReviewId { get; set; }

    /// <summary>
    /// Restaurant ID
    /// </summary>
    public required int RestaurantId { get; set; }

    /// <summary>
    /// ID of the user who created the review
    /// </summary>
    public required Guid AuthorId { get; set; }

    /// <summary>
    /// Navigational property for the user who created the review
    /// </summary>
    public required string AuthorFullName { get; set; }

    /// <summary>
    /// Number of stars (1-5)
    /// </summary>
    public required int Stars { get; set; }

    /// <summary>
    /// When the review was created
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the review was edited
    /// </summary>
    public DateTime? DateEdited { get; set; }

    /// <summary>
    /// Optional comment
    /// </summary>
    public required string? Contents { get; set; }

    /// <summary>
    /// When the restaurant responded to the review
    /// </summary>
    public required DateTime? AnsweredAt { get; set; }

    /// <summary>
    /// Restaurant's response
    /// </summary>
    public required string? RestaurantResponse { get; set; }
}
