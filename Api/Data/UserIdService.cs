using Reservant.Api.Identity;

namespace Reservant.Api.Data;

/// <summary>
/// Allows accessing current user's ID
/// </summary>
public class UserIdService(IHttpContextAccessor contextAccessor)
{
    /// <summary>
    /// Get current user's ID
    /// </summary>
    public string? GetUserId()
    {
        return contextAccessor.HttpContext?.User.GetUserId();
    }
}
