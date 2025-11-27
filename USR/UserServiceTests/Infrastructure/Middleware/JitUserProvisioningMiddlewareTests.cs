using System.Security.Claims;
using Aristotle.Application.Services;
using Aristotle.Domain.Entities;
using Aristotle.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserService.UnitTests.Infrastructure.Middleware;

/// <summary>
///     Unit tests for JitUserProvisioningMiddleware
/// </summary>
public class JitUserProvisioningMiddlewareTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<ILogger<JitUserProvisioningMiddleware>> _loggerMock;
    private readonly JitUserProvisioningMiddleware _middleware;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<IUserService> _userServiceMock;

    public JitUserProvisioningMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<JitUserProvisioningMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _userServiceMock = new Mock<IUserService>();
        _middleware = new JitUserProvisioningMiddleware(_nextMock.Object, _loggerMock.Object);
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAuthenticated_ShouldProvisionUser()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        var provisionedUser = new User
        {
            ExternalUserId = Guid.NewGuid()
        };

        _userServiceMock.Setup(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(provisionedUser);

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _userServiceMock.Verify(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);

        Assert.NotNull(_httpContext.Items["ProvisionedUser"]);
        Assert.Equal(provisionedUser, _httpContext.Items["ProvisionedUser"]);

        _nextMock.Verify(x => x(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAuthenticated_ShouldLogDebug()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        var userId2 = Guid.NewGuid();
        var provisionedUser = new User
        {
            ExternalUserId = userId2
        };

        _userServiceMock.Setup(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(provisionedUser);

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId2.ToString()) && v.ToString()!.Contains("provisioned")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_ShouldNotProvisionUser()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _userServiceMock.Verify(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);

        Assert.False(_httpContext.Items.ContainsKey("ProvisionedUser"));
        _nextMock.Verify(x => x(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIdentityIsNull_ShouldNotProvisionUser()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(); // No identity

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _userServiceMock.Verify(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);

        Assert.False(_httpContext.Items.ContainsKey("ProvisionedUser"));
        _nextMock.Verify(x => x(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenProvisioningReturnsNull_ShouldNotAddUserToContext()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        _userServiceMock.Setup(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((User?)null);

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _userServiceMock.Verify(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);

        Assert.False(_httpContext.Items.ContainsKey("ProvisionedUser"));
        _nextMock.Verify(x => x(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenProvisioningReturnsNull_ShouldNotLogDebug()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        _userServiceMock.Setup(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((User?)null);

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAlwaysCallNextDelegate()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        // Act
        await _middleware.InvokeAsync(_httpContext, _userServiceMock.Object);

        // Assert
        _nextMock.Verify(x => x(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenProvisioningThrowsException_ShouldPropagateException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _httpContext.User = claimsPrincipal;

        var expectedException = new InvalidOperationException("Provisioning failed");
        _userServiceMock
            .Setup(x => x.GetOrProvisionUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext, _userServiceMock.Object));

        Assert.Equal("Provisioning failed", exception.Message);
        _nextMock.Verify(x => x(_httpContext), Times.Never);
    }
}