using System.Security.Claims;
using Aristotle.Application.Exceptions;
using Aristotle.Application.Extensions;
using Aristotle.Application.Validators;
using Aristotle.Domain.Entities;
using Aristotle.Domain.Interfaces;

namespace Aristotle.Application.Services;

/// <summary>
///     Service class that handles user-related business operations.
///     Provides methods for user operations with proper exception handling
///     and business logic validation following hexagonal architecture principles.
///     Also handles Just-In-Time (JIT) user provisioning.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserValidator _userValidator;

    /// <summary>
    ///     Initializes a new instance of the UserService class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access operations.</param>
    /// <param name="userValidator">Validator for user business rules.</param>
    /// <param name="logger">Logger for service operations and error tracking.</param>
    public UserService(IUserRepository userRepository, IUserValidator userValidator, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Gets user by their external user ID.
    /// </summary>
    /// <param name="externalUserId">The external user ID of the user to retrieve.</param>
    /// <returns>The user if found, null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the externalUserId parameter is null or empty.</exception>
    /// <exception cref="ServiceOperationException">Thrown when the operation fails due to infrastructure issues.</exception>
    public async Task<User?> GetUserByExternalUserIdAsync(Guid externalUserId)
    {
        try
        {
            await _userValidator.ValidateGuid(externalUserId);

            _logger.LogDebug("Getting user by external user ID");
            var user = await _userRepository.GetByExternalUserIdAsync(externalUserId);
            return user;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            _logger.LogError(ex, "Error occurred while getting user by external user ID: {ExternalUserId}", externalUserId);
            throw new ServiceOperationException(nameof(UserService), nameof(GetUserByExternalUserIdAsync),
                "An error occurred while retrieving the user by external user ID.", ex);
        }
    }

    /// <summary>
    ///     Gets all users from the system.
    /// </summary>
    /// <returns>A collection of all users in the system.</returns>
    /// <exception cref="ServiceOperationException">
    ///     Thrown when the operation fails due to infrastructure issues (wrapped by
    ///     GlobalExceptionHandlingMiddleware).
    /// </exception>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Getting all users");

        var users = await _userRepository.GetAllAsync();
        var allUsersAsync = users.ToList();

        _logger.LogDebug("Successfully retrieved {UserCount} users", allUsersAsync.Count);

        return allUsersAsync;
    }

    /// <summary>
    ///     Wipes all data from the user database. Primarily used for testing purposes.
    /// </summary>
    /// <returns></returns>
    public Task WipeDatabaseAsync()
    {
        _logger.LogWarning("Wiping all data from the user database. This operation is irreversible and should only be used for testing purposes.");
        return _userRepository.WipeDatabaseAsync();
    }

    /// <summary>
    ///     Gets an existing user or provisions a new one based on JWT claims.
    ///     Simply ensures the user exists in the local database for foreign key relationships.
    ///     Note: Login tracking and user details are managed by Keycloak, not stored here.
    /// </summary>
    /// <param name="principal">The authenticated user's claims principal containing JWT claims.</param>
    /// <returns>The user entity (existing or newly created), or null if provisioning fails.</returns>
    public async Task<User?> GetOrProvisionUserAsync(ClaimsPrincipal? principal)
    {
        if (principal == null || principal.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Attempted to provision user for null or unauthenticated principal");
            return null;
        }

        try
        {
            // Extract ExternalUserId (subclaim) from JWT
            var externalUserId = principal.GetUserId();

            if (externalUserId == Guid.Empty)
            {
                _logger.LogWarning("Failed to extract valid ExternalUserId from principal claims");
                return null;
            }

            var existingUser = await _userRepository.GetByExternalUserIdAsync(externalUserId);

            if (existingUser != null)
            {
                _logger.LogDebug("User {ExternalUserId} already exists in database", externalUserId);
                return existingUser;
            }

            _logger.LogInformation("Provisioning new user {ExternalUserId} (Email: {Email}, Name: {Name})", externalUserId, principal.GetEmail(), principal.GetName());

            var newUser = new User
            {
                ExternalUserId = externalUserId,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(newUser);

            _logger.LogInformation("Successfully provisioned new user {ExternalUserId}", externalUserId);

            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user provisioning for principal {Name}", principal.GetName() ?? "Unknown");
            
            // Return null instead of throwing to avoid breaking the authentication flow
            return null;
        }
    }
}