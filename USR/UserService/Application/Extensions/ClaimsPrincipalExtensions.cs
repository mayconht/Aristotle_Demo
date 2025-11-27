using System.Security.Claims;

namespace Aristotle.Application.Extensions;

/// <summary>
///     Represents extension methods for the ClaimsPrincipal class to facilitate retrieval of user information from claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Get the user's unique identifier from the ClaimsPrincipal.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        return Guid.Parse((principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value) ?? throw new InvalidOperationException());
    }

    /// <summary>
    ///     Get the user's email address.'
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
    }

    /// <summary>
    ///     Get the user's name.'
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static string? GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("preferred_username")?.Value ?? principal.FindFirst("name")?.Value;
    }

    /// <summary>
    ///     Get the user's groups.'
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static List<string> GetGroups(this ClaimsPrincipal principal)
    {
        return principal.FindAll("groups").Select(c => c.Value).ToList();
    }
}