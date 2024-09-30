using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Options;

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
    /// Path to the JSON file containing Firebase credentials (service account key)
    /// </summary>
    [Required]
    public required string CredentialsPath { get; init; }
}
