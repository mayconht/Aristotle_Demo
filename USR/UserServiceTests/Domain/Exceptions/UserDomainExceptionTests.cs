using Aristotle.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Domain.Exceptions;

public class UserDomainExceptionTests
{
    [Fact]
    public void UserNotFoundException_ShouldSetMessageAndErrorCodeAndProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var ex = new UserNotFoundException(id);

        // Assert
        var expectedMessage = $"The User with identifier '{id}' was not found.";
        Assert.Equal(expectedMessage, ex.Message);
        Assert.Equal(nameof(UserNotFoundException), ex.ErrorCode);
        Assert.Equal("User", ex.EntityType);
        Assert.Equal(id, ex.EntityId);
    }
}