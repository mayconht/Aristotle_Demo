using System.Reflection;
using Aristotle.Application.Exceptions;
using Xunit;
using ApplicationException = Aristotle.Application.Exceptions.ApplicationException;

namespace UserService.UnitTests.Application.Exceptions;

/// <summary>
///     Unit tests for ApplicationException
///     Since ApplicationException is abstract, we use ServiceOperationException as a concrete implementation
/// </summary>
public class ApplicationExceptionTests
{
    [Fact]
    public void ApplicationException_Constructor_ShouldSetServiceAndMessage()
    {
        // Arrange
        const string service = "TestService";
        const string message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TestApplicationException(service, message, innerException);

        // Assert
        Assert.Equal(service, exception.Service);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void ApplicationException_ErrorCode_ShouldBeSetToTypeName()
    {
        // Arrange
        const string service = "UserService";
        const string message = "Error occurred";
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException(service, message, innerException);

        // Assert
        Assert.Equal("TestApplicationException", exception.ErrorCode);
    }

    [Fact]
    public void ApplicationException_ErrorCode_ShouldMatchExceptionTypeName()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var testException = new TestApplicationException("Service", "Message", innerException);
        var serviceException = new ServiceOperationException("Service", "Op", "Message", innerException);

        // Assert
        Assert.Equal(nameof(TestApplicationException), testException.ErrorCode);
        Assert.Equal(nameof(ServiceOperationException), serviceException.ErrorCode);
    }

    [Fact]
    public void ApplicationException_ShouldInheritFromSystemException()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("Service", "Message", innerException);

