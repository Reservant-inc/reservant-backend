namespace Reservant.Api.Models.Dtos.Review;

/// <summary>
/// Request to create a Review
/// </summary>
public class CreateReviewRequest
{
    /// <summary>
    /// Number of stars (1-5)
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// Optional comment
    /// </summary>
    public string? Contents { get; set; }
}
