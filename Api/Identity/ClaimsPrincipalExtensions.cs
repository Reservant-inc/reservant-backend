using System.Security.Claims;

namespace Reservant.Api.Identity;

/// <summary>
/// Extension methods
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Get user ID from a ClaimsPrincipal
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
