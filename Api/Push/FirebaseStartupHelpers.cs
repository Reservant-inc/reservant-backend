using System.Diagnostics.CodeAnalysis;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Reservant.Api.Options;

namespace Reservant.Api.Push;

/// <summary>
/// Helper methods used at startup
/// </summary>
public static class FirebaseStartupHelpers
{
    /// <summary>
    /// Try to initialize Firebase SDK. If the credentials path is not provided,
    /// log a warning and do nothing.
    /// </summary>
    [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates",
        Justification = "This method is called just once at startup")]
    public static async Task TryInitFirebase(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<FirebaseOptions>>();
        if (options.Value.CredentialsPath is null)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<FirebaseService>>();
            logger.LogWarning(
                "Firebase credentials path is not specified. Firebase push notifications are disabled");
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = await GoogleCredential.FromFileAsync(options.Value.CredentialsPath, CancellationToken.None),
        });
    }
}
