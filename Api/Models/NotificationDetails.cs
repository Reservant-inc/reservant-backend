namespace Reservant.Api.Models;

/// <summary>
/// Extra details about a notification
/// </summary>
public abstract record NotificationDetails;

/// <summary>
/// Details for a restaurant verification notification
/// </summary>
public record NotificationRestaurantVerified(int RestaurantId) : NotificationDetails;
