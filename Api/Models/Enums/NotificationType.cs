namespace Reservant.Api.Models.Enums;

/// <summary>
/// Type of a notification
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// A user want to join an event you created
    /// </summary>
    UserInterestedInYourEvent,

    /// <summary>
    /// You have been accepted to an event
    /// </summary>
    AcceptedToEvent,

    /// <summary>
    /// You have been rejected from an event
    /// </summary>
    RejectedFromEvent,

    /// <summary>
    /// A restaurant you created has been verified
    /// </summary>
    RestaurantVerified,

    /// <summary>
    /// A user has left a review about one of your restaurants
    /// </summary>
    NewRestaurantReview,
}
