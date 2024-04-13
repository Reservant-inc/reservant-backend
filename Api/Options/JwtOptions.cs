using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Reservant.Api.Options;

/// <summary>
/// JWT configuration.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Configuration section to read the options from.
    /// </summary>
    public const string ConfigSection = "JwtOptions";

    /// <summary>
    /// The token's issuer.
    /// </summary>
    [Required]
    public required string Issuer { get; init; }

    /// <summary>
    /// The token's audience.
    /// </summary>
    [Required]
    public required string Audience { get; init; }

    /// <summary>
    /// Used to sign the token. Must be kept secret.
    /// </summary>
    [Required]
    public required string Key { get; init; }

    /// <summary>
    /// Token lifetime in hours.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LifetimeHours { get; init; }

    /// <summary>
    /// Key as bytes.
    /// </summary>
    public byte[] GetKeyBytes() => Encoding.UTF8.GetBytes(Key);
}
