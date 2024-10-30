using System.Collections.Concurrent;
using System.Globalization;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Reservant.Api.Data;
using Reservant.Api.Mapping;
using Reservant.Api.Models;
using Reservant.Api.Options;
using SmartFormat;
using Notification = Reservant.Api.Models.Notification;
using FirebaseMessage = FirebaseAdmin.Messaging.Message;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;

namespace Reservant.Api.Push;

/// <summary>
/// Service responsible for interacting with Firebase
/// </summary>
public class FirebaseBackgroundService(
    IServiceScopeFactory scopeFactory,
    IStringLocalizer<FirebaseBackgroundService> localizer,
    UrlService urlService,
    ILogger<FirebaseBackgroundService> logger,
    IOptions<FirebaseOptions> options) : BackgroundService
{
    private readonly ConcurrentQueue<Notification> _notificationQueue = new();
    private const int NotificationPollDelayMs = 1000;

    /// <summary>
    /// Enqueue notification to be sent
    /// </summary>
    /// <param name="notification">The notification to be sent</param>
    public void EnqueueNotification(Notification notification)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            return;
        }

        _notificationQueue.Enqueue(notification);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.CredentialsPath is null)
        {
#pragma warning disable CA1848
            logger.LogWarning(
                "Firebase credentials path is not specified. Firebase push notifications are disabled");
#pragma warning restore CA1848
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = await GoogleCredential.FromFileAsync(options.Value.CredentialsPath, stoppingToken),
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_notificationQueue.TryDequeue(out var notification))
            {
                await Task.Delay(NotificationPollDelayMs, stoppingToken);
                continue;
            }

            try
            {
                await SendNotification(notification);
            }
            catch (Exception ex)
            {
                logger.FirebaseMessagingError(ex, notification.TargetUserId);
            }
        }
    }

    /// <summary>
    /// Send a push notification to a specific user
    /// </summary>
    private async Task SendNotification(Notification notification)
    {
        using var scope = scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        var targetUser = await context.FindAsync<User>(notification.TargetUserId)
            ?? throw new InvalidOperationException("Target user not found");
        if (targetUser.FirebaseDeviceToken is null)
        {
            return;
        }

        var userCulture = targetUser.Language;

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(new FirebaseMessage
            {
                Notification = ComposeNotification(notification, userCulture),
                Token = targetUser.FirebaseDeviceToken,
            });
        }
        catch (FirebaseException ex)
        {
            logger.FirebaseMessagingError(ex, notification.TargetUserId);
        }
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
            ImageUrl = urlService.GetUrlForFileName(notification.PhotoFileName)?.ToString(),
        };
    }
}
