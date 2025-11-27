namespace Aristotle.Application.Authorization;

/// <summary>
///     Represents authorization policies for the application.
/// </summary>
public static class Policies
{
    /// <summary>
    ///     Policy for requiring admin role.
    /// </summary>
    public const string RequireAdminRole = "RequireAdminRole";


    /// <summary>
    ///     Policy for requiring user role.
    /// </summary>
    public const string RequireUserRole = "RequireUserRole";
}