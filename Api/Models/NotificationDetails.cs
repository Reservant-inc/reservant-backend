namespace Reservant.Api.Models;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Extra details about a notification
/// </summary>
public abstract class NotificationDetails;

/// <summary>
/// Details for a restaurant verification notification
/// </summary>
public class NotificationRestaurantVerified : NotificationDetails
{
    /// <summary>
    /// ID of the restaurants
    /// </summary>
    public required int RestaurantId { get; init; }

    /// <summary>
    /// Name of the restaurant
    /// </summary>
    public required string RestaurantName { get; init; }
}

/// <summary>
/// Details for a new restaurant review notification
/// </summary>
public class NotificationNewRestaurantReview : NotificationDetails
{
    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; init; }

    /// <summary>
    /// Name of the restaurant
    /// </summary>
    public required string RestaurantName { get; init; }

    /// <summary>
    /// ID of the new review
    /// </summary>
    public required int ReviewId { get; init; }

    /// <summary>
    /// How many stars in the new review
    /// </summary>
    public required int Stars { get; init; }

    /// <summary>
    /// Contents of the new review
    /// </summary>
    public required string? Contents { get; init; }

    /// <summary>
    /// ID of the review author
    /// </summary>
    public required string AuthorId { get; init; }

    /// <summary>
    /// Name of the review author
    /// </summary>
    public required string AuthorName { get; init; }
}


/// <summary>
/// Details for a new friend request notification
/// </summary>
public class NotificationNewFriendRequest : NotificationDetails
{
    /// <summary>
    /// ID of the sender
    /// </summary>
    [StringLength(36)]
    public required string SenderId { get; set; } = null!;

    /// <summary>
    /// Name of the sender
    /// </summary>
    public required String SenderName { get; set; }
}