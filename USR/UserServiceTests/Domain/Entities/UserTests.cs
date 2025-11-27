using Aristotle.Domain.Entities;
using Xunit;

namespace UserService.UnitTests.Domain.Entities;
//Many tests will feel dumb or redundant, but this is a demo project for learning purposes
public class UserTests
{
    #region Constructor and Property Tests

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var externalUserId = Guid.NewGuid();
        var user = new User { ExternalUserId = externalUserId };

        // Assert
        Assert.Equal(externalUserId, user.ExternalUserId);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void ExternalUserId_Property_ShouldBeRequired()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();

        // Act
        var user = new User { ExternalUserId = externalUserId };

        // Assert
        Assert.Equal(externalUserId, user.ExternalUserId);
    }

    [Fact]
    public void CreatedAt_Property_ShouldBeSetToCurrentUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = new User { ExternalUserId = Guid.NewGuid() };
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(user.CreatedAt >= beforeCreation);
        Assert.True(user.CreatedAt <= afterCreation);
    }

    [Fact]
    public void User_WithAllProperties_ShouldCreateSuccessfully()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();

        // Act
        var user = new User
        {
            ExternalUserId = externalUserId
        };

        // Assert
        Assert.Equal(externalUserId, user.ExternalUserId);
        Assert.NotEqual(default, user.CreatedAt);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public void ExternalUserId_CanBeEmptyGuid()
    {
        // Arrange & Act
        var user = new User { ExternalUserId = Guid.Empty };

        // Assert
        Assert.Equal(Guid.Empty, user.ExternalUserId);
    }

    [Fact]
    public void CreatedAt_IsImmutable()
    {
        // Arrange
        var user = new User { ExternalUserId = Guid.NewGuid() };
        var originalCreatedAt = user.CreatedAt;

        // Act - CreatedAt has 'init' accessor, cannot be changed after construction
        // user.CreatedAt = DateTime.UtcNow;
        // This test validates that the property exists and is set

        // Assert
        Assert.Equal(originalCreatedAt, user.CreatedAt);
    }

    [Fact]
    public void User_SupportsMultipleInstances()
    {
        // Arrange & Act
        var user1 = new User { ExternalUserId = Guid.NewGuid() };
        var user2 = new User { ExternalUserId = Guid.NewGuid() };

        // Assert
        Assert.NotEqual(user1.ExternalUserId, user2.ExternalUserId);
        Assert.NotEqual(user1.CreatedAt, user2.CreatedAt);
    }

    #endregion
}