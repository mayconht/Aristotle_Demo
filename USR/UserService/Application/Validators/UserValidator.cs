using Aristotle.Domain.Entities;
using Aristotle.Domain.Exceptions;

namespace Aristotle.Application.Validators;

/// <summary>
///     Validator service for User entity business rules.
/// </summary>
public class UserValidator : IUserValidator
{
    private readonly ILogger<UserValidator> _logger;

    /// <summary>
    ///     Initializes a new instance of the UserValidator class.
    /// </summary>
    /// <param name="logger">The logger instance for validation operations.</param>
    public UserValidator(ILogger<UserValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Validates user data according to domain rules.
    /// </summary>
    /// <param name="user">The user to validate.</param>
    /// <exception cref="DomainValidationException">Thrown when validation fails.</exception>
    public Task ValidateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var validationErrors = new Dictionary<string, List<string>>();

        if (Guid.Empty == user.ExternalUserId)
        {
            validationErrors.Add(nameof(User.ExternalUserId), ["External User ID is required and cannot be empty."]);
        }
        
        if (validationErrors.Count != 0)
        {
            _logger.LogWarning("User validation failed with {ErrorCount} errors for ExternalUserId: {ExternalUserId}", validationErrors.Count, user.ExternalUserId);

            throw new DomainValidationException(validationErrors, nameof(User));
        }

        _logger.LogDebug("User validation successful for ExternalUserId: {ExternalUserId}", user.ExternalUserId);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates that the provided GUID is not empty and is a valid identifier.
    /// </summary>
    /// <param name="externalUserId">The external user ID to validate.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the external user ID is empty.</exception>
    public Task ValidateGuid(Guid externalUserId)
    {
        if (Guid.Empty == externalUserId)
        {
            _logger.LogWarning("Validation failed: External User ID is empty.");
            throw new ArgumentNullException(nameof(externalUserId), "External User ID cannot be empty.");
        }

        _logger.LogDebug("Validation successful for External User ID: {ExternalUserId}", externalUserId);
        return Task.CompletedTask;
    }
}