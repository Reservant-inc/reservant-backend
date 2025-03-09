namespace Reservant.Api.Configuration;

/// <summary>
/// Firebase configuration
/// </summary>
public class FirebaseOptions
{
    /// <summary>
    /// Configuration section to read the options from
    /// </summary>
    public const string ConfigSection = "Firebase";

    /// <summary>
    /// Path to the JSON file containing Firebase credentials (service account key).
    /// If null, then Firebase push notifications will not work.
    /// </summary>
    public string? CredentialsPath { get; init; }
}
