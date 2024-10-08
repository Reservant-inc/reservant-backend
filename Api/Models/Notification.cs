using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// File name of a picture related to the notification
    /// </summary>
    [StringLength(50)]
    public string? PhotoFileName { get; set; }

    /// <summary>
    /// Type of the details, used for persistence
    /// </summary>
    private Type DetailsKind => Details.GetType();

    /// <summary>
    /// Serialized details, used for persistence
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    private string DetailsJson => JsonSerializer.Serialize(Details, DetailsKind);

    /// <summary>
    /// Extra details
    /// </summary>
    public NotificationDetails Details { get; set; }

    /// <summary>
    /// Navigation property for the user that has received the notification
    /// </summary>
    public User TargetUser { get; set; } = null!;

    /// <summary>
    /// Navigation property for a picture related to the notification
    /// </summary>
    public FileUpload Photo { get; set; } = null!;

    /// <summary>
    /// Constructor for Entity Framework
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private Notification(Type detailsKind, string detailsJson)
    {
        Details = (NotificationDetails)JsonSerializer.Deserialize(detailsJson, detailsKind)!;
    }

    /// <summary>
    /// Constructor for the rest of the app
    /// </summary>
    public Notification(DateTime dateCreated, Guid targetUserId, NotificationDetails details)
    {
        (DateCreated, TargetUserId, Details) = (dateCreated, targetUserId, details);
    }
}
