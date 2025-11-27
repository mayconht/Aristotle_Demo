using Aristotle.Application.Validators;
using Aristotle.Domain.Entities;
using Aristotle.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserService.UnitTests.Application.Validators;

public class UserValidatorTests
{
    private readonly Mock<ILogger<UserValidator>> _loggerMock;
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _loggerMock = new Mock<ILogger<UserValidator>>();
        _validator = new UserValidator(_loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new UserValidator(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Arrange & Act
        var validator = new UserValidator(_loggerMock.Object);

        // Assert
        Assert.NotNull(validator);
    }

    #endregion

    #region ValidateUserAsync Tests

    [Fact]
    public async Task ValidateUserAsync_WithValidUser_DoesNotThrow()
    {
        // Arrange
        var user = new User
        {
            ExternalUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert - should not throw
        await _validator.ValidateUserAsync(user);

        // Verify debug logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation successful")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateUserAsync_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _validator.ValidateUserAsync(null!));
    }

    [Fact]
    public async Task ValidateUserAsync_WithEmptyExternalUserId_ThrowsDomainValidationException()
    {
        // Arrange
        var user = new User
        {
            ExternalUserId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => _validator.ValidateUserAsync(user));

        // Verify exception details
        Assert.Single(exception.ValidationErrors);
        Assert.Contains(nameof(User.ExternalUserId), exception.ValidationErrors.Keys);
        Assert.Contains("External User ID is required and cannot be empty.",
            exception.ValidationErrors[nameof(User.ExternalUserId)]);
        Assert.Equal(nameof(User), exception.TargetType);
    }

    [Fact]
    public async Task ValidateUserAsync_WithEmptyExternalUserId_LogsWarning()
    {
        // Arrange
        var user = new User
        {
            ExternalUserId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        try
        {
            await _validator.ValidateUserAsync(user);
        }
        catch (DomainValidationException)
        {
            // Expected exception
        }

        // Assert - verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateUserAsync_WithEmptyExternalUserId_IncludesErrorCountInLog()
    {
        // Arrange
        var user = new User
        {
            ExternalUserId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        try
        {
            await _validator.ValidateUserAsync(user);
        }
        catch (DomainValidationException)
        {
            // Expected exception
        }

        // Assert - verify error count is in log message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1 errors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ValidateGuid Tests

    [Fact]
    public async Task ValidateGuid_WithValidGuid_DoesNotThrow()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act & Assert - should not throw
        await _validator.ValidateGuid(validGuid);

        // Verify debug logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation successful")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateGuid_WithEmptyGuid_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _validator.ValidateGuid(Guid.Empty));

        Assert.Equal("externalUserId", exception.ParamName);
        Assert.Contains("External User ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ValidateGuid_WithEmptyGuid_LogsWarning()
    {
        // Act
        try
        {
            await _validator.ValidateGuid(Guid.Empty);
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }

        // Assert - verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("External User ID is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateGuid_WithValidGuid_LogsDebugWithGuidValue()
    {
        // Arrange
        var testGuid = Guid.NewGuid();

        // Act
        await _validator.ValidateGuid(testGuid);

        // Assert - verify debug log includes the GUID value
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(testGuid.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateGuid_MultipleValidCalls_EachLogsDebug()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Act
        await _validator.ValidateGuid(guid1);
        await _validator.ValidateGuid(guid2);

        // Assert - verify both calls logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation successful")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    #endregion
}
