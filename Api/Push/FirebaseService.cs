using System.Text;
using FirebaseAdmin.Messaging;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Options;

namespace Reservant.Api.Push;

/// <summary>
/// Service responsible for interacting with Firebase
/// </summary>
public class FirebaseService(ApiDbContext context, IOptions<FirebaseOptions> options)
{
    /// <summary>
    /// Send a push notification to a specific user
    /// </summary>
    public async Task SendNotification(Models.Notification notification)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(options.Value.CredentialsPath),
            });
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
