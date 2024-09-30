using System.Text;
using FirebaseAdmin.Messaging;
using System.Text.Json;
using FirebaseAdmin;
using Reservant.Api.Data;
using Reservant.Api.Models;

namespace Reservant.Api.Push;

/// <summary>
/// Service responsible for interacting with Firebase
/// </summary>
public class FirebaseService(ApiDbContext context)
{
    /// <summary>
    /// Send a push notification to a specific user
    /// </summary>
    public async Task SendNotification(Models.Notification notification)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            return;
        }

        var targetUser = await context.FindAsync<User>(notification.TargetUserId)
            ?? throw new InvalidOperationException("Target user not found");

        var notificationType = notification.Details.GetType().Name;

        await FirebaseMessaging.DefaultInstance.SendAsync(new FirebaseAdmin.Messaging.Message
        {
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = notificationType,
                Body = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes((object)notification.Details)),
            },
            Token = targetUser.FirebaseDeviceToken,
        });
    }
}
