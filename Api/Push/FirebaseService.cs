using System.Globalization;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Microsoft.Extensions.Localization;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Services;
using SmartFormat;
using Notification = Reservant.Api.Models.Notification;
using FirebaseMessage = FirebaseAdmin.Messaging.Message;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;

namespace Reservant.Api.Push;

/// <summary>
/// Service responsible for interacting with Firebase
/// </summary>
public class FirebaseService(
    ApiDbContext context,
    IStringLocalizer<FirebaseService> localizer,
    FileUploadService uploadService)
{
    /// <summary>
    /// Send a push notification to a specific user
    /// </summary>
    public async Task SendNotification(Notification notification)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            return;
        }

        var targetUser = await context.FindAsync<User>(notification.TargetUserId)
            ?? throw new InvalidOperationException("Target user not found");
        if (targetUser.FirebaseDeviceToken is null)
        {
            return;
        }

        var userCulture = targetUser.Language;

        await FirebaseMessaging.DefaultInstance.SendAsync(new FirebaseMessage
        {
            Notification = ComposeNotification(notification, userCulture),
            Token = targetUser.FirebaseDeviceToken,
        });
    }

    /// <summary>
    /// Compose human-readable notification from a Notification object
    /// </summary>
    /// <param name="notification">The Notification object</param>
    /// <param name="culture">Language of the notification</param>
    private FirebaseNotification ComposeNotification(Notification notification, CultureInfo culture)
    {
        var notificationType = notification.Details.GetType().Name;

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;

        var title = Smart.Format(localizer[$"{notificationType}.Title"], notification.Details);
        var body = Smart.Format(localizer[$"{notificationType}.Details"], notification.Details);

        CultureInfo.CurrentUICulture = originalCulture;

        return new FirebaseNotification
        {
            Title = title,
            Body = body,
            ImageUrl = uploadService.GetUrlForFileName(notification.PhotoFileName)?.ToString(),
        };
    }
}
