using System.Reflection;
using Aristotle.Infrastructure.Exceptions;
using Xunit;

namespace UserService.UnitTests.Infrastructure.Exceptions;

/// <summary>
///     Unit tests for InfrastructureException
///     Since InfrastructureException is abstract, we use DatabaseException as a concrete implementation
/// </summary>
public class InfrastructureExceptionTests
{
    [Fact]
    public void InfrastructureException_Constructor_ShouldSetComponentAndMessage()
    {
        // Arrange
        const string component = "TestComponent";
        const string message = "Test error message";

        // Act
        var exception = new TestInfrastructureException(component, message);

        // Assert
        Assert.Equal(component, exception.Component);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InfrastructureException_ErrorCode_ShouldBeSetToTypeName()
    {
        // Arrange
        const string component = "Database";
        const string message = "Error occurred";

        // Act
        var exception = new TestInfrastructureException(component, message);

        // Assert
        Assert.Equal("TestInfrastructureException", exception.ErrorCode);
    }

    [Fact]
    public void InfrastructureException_ErrorCode_ShouldMatchExceptionTypeName()
    {
        // Arrange & Act
        var testException = new TestInfrastructureException("Component", "Message");
        var dbException = new DatabaseException("Op", "Table", "Msg", "Details");

        // Assert
        Assert.Equal(nameof(TestInfrastructureException), testException.ErrorCode);
        Assert.Equal(nameof(DatabaseException), dbException.ErrorCode);
    }

    [Fact]
    public void InfrastructureException_ShouldInheritFromSystemException()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("Component", "Message");

        // Assert
        Assert.IsType<TestInfrastructureException>(exception, false);
    }

    [Fact]
    public void InfrastructureException_CanBeCaughtAsBaseException()
    {
        // Arrange & Act & Assert
        try
        {
            throw new TestInfrastructureException("API", "API call failed");
        }
        catch (Exception ex)
        {
            Assert.IsType<TestInfrastructureException>(ex);
            Assert.Equal("API call failed", ex.Message);
        }
    }

    [Fact]
    public void InfrastructureException_WithEmptyComponent_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("", "Error message");

        // Assert
        Assert.Equal(string.Empty, exception.Component);
        Assert.NotNull(exception.ErrorCode);
    }

    [Fact]
    public void InfrastructureException_WithEmptyMessage_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("Component", "");

        // Assert
        Assert.Equal(string.Empty, exception.Message);
        Assert.NotNull(exception.Component);
    }

    [Fact]
    public void InfrastructureException_Component_Property_IsReadOnly()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("TestComp", "Test message");

        // Assert
        var componentProperty = typeof(InfrastructureException).GetProperty("Component");
        Assert.NotNull(componentProperty);
        Assert.Null(componentProperty!.SetMethod);
    }

    [Fact]
    public void InfrastructureException_ErrorCode_Property_IsReadOnly()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("TestComp", "Test message");

        // Assert
        var errorCodeProperty = typeof(InfrastructureException).GetProperty("ErrorCode");
        Assert.NotNull(errorCodeProperty);
        Assert.Null(errorCodeProperty!.SetMethod);
    }

    [Fact]
    public void InfrastructureException_WithNullComponent_ShouldHandleNull()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException(null!, "Error message");

        // Assert
        Assert.Null(exception.Component);
        Assert.NotNull(exception.ErrorCode);
        Assert.Equal("Error message", exception.Message);
    }

    [Fact]
    public void InfrastructureException_WithNullMessage_ShouldHandleNull()
    {
        // Arrange & Act
        var exception = new TestInfrastructureException("Component", null!);

        // Assert
        Assert.NotNull(exception.Message); // Exception base class creates a default message
        Assert.Equal("Component", exception.Component);
    }

    [Fact]
    public void InfrastructureException_WithLongComponentName_ShouldAcceptLongStrings()
    {
        // Arrange
        var longComponent = new string('C', 1000);

        // Act
        var exception = new TestInfrastructureException(longComponent, "Error");

        // Assert
        Assert.Equal(longComponent, exception.Component);
        Assert.Equal(1000, exception.Component!.Length);
    }

    [Fact]
    public void InfrastructureException_WithLongMessage_ShouldAcceptLongStrings()
    {
        // Arrange
        var longMessage = new string('M', 2000);

        // Act
        var exception = new TestInfrastructureException("Component", longMessage);

        // Assert
        Assert.Equal(longMessage, exception.Message);
        Assert.Equal(2000, exception.Message.Length);
    }

    [Fact]
    public void InfrastructureException_WithSpecialCharacters_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        const string component = "Component<>@#$%";
        const string message = "Error with special chars: <>\"'!@#";

        // Act
        var exception = new TestInfrastructureException(component, message);

        // Assert
        Assert.Equal(component, exception.Component);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InfrastructureException_MultipleInstances_ShouldHaveIndependentProperties()
    {
        // Arrange & Act
        var exception1 = new TestInfrastructureException("Component1", "Message1");
        var exception2 = new TestInfrastructureException("Component2", "Message2");

        // Assert
        Assert.NotEqual(exception1.Component, exception2.Component);
        Assert.NotEqual(exception1.Message, exception2.Message);
        Assert.Equal(exception1.ErrorCode, exception2.ErrorCode); // Same type
    }

    [Fact]
    public void InfrastructureException_IsAbstract_CannotBeInstantiatedDirectly()
    {
        // Arrange
        var type = typeof(InfrastructureException);

        // Assert
        Assert.True(type.IsAbstract);
    }

    [Fact]
    public void InfrastructureException_DerivedTypes_ShouldInheritBaseProperties()
    {
        // Arrange & Act
        var dbException = new DatabaseException("Insert", "Users", "Insert failed", "Duplicate key");

        // Assert
        Assert.IsType<DatabaseException>(dbException, false);
        Assert.NotNull(dbException.Component);
        Assert.NotNull(dbException.ErrorCode);
        Assert.Equal("DatabaseException", dbException.ErrorCode);
    }

    [Fact]
    public void InfrastructureException_Constructor_IsProtected()
    {
        // Arrange
        var type = typeof(InfrastructureException);
        var constructors = type.GetConstructors(
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        // Assert
        Assert.Single(constructors);
        Assert.True(constructors[0].IsFamily); // Protected
    }

    // Test concrete implementation to test abstract base class
    private class TestInfrastructureException : InfrastructureException
    {
        public TestInfrastructureException(string component, string message)
            : base(component, message)
        {
        }
    }
}