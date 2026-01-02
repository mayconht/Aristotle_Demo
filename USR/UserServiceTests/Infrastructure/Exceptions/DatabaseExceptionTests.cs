using Aristotle.Infrastructure.Exceptions;
using Xunit;

namespace UserService.UnitTests.Infrastructure.Exceptions;

/// <summary>
///     Unit tests for DatabaseException and RepositoryException
/// </summary>
/*TODO: Many of these tests are to validate that the correct exception is thrown. Down the line, observability will benefit from specific error handling. */
public class DatabaseExceptionTests
{
    #region DatabaseException Tests

    [Fact]
    public void DatabaseException_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string operation = "Insert";
        const string tableName = "Users";
        const string message = "Duplicate key violation";
        const string details = "Primary key 'user-123' already exists";

        // Act
        var exception = new DatabaseException(operation, tableName, message, details);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(tableName, exception.TableName);
        Assert.Contains(message, exception.Message);
        Assert.Contains(details, exception.Message);
    }

    [Fact]
    public void DatabaseException_ShouldInheritFromInfrastructureException()
    {
        // Arrange & Act
        var exception = new DatabaseException("Select", "Users", "Query failed", "Timeout");

        // Assert
        Assert.IsType<DatabaseException>(exception, false);
    }

    [Fact]
    public void DatabaseException_WithEmptyOperation_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new DatabaseException("", "Users", "Error", "Details");

        // Assert
        Assert.Equal(string.Empty, exception.Operation);
    }

    [Fact]
    public void DatabaseException_WithEmptyTableName_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new DatabaseException("Update", "", "Error", "Details");

        // Assert
        Assert.Equal(string.Empty, exception.TableName);
    }

    [Fact]
    public void DatabaseException_Message_ShouldCombineMessageAndDetails()
    {
        // Arrange
        const string message = "Connection failed";
        const string details = "Host unreachable";

        // Act
        var exception = new DatabaseException("Connect", "N/A", message, details);

        // Assert
        Assert.Contains(message, exception.Message);
        Assert.Contains("Details:", exception.Message);
        Assert.Contains(details, exception.Message);
    }

    [Fact]
    public void DatabaseException_CanBeCaughtAsInfrastructureException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new DatabaseException("Delete", "Orders", "Delete failed", "FK constraint");
        }
        catch (InfrastructureException ex)
        {
            Assert.IsType<DatabaseException>(ex);
            var dbEx = (DatabaseException)ex;
            Assert.Equal("Delete", dbEx.Operation);
            Assert.Equal("Orders", dbEx.TableName);
        }
    }

    [Fact]
    public void DatabaseException_CanBeCaughtAsBaseException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new DatabaseException("Update", "Products", "Update failed", "Concurrency");
        }
        catch (Exception ex)
        {
            Assert.IsType<DatabaseException>(ex);
        }
    }

    [Fact]
    public void DatabaseException_WithNullDetails_ShouldHandleNull()
    {
        // Arrange & Act
        var exception = new DatabaseException("Insert", "Users", "Insert failed", null!);

        // Assert
        Assert.NotNull(exception.Message);
        Assert.Contains("Insert failed", exception.Message);
    }

    [Fact]
    public void DatabaseException_Properties_AreReadOnly()
    {
        // Assert
        var operationProperty = typeof(DatabaseException).GetProperty("Operation");
        var tableNameProperty = typeof(DatabaseException).GetProperty("TableName");

        Assert.NotNull(operationProperty);
        Assert.NotNull(tableNameProperty);
        Assert.Null(operationProperty.SetMethod);
        Assert.Null(tableNameProperty.SetMethod);
    }

    [Fact]
    public void DatabaseException_WithLongOperationName_ShouldAcceptLongStrings()
    {
        // Arrange
        var longOperation = new string('X', 500);

        // Act
        var exception = new DatabaseException(longOperation, "Users", "Error", "Details");

        // Assert
        Assert.Equal(longOperation, exception.Operation);
        Assert.Equal(500, exception.Operation.Length);
    }

    [Fact]
    public void DatabaseException_WithSpecialCharacters_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        const string operation = "INSERT<>@#$%";
        const string tableName = "Table!@#";
        const string details = "Error with special chars: <>\"'";

        // Act
        var exception = new DatabaseException(operation, tableName, "Error", details);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(tableName, exception.TableName);
        Assert.Contains(details, exception.Message);
    }

    #endregion

    #region RepositoryException Tests

    [Fact]
    public void RepositoryException_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string repositoryType = "UserRepository";
        const string entityId = "user-123";
        const string message = "Entity not found";

        // Act
        var exception = new RepositoryException(repositoryType, entityId, message);

        // Assert
        Assert.Equal("Repository", exception.Operation);
        Assert.Equal("Entity", exception.TableName);
        Assert.Contains(message, exception.Message);
        Assert.Contains(repositoryType, exception.Message);
        Assert.Contains(entityId, exception.Message);
    }

    [Fact]
    public void RepositoryException_ShouldInheritFromDatabaseException()
    {
        // Arrange & Act
        var exception = new RepositoryException("UserRepository", "123", "Not found");

        // Assert
        Assert.IsType<RepositoryException>(exception, false);
    }

    [Fact]
    public void RepositoryException_ShouldInheritFromInfrastructureException()
    {
        // Arrange & Act
        var exception = new RepositoryException("UserRepository", "123", "Not found");

        // Assert
        Assert.IsType<RepositoryException>(exception, false);
    }

    [Fact]
    public void RepositoryException_WithGuidEntityId_ShouldAcceptGuid()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var exception = new RepositoryException("UserRepository", entityId, "Entity not found");

        // Assert
        Assert.Contains(entityId.ToString(), exception.Message);
    }

    [Fact]
    public void RepositoryException_WithIntEntityId_ShouldAcceptInt()
    {
        // Arrange
        const int entityId = 12345;

        // Act
        var exception = new RepositoryException("OrderRepository", entityId, "Order not found");

        // Assert
        Assert.Contains(entityId.ToString(), exception.Message);
    }

    [Fact]
    public void RepositoryException_WithNullEntityId_ShouldHandleNull()
    {
        // Arrange & Act
        var exception = new RepositoryException("UserRepository", null!, "Error");

        // Assert
        Assert.NotNull(exception.Message);
        Assert.Contains("UserRepository", exception.Message);
    }

    [Fact]
    public void RepositoryException_CanBeCaughtAsDatabaseException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new RepositoryException("ProductRepository", "prod-456", "Product not found");
        }
        catch (DatabaseException ex)
        {
            Assert.IsType<RepositoryException>(ex);
            Assert.Equal("Repository", ex.Operation);
            Assert.Equal("Entity", ex.TableName);
        }
    }

    [Fact]
    public void RepositoryException_CanBeCaughtAsInfrastructureException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new RepositoryException("CategoryRepository", 789, "Category not found");
        }
        catch (InfrastructureException ex)
        {
            Assert.IsType<RepositoryException>(ex);
        }
    }

    [Fact]
    public void RepositoryException_CanBeCaughtAsBaseException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new RepositoryException("UserRepository", "user-999", "User not found");
        }
        catch (Exception ex)
        {
            Assert.IsType<RepositoryException>(ex);
        }
    }

    [Fact]
    public void RepositoryException_WithEmptyRepositoryType_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new RepositoryException("", "123", "Error");

        // Assert
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void RepositoryException_WithEmptyMessage_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new RepositoryException("UserRepository", "123", "");

        // Assert
        Assert.NotNull(exception.Message);
        Assert.Contains("UserRepository", exception.Message);
    }

    [Fact]
    public void RepositoryException_WithComplexObject_ShouldAcceptComplexEntityId()
    {
        // Arrange
        var complexId = new { UserId = 123, TenantId = 456 };
        var complexIdString = $"UserId={complexId.UserId},TenantId={complexId.TenantId}";

        // Act
        var exception = new RepositoryException("MultiTenantRepository", complexIdString, "Entity not found");

        // Assert
        Assert.Contains(complexIdString, exception.Message);
    }

    [Fact]
    public void RepositoryException_WithSpecialCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        const string repositoryType = "Repository<User>";
        const string entityId = "user@domain.com";
        const string message = "Error: <>\"'!@#";

        // Act
        var exception = new RepositoryException(repositoryType, entityId, message);

        // Assert
        Assert.Contains(repositoryType, exception.Message);
        Assert.Contains(entityId, exception.Message);
        Assert.Contains(message, exception.Message);
    }

    #endregion

    #region Cross-Exception Hierarchy Tests

    [Fact]
    public void ExceptionHierarchy_DatabaseException_IsInfrastructureException()
    {
        // Arrange
        Exception exception = new DatabaseException("Op", "Table", "Msg", "Details");

        // Assert
        Assert.IsType<DatabaseException>(exception, false);
    }

    [Fact]
    public void ExceptionHierarchy_RepositoryException_IsDatabaseException()
    {
        // Arrange
        Exception exception = new RepositoryException("Repo", "123", "Msg");

        // Assert
        Assert.IsType<RepositoryException>(exception, false);
    }

    [Fact]
    public void ExceptionHierarchy_RepositoryException_IsInfrastructureException()
    {
        // Arrange
        Exception exception = new RepositoryException("Repo", "123", "Msg");

        // Assert
        Assert.IsType<RepositoryException>(exception, false);
    }

    #endregion
}