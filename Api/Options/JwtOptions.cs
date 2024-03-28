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
    public required string Issuer { get; init; }

    /// <summary>
    /// The token's audience.
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Used to sign the token. Must be kept secret.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Token lifetime in hours.
    /// </summary>
    public int LifetimeHours { get; init; }

    /// <summary>
    /// Key as bytes.
    /// </summary>
    public byte[] GetKeyBytes() => Encoding.UTF8.GetBytes(Key);
}
