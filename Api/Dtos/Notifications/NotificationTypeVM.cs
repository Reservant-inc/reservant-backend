namespace Reservant.Api.Dtos.Notifications;

/// <summary>
/// Notification type documentation
/// </summary>
public class NotificationTypeVM
{
    /// <summary>
    /// Value of the "notificationType" filed
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Files in the details object
    /// </summary>
    public required Dictionary<string, string> DetailsFields { get; set; }
}
