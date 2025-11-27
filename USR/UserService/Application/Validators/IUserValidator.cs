using Aristotle.Domain.Entities;

namespace Aristotle.Application.Validators;

/// <summary>
///     Interface for user validation service.
/// </summary>
public interface IUserValidator
{
    /// <summary>
    ///     Validates user data according to domain rules.
    /// </summary>
    /// <param name="user">The user to validate.</param>
    /// <exception cref="Aristotle.Domain.Exceptions.DomainValidationException">Thrown when validation fails.</exception>
    Task ValidateUserAsync(User user);

    /// <summary>
    ///     Validates that the provided external user ID is not empty and is a valid identifier.
    /// </summary>
    /// <param name="externalUserId">The external user ID to validate.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when the external user ID is empty.</exception>
    Task ValidateGuid(Guid externalUserId);
}