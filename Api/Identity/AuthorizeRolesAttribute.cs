using Microsoft.AspNetCore.Authorization;

namespace Reservant.Api.Identity;

/// <summary>
/// Makes using the Authorize attribute easier when providing several roles
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Construct the attribute with the given roles
    /// </summary>
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(',', roles);
    }
}
