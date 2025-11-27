using System.Security.Claims;
using Aristotle.Application.Services;
using Aristotle.Application.Validators;
using Aristotle.Domain.Entities;
using Aristotle.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserService.UnitTests.Application.Service;

public class UserServiceProvisioningTests
{
    private readonly Mock<ILogger<Aristotle.Application.Services.UserService>> _loggerMock;
    private readonly Aristotle.Application.Services.UserService _userService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserValidator> _userValidatorMock;

    public UserServiceProvisioningTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userValidatorMock = new Mock<IUserValidator>();
        _loggerMock = new Mock<ILogger<Aristotle.Application.Services.UserService>>();
        _userService = new Aristotle.Application.Services.UserService(_userRepositoryMock.Object, _userValidatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Aristotle.Application.Services.UserService(null!, _userValidatorMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Aristotle.Application.Services.UserService(_userRepositoryMock.Object, _userValidatorMock.Object, null!));
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithUnauthenticatedPrincipal_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal();

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithNullPrincipal_ReturnsNull()
    {
        // Act
        var result = await _userService.GetOrProvisionUserAsync(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithExistingUser_ReturnsExistingUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var existingUser = new User
        {
            ExternalUserId = externalUserId
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, externalUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(externalUserId, result.ExternalUserId);
        _userRepositoryMock.Verify(r => r.GetByExternalUserIdAsync(externalUserId), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never); // Should NOT update
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never); // Should NOT create
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithNewUser_CreatesUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, externalUserId.ToString()),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(externalUserId, result.ExternalUserId);
        _userRepositoryMock.Verify(r => r.GetByExternalUserIdAsync(externalUserId), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.ExternalUserId == externalUserId
        )), Times.Once);
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WhenRepositoryThrows_ReturnsNull()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, externalUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithSubClaim_CreatesUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("sub", externalUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _userRepositoryMock
            .Setup(r => r.GetByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(externalUserId, result.ExternalUserId);
    }

    [Fact]
    public async Task GetOrProvisionUserAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _userService.GetOrProvisionUserAsync(principal);

        // Assert
        Assert.Null(result);
    }

}