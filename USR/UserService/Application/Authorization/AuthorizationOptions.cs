namespace Aristotle.Application.Authorization;

/// <summary>
///     Defines role names for role-based access control in the system.
///     All members are static as role names are constants.
/// </summary>
public static class RolesConfiguration
{
    /// <summary>
    ///     Administrator role name.
    /// </summary>
    public static string Admin => "Admins";

    /// <summary>
    ///     User role name.
    /// </summary>
    public static string User => "Users";
}