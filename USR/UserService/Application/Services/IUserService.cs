using System.Security.Claims;
using Aristotle.Domain.Entities;

namespace Aristotle.Application.Services;

/// <summary>
///     Provides methods for user-related business operations such as retrieving, creating, updating, and deleting users.
///     Also handles Just-In-Time (JIT) user provisioning.
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Retrieves a user by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByExternalUserIdAsync(Guid externalUserId);

    /// <summary>
    ///     Retrieves all users.
    /// </summary>
    /// <returns>An enumerable collection of users.</returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    ///     Wipes all data from the user database. Primarily used for testing purposes.
    /// </summary>
    /// <returns></returns>
    Task WipeDatabaseAsync();

    /// <summary>
    ///     Gets an existing user or provisions a new one based on JWT claims.
    ///     Simply ensures the user exists in the local database for foreign key relationships.
    ///     Note: Login tracking and user details are managed by Keycloak, not stored here.
    /// </summary>
    /// <param name="principal">The authenticated user's claims principal containing JWT claims.</param>
    /// <returns>The user entity (existing or newly created), or null if provisioning fails.</returns>
    Task<User?> GetOrProvisionUserAsync(ClaimsPrincipal principal);
}