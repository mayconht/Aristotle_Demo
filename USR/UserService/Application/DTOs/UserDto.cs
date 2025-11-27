using System.ComponentModel.DataAnnotations;

namespace Aristotle.Application.DTOs;

/// <summary>
///     Data Transfer Object for User entity.
/// </summary>
public class UserResponseDto
{
    /// <summary>
    ///     Empty constructor for UserDto.
    /// </summary>
    public UserResponseDto()
    {
    }

    /// <summary>
    ///     User external identifier (from Keycloak 'sub' claim).
    /// </summary>
    public required Guid ExternalUserId { get; init; }

    /// <summary>
    ///     Timestamp when the user was first provisioned in the local database.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
///     Data Transfer Object for updating an existing user.
///     User updates are handled by Keycloak.
/// </summary> // ToDo: will be expanded in the future if we allow local user updates
public abstract class UserUpdateDto
{
    /// <summary>
    ///     Constructor for UserUpdateDto.
    /// </summary>
    protected UserUpdateDto()
    {
    }

    /// <summary>
    ///     External user identifier. Unique identifier from the Identity Provider.
    /// </summary>
    [Required]
    public required Guid ExternalUserId { get; init; }
}