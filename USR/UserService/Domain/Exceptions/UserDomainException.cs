namespace Aristotle.Domain.Exceptions;

/// <summary>
///     Exception thrown when a user is not found by the specified criteria.
///     This is a specialized version of EntityNotFoundException for user entities.
/// </summary>
public class UserNotFoundException : EntityNotFoundException
{
    /// <summary>
    ///     Initializes a new instance of the UserNotFoundException class with a user ID.
    /// </summary>
    /// <param name="userId">The user ID that was not found.</param>
    public UserNotFoundException(Guid userId) : base("User", userId)
    {
    }
}