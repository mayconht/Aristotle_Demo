using Aristotle.Application.Exceptions;
using Aristotle.Application.Validators;
using Aristotle.Domain.Entities;
using Aristotle.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserService.UnitTests.Application.Service;

/// <summary>
/// Tests for UserService GET operations (GetUserByExternalUserIdAsync and GetAllUsersAsync)
/// </summary>
public class UserServiceGetOperationsTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserValidator> _userValidatorMock;
    private readonly Mock<ILogger<Aristotle.Application.Services.UserService>> _loggerMock;
    private readonly Aristotle.Application.Services.UserService _userService;

    public UserServiceGetOperationsTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userValidatorMock = new Mock<IUserValidator>();
        _loggerMock = new Mock<ILogger<Aristotle.Application.Services.UserService>>();
        _userService = new Aristotle.Application.Services.UserService(
            _userRepositoryMock.Object,
            _userValidatorMock.Object,
            _loggerMock.Object);
    }

    #region GetUserByExternalUserIdAsync Tests

    [Fact]
    public async Task GetUserByExternalUserIdAsync_WithValidGuid_ReturnsUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var expectedUser = new User
        {
            ExternalUserId = externalUserId,
            CreatedAt = DateTime.UtcNow
        };

        _userValidatorMock
            .Setup(v => v.ValidateGuid(externalUserId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByExternalUserIdAsync(externalUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(externalUserId, result.ExternalUserId);
        _userValidatorMock.Verify(v => v.ValidateGuid(externalUserId), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByExternalUserIdAsync(externalUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserByExternalUserIdAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();

        _userValidatorMock
            .Setup(v => v.ValidateGuid(externalUserId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByExternalUserIdAsync(externalUserId);

        // Assert
        Assert.Null(result);
        _userRepositoryMock.Verify(r => r.GetByExternalUserIdAsync(externalUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserByExternalUserIdAsync_WithEmptyGuid_ThrowsArgumentNullException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        _userValidatorMock
            .Setup(v => v.ValidateGuid(emptyGuid))
            .ThrowsAsync(new ArgumentNullException($"externalUserId", "External User ID cannot be empty."));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _userService.GetUserByExternalUserIdAsync(emptyGuid));

        _userValidatorMock.Verify(v => v.ValidateGuid(emptyGuid), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByExternalUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByExternalUserIdAsync_WhenRepositoryThrows_ThrowsServiceOperationException()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var repositoryException = new InvalidOperationException("Database connection failed");

        _userValidatorMock
            .Setup(v => v.ValidateGuid(externalUserId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceOperationException>(
            () => _userService.GetUserByExternalUserIdAsync(externalUserId));

        Assert.Equal(nameof(Aristotle.Application.Services.UserService), exception.Service);
        Assert.Equal(nameof(Aristotle.Application.Services.UserService.GetUserByExternalUserIdAsync), exception.Operation);
        Assert.Contains("An error occurred while retrieving the user by external user ID", exception.Message);
        Assert.Equal(repositoryException, exception.InnerException);
    }

    [Fact]
    public async Task GetUserByExternalUserIdAsync_WhenRepositoryThrows_LogsError()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var repositoryException = new InvalidOperationException("Database connection failed");

        _userValidatorMock
            .Setup(v => v.ValidateGuid(externalUserId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ThrowsAsync(repositoryException);

        // Act
        try
        {
            await _userService.GetUserByExternalUserIdAsync(externalUserId);
        }
        catch (ServiceOperationException)
        {
            // Expected exception
        }

        // Assert - verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting user")),
                It.Is<Exception>(ex => ex == repositoryException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByExternalUserIdAsync_LogsDebugMessage()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var user = new User { ExternalUserId = externalUserId };

        _userValidatorMock
            .Setup(v => v.ValidateGuid(externalUserId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync(user);

        // Act
        await _userService.GetUserByExternalUserIdAsync(externalUserId);

        // Assert - verify debug log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting user by external user ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WithMultipleUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        var usersList = result.ToList();
        Assert.Equal(3, usersList.Count);
        Assert.Equal(users[0].ExternalUserId, usersList[0].ExternalUserId);
        Assert.Equal(users[1].ExternalUserId, usersList[1].ExternalUserId);
        Assert.Equal(users[2].ExternalUserId, usersList[2].ExternalUserId);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.Empty(result);
        _userRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_LogsInformationMessage()
    {
        // Arrange
        var users = new List<User>
        {
            new() { ExternalUserId = Guid.NewGuid() }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        await _userService.GetAllUsersAsync();

        // Assert - verify information log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting all users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_LogsDebugWithUserCount()
    {
        // Arrange
        var users = new List<User>
        {
            new() { ExternalUserId = Guid.NewGuid() },
            new() { ExternalUserId = Guid.NewGuid() },
            new() { ExternalUserId = Guid.NewGuid() }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        await _userService.GetAllUsersAsync();

        // Assert - verify debug log with count
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved") && v.ToString()!.Contains("3")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithEmptyList_LogsZeroCount()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        await _userService.GetAllUsersAsync();

        // Assert - verify debug log shows 0 users
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved") && v.ToString()!.Contains("0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region WipeDatabaseAsync Tests

    [Fact]
    public async Task WipeDatabaseAsync_CallsRepositoryMethod()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.WipeDatabaseAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _userService.WipeDatabaseAsync();

        // Assert
        _userRepositoryMock.Verify(r => r.WipeDatabaseAsync(), Times.Once);
    }

    [Fact]
    public async Task WipeDatabaseAsync_LogsWarningMessage()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.WipeDatabaseAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _userService.WipeDatabaseAsync();

        // Assert - verify warning log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Wiping all data") && v.ToString()!.Contains("irreversible")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
