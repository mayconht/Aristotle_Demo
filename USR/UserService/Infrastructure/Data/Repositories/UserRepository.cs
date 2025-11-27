using Aristotle.Domain.Entities;
using Aristotle.Domain.Interfaces;
using Aristotle.Infrastructure.Exceptions;
using Aristotle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aristotle.Infrastructure.Data.Repositories;

/// <summary>
///     Repository implementation for User entity data access operations.
///     Handles database interactions with proper exception handling and logging.
///     Implements the IUserRepository interface following hexagonal architecture principles.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    /// <summary>
    ///     Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The Entity Framework database context.</param>
    /// <param name="logger">Logger for repository operations and error tracking.</param>
    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Gets a user by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier from the identity provider.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    /// <exception cref="DatabaseException">Thrown when database operation fails.</exception>
    public async Task<User?> GetByExternalUserIdAsync(Guid externalUserId)
    {
        try
        {
            _logger.LogDebug("Retrieving user by external user ID: {ExternalUserId}", externalUserId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);
            _logger.LogDebug("External user ID lookup completed. Found: {Found}", user != null);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred while retrieving user by external user ID: {ExternalUserId}",
                externalUserId);
            throw new DatabaseException(nameof(GetByExternalUserIdAsync), nameof(User),
                $"Failed to retrieve user by external user ID: {externalUserId}", ex.ToString());
        }
    }

    /// <summary>
    ///     Gets all users from the database.
    /// </summary>
    /// <returns>A collection of all users in the system.</returns>
    /// <exception cref="DatabaseException">Thrown when database operation fails.</exception>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Retrieving all users");
            var users = await _context.Users.ToListAsync();
            _logger.LogDebug("Successfully retrieved {UserCount} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred while retrieving all users");
            throw new DatabaseException(nameof(GetAllAsync), nameof(User), "Failed to retrieve all users", ex.ToString());
        }
    }

    /// <summary>
    ///     Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <returns>The added user with generated ExternalUserId.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="DatabaseException">Thrown when database operation fails.</exception>
    public async Task<User> AddAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user), "User cannot be null");
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully added user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            return user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogDebug(ex, "Database constraint violation while adding user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);

            // PostgreSQL error code 23505 = unique_violation
            if (ex.InnerException?.Message.Contains("23505") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                throw new DatabaseException(nameof(AddAsync), nameof(User),
                    $"A user with ExternalUserId '{user.ExternalUserId}' already exists in the database",
                    ex.ToString());
            throw new DatabaseException(nameof(AddAsync), nameof(User), $"Failed to add user with ExternalUserId: {user.ExternalUserId}", ex.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while adding user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            throw new DatabaseException(nameof(AddAsync), nameof(User),
                $"Unexpected error occurred while adding user with ExternalUserId: {user.ExternalUserId}",
                ex.ToString());
        }
    }

    /// <summary>
    ///     Updates an existing user in the database identified by ExternalUserId.
    /// </summary>
    /// <param name="user">The user entity with updated information, identified by ExternalUserId.</param>
    /// <returns>The updated user entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="RepositoryException">Thrown when user to update is not found.</exception>
    /// <exception cref="DatabaseException">Thrown when database operation fails.</exception>
    public async Task<User> UpdateAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user), "User cannot be null");
        try
        {
            _logger.LogDebug("Updating user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == user.ExternalUserId);
            if (existingUser == null)
                throw new RepositoryException(nameof(UserRepository), user.ExternalUserId, $"User with ExternalUserId {user.ExternalUserId} not found for update");

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            return user;
        }
        catch (RepositoryException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while updating user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            throw new DatabaseException(nameof(UpdateAsync), nameof(User), "The user was modified by another process", ex.ToString());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while updating user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);

            // PostgreSQL error code 23505 = unique_violation
            if (ex.InnerException?.Message.Contains("23505") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                throw new DatabaseException(nameof(UpdateAsync), nameof(User), "A user with this ExternalUserId already exists in the database", ex.ToString());

            throw new DatabaseException(nameof(UpdateAsync), nameof(User), "Failed to update user in database", ex.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating user with ExternalUserId: {ExternalUserId}", user.ExternalUserId);
            throw new DatabaseException(nameof(UpdateAsync), nameof(User), "Unexpected error occurred while updating user", ex.ToString());
        }
    }

    /// <summary>
    ///     Deletes a user from the database by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier of the user to delete.</param>
    /// <returns>True if the user was deleted successfully, false if the user was not found.</returns>
    /// <exception cref="DatabaseException">Thrown when database operation fails.</exception>
    public async Task<bool> DeleteAsync(Guid externalUserId)
    {
        try
        {
            _logger.LogDebug("Deleting user with ExternalUserId: {ExternalUserId}", externalUserId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);
            if (user == null)
            {
                _logger.LogInformation("User with ExternalUserId {ExternalUserId} not found for deletion", externalUserId);
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted user with ExternalUserId: {ExternalUserId}", externalUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting user with ExternalUserId: {ExternalUserId}", externalUserId);
            throw new DatabaseException(nameof(DeleteAsync), nameof(User), $"Failed to delete user with ExternalUserId: {externalUserId}", ex.ToString());
        }
    }

    /// <summary>
    ///     Wipes all data from the user database. Primarily used for testing purposes.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    public Task WipeDatabaseAsync()
    {
        try
        {
            _logger.LogWarning("Wiping all user data from the database. This operation is irreversible.");
            _context.Users.RemoveRange(_context.Users);
            return _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred while wiping user database");
            throw new DatabaseException(nameof(WipeDatabaseAsync), nameof(User), "Failed to wipe user database", ex.ToString());
        }
    }
}