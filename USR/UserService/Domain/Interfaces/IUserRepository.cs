using Aristotle.Domain.Entities;

namespace Aristotle.Domain.Interfaces;

// I truly like to keep interfaces lke this, it is easier to find the data access layer
// and it is eeasier to implement the repository pattern
/// <summary>
/// </summary>
public interface IUserRepository
{
    /// <summary>
    ///     Gets a user by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier from the identity provider.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByExternalUserIdAsync(Guid externalUserId);

    /// <summary>
    ///     Gets all users.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<User>> GetAllAsync();


    /// <summary>
    ///     Adds a new user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<User> AddAsync(User user);

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    ///     Deletes a user by their external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier of the user to delete.</param>
    /// <returns>True if the user was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(Guid externalUserId);

    /// <summary>
    ///     Wipes all data from the user database. Primarily used for testing purposes.
    /// </summary>
    /// <returns></returns>
    Task WipeDatabaseAsync();
}