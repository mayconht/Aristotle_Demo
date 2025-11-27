namespace Aristotle.Domain.Entities;

/// <summary>
///     A user entity representing a person in the system.
///     This entity is intentionally minimal as user identity and login tracking is managed in Keycloak.
///     We only store the minimum necessary for local operations.
/// </summary>
public class User
{
    /// <summary>
    ///     Constructor for User entity.
    /// </summary>
    public User()
    {
    }

    /// <summary>
    ///     External User ID (sub claim from IdP/Keycloak).
    ///     This is the primary key and correlates to the 'sub' claim in JWT tokens.
    ///     Marked as init-only (immutable after initialization) because user identity
    ///     should never change after creation. Updates use the entire User entity
    ///     which preserves this value.
    /// </summary>
    public required Guid ExternalUserId { get; init; }

    /// <summary>
    ///     Timestamp when the user was first provisioned in the local database.
    ///     Marked as init-only to prevent accidental modification of audit data.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}