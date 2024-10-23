namespace Reservant.Api.Models;

using Dtos.Users;
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
    public required Guid AuthorId { get; init; }

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
    /// ID of the friend request
    /// </summary>
    public required int FriendRequestId { get; set; }

    /// <summary>
    /// ID of the sender
    /// </summary>
    public required Guid SenderId { get; set; }

    /// <summary>
    /// Name of the sender
    /// </summary>
    public required String SenderName { get; set; }
}

/// <summary>
/// Details for a notification when users friend request was accepted
/// </summary>
public class NotificationFriendRequestAccepted : NotificationDetails
{
    /// <summary>
    /// ID of the friend request
    /// </summary>
    public required int FriendRequestId { get; init; }

    /// <summary>
    /// ID of the person that accepted the friend request
    /// </summary>
    public required Guid AcceptingUserId { get; init; }

    /// <summary>
    /// FullName of the user that accepted the friend request
    /// </summary>
    public required string AcceptingUserFullName { get; init; }
}


/// <summary>
/// Details for a new participation request notification
/// </summary>
public class NotificationNewParticipationRequest : NotificationDetails
{
    /// <summary>
    /// ID of the sender
    /// </summary>
    public required Guid SenderId { get; set; }

    /// <summary>
    /// Name of the sender
    /// </summary>
    public required string SenderName { get; set; }

    /// <summary>
    /// Event ID
    /// </summary>
    public required int EventId { get; set; }

    /// <summary>
    /// Event name
    /// </summary>
    public required string EventName { get; set; }
}

/// <summary>
/// Details for a new participation requests response notification
/// </summary>
public class NotificationParticipationRequestResponse : NotificationDetails
{
    /// <summary>
    /// Event ID
    /// </summary>
    public required int EventId { get; set; }

    /// <summary>
    /// name of the event
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// ID of the creator of the event
    /// </summary>
    public required Guid CreatorId { get; set; }

    /// <summary>
    /// Event creator's name
    /// </summary>
    public required string CreatorName { get; set; }

    /// <summary>
    /// Bool determining if its accepted
    /// </summary>
    public required bool IsAccepted { get; set; }
}

/// <summary>
/// Details for a new participation requests response notification
/// </summary>
public class NotificationVisitApprovedDeclined  : NotificationDetails
{
    /// <summary>
    /// Visit ID
    /// </summary>
    public required int VisitId { get; set; }

    /// <summary>
    /// Bool determining if its accepted
    /// </summary>
    public required bool IsAccepted { get; set; }


    /// <summary>
    /// Name of the restaurant in which visit takes place
    /// </summary>
    public required string RestaurantName { get; set; }

    /// <summary>
    /// Time of the visit
    /// </summary>
    public required DateTime Date { get; set; }
}
