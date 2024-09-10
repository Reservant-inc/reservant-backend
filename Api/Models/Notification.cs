using Reservant.Api.Models.Enums;
using System.Text.Json;

namespace Reservant.Api.Models;

/// <summary>
/// Notification model
/// </summary>
public class Notification
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int Id { get ; set; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// When the notification was read
    /// </summary>
    public DateTime? DateRead { get; set; }

    /// <summary>
    /// User that has received the notification
    /// </summary>
    public string TargetUserId { get; set; } = null!;

    /// <summary>
    /// Type of the notification
    /// </summary>
    public NotificationType NotificationType {  get; set; }

    /// <summary>
    /// Extra data
    /// </summary>
    public JsonElement Details { get; set; }

    /// <summary>
    /// Navigation property for the user that has received the notification
    /// </summary>
    public User TargetUser { get; set; } = null!;
}