        // Assert
        Assert.IsType<TestApplicationException>(exception, false);
    }

    [Fact]
    public void ApplicationException_CanBeCaughtAsBaseException()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act & Assert
        try
        {
            throw new TestApplicationException("API", "API call failed", innerException);
        }
        catch (Exception ex)
        {
            Assert.IsType<TestApplicationException>(ex);
            Assert.Equal("API call failed", ex.Message);
        }
    }

    [Fact]
    public void ApplicationException_WithNullService_ShouldHandleNull()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException(null!, "Error message", innerException);

        // Assert
        Assert.Null(exception.Service);
        Assert.NotNull(exception.ErrorCode);
    }

    [Fact]
    public void ApplicationException_WithEmptyService_ShouldAcceptEmptyString()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("", "Error message", innerException);

        // Assert
        Assert.Equal(string.Empty, exception.Service);
        Assert.NotNull(exception.ErrorCode);
    }

    [Fact]
    public void ApplicationException_WithEmptyMessage_ShouldAcceptEmptyString()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("Service", "", innerException);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
        Assert.NotNull(exception.Service);
    }

    [Fact]
    public void ApplicationException_Service_Property_IsReadOnly()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("TestService", "Test message", innerException);

        // Assert
        var serviceProperty = typeof(ApplicationException).GetProperty("Service");
        Assert.NotNull(serviceProperty);
        Assert.Null(serviceProperty!.SetMethod);
    }

    [Fact]
    public void ApplicationException_ErrorCode_Property_IsReadOnly()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("TestService", "Test message", innerException);

        // Assert
        var errorCodeProperty = typeof(ApplicationException).GetProperty("ErrorCode");
        Assert.NotNull(errorCodeProperty);
        Assert.Null(errorCodeProperty!.SetMethod);
    }

    [Fact]
    public void ApplicationException_InnerException_ShouldBePreserved()
    {
        // Arrange
        var innerException = new ArgumentNullException("userId", "User ID cannot be null");

        // Act
        var exception = new TestApplicationException("UserService", "Processing failed", innerException);

        // Assert
        Assert.NotNull(exception.InnerException);
        Assert.IsType<ArgumentNullException>(exception.InnerException);
        var argNullEx = (ArgumentNullException)exception.InnerException;
        Assert.Equal("userId", argNullEx.ParamName);
    }

    [Fact]
    public void ApplicationException_WithLongServiceName_ShouldAcceptLongStrings()
    {
        // Arrange
        var longService = new string('S', 1000);
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException(longService, "Error", innerException);

        // Assert
        Assert.Equal(longService, exception.Service);
        Assert.Equal(1000, exception.Service!.Length);
    }

    [Fact]
    public void ApplicationException_WithLongMessage_ShouldAcceptLongStrings()
    {
        // Arrange
        var longMessage = new string('M', 2000);
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException("Service", longMessage, innerException);

        // Assert
        Assert.Equal(longMessage, exception.Message);
        Assert.Equal(2000, exception.Message.Length);
    }

    [Fact]
    public void ApplicationException_WithSpecialCharacters_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        const string service = "Service<>@#$%";
        const string message = "Error with special chars: <>\"'!@#";
        var innerException = new Exception("Inner");

        // Act
        var exception = new TestApplicationException(service, message, innerException);

        // Assert
        Assert.Equal(service, exception.Service);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ApplicationException_MultipleInstances_ShouldHaveIndependentProperties()
    {
        // Arrange
        var innerException1 = new Exception("Inner1");
        var innerException2 = new Exception("Inner2");

        // Act
        var exception1 = new TestApplicationException("Service1", "Message1", innerException1);
        var exception2 = new TestApplicationException("Service2", "Message2", innerException2);

        // Assert
        Assert.NotEqual(exception1.Service, exception2.Service);
        Assert.NotEqual(exception1.Message, exception2.Message);
        Assert.Equal(exception1.ErrorCode, exception2.ErrorCode); // Same type
    }

    [Fact]
    public void ApplicationException_IsAbstract_CannotBeInstantiatedDirectly()
    {
        // Arrange
        var type = typeof(ApplicationException);

        // Assert
        Assert.True(type.IsAbstract);
    }

    [Fact]
    public void ApplicationException_DerivedTypes_ShouldInheritBaseProperties()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act
        var serviceException = new ServiceOperationException("UserService", "CreateUser", "Creation failed", innerException);

        // Assert
        Assert.IsType<ServiceOperationException>(serviceException, false);
        Assert.NotNull(serviceException.Service);
        Assert.NotNull(serviceException.ErrorCode);
        Assert.Equal("ServiceOperationException", serviceException.ErrorCode);
    }

    [Fact]
    public void ApplicationException_Constructor_IsProtected()
    {
        // Arrange
        var type = typeof(ApplicationException);
        var constructors = type.GetConstructors(
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        // Assert
        Assert.Single(constructors);
        Assert.True(constructors[0].IsFamily); // Protected
    }

    [Fact]
    public void ApplicationException_WithNullInnerException_ShouldHandleNull()
    {
        // Arrange & Act
        var exception = new TestApplicationException("Service", "Message", null!);

        // Assert
        Assert.Null(exception.InnerException);
        Assert.Equal("Service", exception.Service);
        Assert.Equal("Message", exception.Message);
    }

    [Fact]
    public void ApplicationException_WithNestedInnerExceptions_ShouldPreserveChain()
    {
        // Arrange
        var rootException = new InvalidOperationException("Root cause");
        var middleException = new ArgumentException("Middle layer", rootException);

        // Act
        var exception = new TestApplicationException("Service", "Top level error", middleException);

        // Assert
        Assert.NotNull(exception.InnerException);
        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.NotNull(exception.InnerException.InnerException);
        Assert.IsType<InvalidOperationException>(exception.InnerException.InnerException);
    }

    [Fact]
    public void ApplicationException_ToString_ShouldIncludeExceptionDetails()
    {
        // Arrange
        var innerException = new Exception("Inner error");
        var exception = new TestApplicationException("UserService", "Operation failed", innerException);

        // Act
        var exceptionString = exception.ToString();

        // Assert
        Assert.Contains("TestApplicationException", exceptionString);
        Assert.Contains("Operation failed", exceptionString);
    }

    // Test concrete implementation to test abstract base class
    private class TestApplicationException(string service, string message, Exception innerException) : ApplicationException(service, message, innerException);
}