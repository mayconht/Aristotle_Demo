using Aristotle.Application.Exceptions;
using Xunit;
using ApplicationException = Aristotle.Application.Exceptions.ApplicationException;

namespace UserService.UnitTests.Application.Exceptions;

/// <summary>
///     Unit tests for ServiceOperationException
/// </summary>
public class ServiceOperationExceptionTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "CreateUser";
        const string message = "Failed to create user";
        var innerException = new InvalidOperationException("Database connection failed");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
        Assert.NotNull(exception.Service);
    }

    [Fact]
    public void Constructor_WithNullOperation_ShouldAcceptNullOperation()
    {
        // Arrange
        const string service = "UserService";
        const string? operation = null;
        const string message = "Operation failed";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new ServiceOperationException(service, operation!, message, innerException);

        // Assert
        Assert.Null(exception.Operation);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyOperation_ShouldAcceptEmptyOperation()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "";
        const string message = "Operation failed";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.Equal(string.Empty, exception.Operation);
    }

    [Fact]
    public void Constructor_ShouldInheritFromApplicationException()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "DeleteUser";
        const string message = "Failed to delete user";
        var innerException = new Exception("Error");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.IsType<ServiceOperationException>(exception, false);
    }

    [Fact]
    public void Exception_CanBeCaughtAsApplicationException()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "UpdateUser";
        const string message = "Update failed";
        var innerException = new Exception("Error");

        // Act & Assert
        try
        {
            throw new ServiceOperationException(service, operation, message, innerException);
        }
        catch (ApplicationException ex)
        {
            Assert.IsType<ServiceOperationException>(ex);
            var serviceEx = (ServiceOperationException)ex;
            Assert.Equal(operation, serviceEx.Operation);
        }
    }

    [Fact]
    public void Exception_CanBeCaughtAsBaseException()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "GetUser";
        const string message = "Get failed";
        var innerException = new Exception("Error");

        // Act & Assert
        Exception? caughtException = null;
        try
        {
            throw new ServiceOperationException(service, operation, message, innerException);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.NotNull(caughtException);
        Assert.IsType<ServiceOperationException>(caughtException);
    }

    [Fact]
    public void Operation_Property_IsReadOnly()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "TestOperation";
        const string message = "Test message";
        var innerException = new Exception("Error");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert - Verify property is get-only (this is compile-time checked, but we verify the value)
        Assert.Equal(operation, exception.Operation);

        // Ensure Operation property doesn't have a public setter
        var propertyInfo = typeof(ServiceOperationException).GetProperty("Operation");
        Assert.NotNull(propertyInfo);
        Assert.Null(propertyInfo!.SetMethod);
    }

    [Fact]
    public void Constructor_WithDifferentInnerExceptionTypes_ShouldPreserveInnerException()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "ProcessUser";
        const string message = "Processing failed";
        var innerException = new ArgumentNullException("userId", "User ID cannot be null");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.NotNull(exception.InnerException);
        Assert.IsType<ArgumentNullException>(exception.InnerException);
        var argNullEx = (ArgumentNullException)exception.InnerException;
        Assert.Equal("userId", argNullEx.ParamName);
    }

    [Fact]
    public void Constructor_WithLongOperationName_ShouldAcceptLongStrings()
    {
        // Arrange
        const string service = "UserService";
        var operation = new string('A', 1000); // Very long operation name
        const string message = "Operation failed";
        var innerException = new Exception("Error");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(1000, exception.Operation!.Length);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInOperation_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        const string service = "UserService";
        const string operation = "Create<User>_With@Special#Characters!";
        const string message = "Operation failed";
        var innerException = new Exception("Error");

        // Act
        var exception = new ServiceOperationException(service, operation, message, innerException);

        // Assert
        Assert.Equal(operation, exception.Operation);
    }
}