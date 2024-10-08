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
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return id == null ? null : Guid.Parse(id);
    }
}
