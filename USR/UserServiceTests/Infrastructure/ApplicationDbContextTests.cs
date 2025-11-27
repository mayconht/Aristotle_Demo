using Aristotle.Domain.Entities;
using Aristotle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace UserService.UnitTests.Infrastructure;

public class ApplicationDbContextTests
{
    #region Disposal Tests

    [Fact]
    public void Dispose_ShouldDisposeContextProperly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDispose")
            .Options;

        ApplicationDbContext context;

        // Act
        using (context = new ApplicationDbContext(options))
        {
            Assert.NotNull(context);
        }

        Assert.Throws<ObjectDisposedException>(() => context.Users.Count());
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidOptions_ShouldCreateContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Users);
    }

    [Fact]
    public void Users_Property_ShouldBeConfigured()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestUsersProperty")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);

        // Assert
        Assert.NotNull(context.Users);
    }

    #endregion

    #region Model Configuration Tests

    [Fact]
    public void OnModelCreating_ShouldConfigureUserEntity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestModelCreating")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var model = context.Model;
        var userEntity = model.FindEntityType(typeof(User));

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal("Users", userEntity.GetTableName());
    }

    [Fact]
    public void OnModelCreating_UserEntity_ShouldHaveCorrectKeyConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestKeyConfig")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var model = context.Model;
        var userEntity = model.FindEntityType(typeof(User));
        var primaryKey = userEntity?.FindPrimaryKey();

        // Assert
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("ExternalUserId", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void OnModelCreating_UserEntity_ShouldHaveCorrectPropertyConfigurations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestPropertyConfig")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var model = context.Model;
        var userEntity = model.FindEntityType(typeof(User));

        var externalUserIdProperty = userEntity?.FindProperty("ExternalUserId");
        var createdAtProperty = userEntity?.FindProperty("CreatedAt");

        // Assert
        Assert.NotNull(externalUserIdProperty);
        Assert.False(externalUserIdProperty.IsNullable);
        Assert.False(externalUserIdProperty.ValueGenerated.HasFlag(ValueGenerated.OnAdd));

        Assert.NotNull(createdAtProperty);
        Assert.False(createdAtProperty.IsNullable);
    }

    [Fact]
    public void OnModelCreating_UserEntity_ShouldHaveUniqueExternalUserIdIndex()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestExternalUserIdIndex")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var model = context.Model;
        var userEntity = model.FindEntityType(typeof(User));
        var externalUserIdIndex = userEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "ExternalUserId"));

        // Assert
        Assert.NotNull(externalUserIdIndex);
        Assert.True(externalUserIdIndex.IsUnique);
        Assert.Single(externalUserIdIndex.Properties);
        Assert.Equal("ExternalUserId", externalUserIdIndex.Properties[0].Name);
    }

    [Fact]
    public void OnModelCreating_UserEntity_ShouldHaveCreatedAtIndex()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestCreatedAtIndex")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act
        var model = context.Model;
        var userEntity = model.FindEntityType(typeof(User));
        var createdAtIndex = userEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "CreatedAt"));

        // Assert
        Assert.NotNull(createdAtIndex);
        Assert.False(createdAtIndex.IsUnique);
        Assert.Single(createdAtIndex.Properties);
        Assert.Equal("CreatedAt", createdAtIndex.Properties[0].Name);
    }

    #endregion
}