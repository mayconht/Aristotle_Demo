using System.Security.Claims;
using Aristotle.Application.DTOs;
using Aristotle.Application.Services;
using Aristotle.Controllers;
using Aristotle.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.UnitTests.Builders;
using Xunit;

namespace UserService.UnitTests.Api.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UserController>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UserController>>();
        _mapperMock = new Mock<IMapper>();
        _controller = new UserController(_userServiceMock.Object, _loggerMock.Object, _mapperMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var controller = new UserController(_userServiceMock.Object, _loggerMock.Object, _mapperMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserController(null!, _loggerMock.Object, _mapperMock.Object));

        Assert.Equal("userService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserController(_userServiceMock.Object, null!, _mapperMock.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserController(_userServiceMock.Object, _loggerMock.Object, null!));

        Assert.Equal("mapper", exception.ParamName);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithExistingUser_ReturnsOkWithUserAndClaims()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var user = new User
        {
            ExternalUserId = externalUserId,
            CreatedAt = DateTime.UtcNow
        };
        var userDto = new UserResponseDto
        {
            ExternalUserId = externalUserId,
            CreatedAt = user.CreatedAt
        };

        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(externalUserId, "test@example.com", "Test User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserResponseDto>(user))
            .Returns(userDto);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert //TODO Change this to verify lib, it is faster and more reliable
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        var resultType = okResult.Value.GetType();
        var claimsProperty = resultType.GetProperty("claims");
        var userProperty = resultType.GetProperty("user");

        Assert.NotNull(claimsProperty);
        Assert.NotNull(userProperty);

        var claimsValue = claimsProperty.GetValue(okResult.Value);
        var userValue = userProperty.GetValue(okResult.Value);

        Assert.NotNull(claimsValue);
        Assert.NotNull(userValue);

        _userServiceMock.Verify(s => s.GetUserByExternalUserIdAsync(externalUserId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserResponseDto>(user), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_WithNonExistingUser_ReturnsOkWithClaimsAndNullUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateUser(externalUserId, "test@example.com", "Test User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        Assert.NotNull(okResult.Value);
        var resultType = okResult.Value.GetType();
        var claimsProperty = resultType.GetProperty("claims");
        var userProperty = resultType.GetProperty("user");

        Assert.NotNull(claimsProperty);
        Assert.NotNull(userProperty);

        var claimsValue = claimsProperty.GetValue(okResult.Value);
        var userValue = userProperty.GetValue(okResult.Value);

        Assert.NotNull(claimsValue);
        Assert.Null(userValue);

        _userServiceMock.Verify(s => s.GetUserByExternalUserIdAsync(externalUserId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserResponseDto>(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidClaimsButNoUserInDb_ReturnsOkWithClaimsAndNullUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var principal = new ClaimsPrincipalBuilder()
            .WithNameIdentifier(externalUserId)
            .WithEmail("test@example.com")
            .WithName("Test User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verify result structure
        Assert.NotNull(okResult.Value);
        var resultType = okResult.Value.GetType();
        var claimsProperty = resultType.GetProperty("claims");
        var userProperty = resultType.GetProperty("user");

        Assert.NotNull(claimsProperty);
        Assert.NotNull(userProperty);

        var claimsValue = claimsProperty.GetValue(okResult.Value);
        var userValue = userProperty.GetValue(okResult.Value);

        Assert.NotNull(claimsValue);
        Assert.Null(userValue);

        _userServiceMock.Verify(s => s.GetUserByExternalUserIdAsync(externalUserId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_ExtractsAllClaimTypes_Successfully()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var principal = new ClaimsPrincipalBuilder()
            .WithSubject(externalUserId)
            .WithEmail("test@example.com")
            .WithName("Test User")
            .WithGroups("group1", "group2")
            .WithRole("role1")
            .WithRoleSingular("role2")
            .WithClaim("custom", "customValue")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Verify result structure
        Assert.NotNull(okResult.Value);
        var resultType = okResult.Value.GetType();
        var claimsProperty = resultType.GetProperty("claims");

        Assert.NotNull(claimsProperty);

        var claimsValue = claimsProperty.GetValue(okResult.Value);
        Assert.NotNull(claimsValue);

        // Verify the claims object has expected properties
        var claimsType = claimsValue!.GetType();
        var allProperty = claimsType.GetProperty("all");
        Assert.NotNull(allProperty);
    }

    #endregion

    #region GetUserByExternalUserId Tests

    [Fact]
    public async Task GetUserByExternalUserId_WithExistingUser_ReturnsOkWithUser()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();
        var user = new User
        {
            ExternalUserId = externalUserId,
            CreatedAt = DateTime.UtcNow
        };
        var userDto = new UserResponseDto
        {
            ExternalUserId = externalUserId,
            CreatedAt = user.CreatedAt
        };

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserResponseDto>(user))
            .Returns(userDto);

        // Act
        var result = await _controller.GetUserByExternalUserId(externalUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(userDto, okResult.Value);

        _userServiceMock.Verify(s => s.GetUserByExternalUserIdAsync(externalUserId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserResponseDto>(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByExternalUserId_WithNonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var externalUserId = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.GetUserByExternalUserIdAsync(externalUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserByExternalUserId(externalUserId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);

        _userServiceMock.Verify(s => s.GetUserByExternalUserIdAsync(externalUserId), Times.Once);
        _mapperMock.Verify(m => m.Map<UserResponseDto>(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_WithMultipleUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };

        var userDtos = users.Select(u => new UserResponseDto
        {
            ExternalUserId = u.ExternalUserId,
            CreatedAt = u.CreatedAt
        }).ToList();

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponseDto>>(users))
            .Returns(userDtos);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(userDtos, okResult.Value);

        _userServiceMock.Verify(s => s.GetAllUsersAsync(), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<UserResponseDto>>(users), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WithNoUsers_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyUserList = new List<User>();
        var emptyDtoList = new List<UserResponseDto>();

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(emptyUserList);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponseDto>>(It.IsAny<IEnumerable<User>>()))
            .Returns(emptyDtoList);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var resultValue = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(okResult.Value);
        Assert.Empty(resultValue);

        _userServiceMock.Verify(s => s.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_LogsInformationMessages()
    {
        // Arrange
        var users = new List<User>
        {
            new() { ExternalUserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        var userDtos = new List<UserResponseDto>
        {
            new() { ExternalUserId = users[0].ExternalUserId, CreatedAt = users[0].CreatedAt }
        };

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponseDto>>(users))
            .Returns(userDtos);

        // Act
        await _controller.GetAllUsers();

        // Assert - Verify information logs were called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region WipeDatabase Tests

    [Fact]
    public async Task WipeDatabase_InDevelopmentEnvironment_ReturnsNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(adminId, "admin@example.com", "Admin User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Set environment to Development
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _userServiceMock
            .Setup(s => s.WipeDatabaseAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.WipeDatabase();

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);

        _userServiceMock.Verify(s => s.WipeDatabaseAsync(), Times.Once);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task WipeDatabase_InProductionEnvironment_ReturnsForbidden()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(adminId, "admin@example.com", "Admin User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Set environment to Production
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        // Act
        var result = await _controller.WipeDatabase();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);

        // Verify the error message using reflection
        Assert.NotNull(objectResult.Value);
        var resultType = objectResult.Value.GetType();
        var messageProperty = resultType.GetProperty("Message");

        Assert.NotNull(messageProperty);
        var messageValue = messageProperty.GetValue(objectResult.Value) as string;

        Assert.NotNull(messageValue);
        Assert.Contains("only allowed in development", messageValue.ToLower());

        _userServiceMock.Verify(s => s.WipeDatabaseAsync(), Times.Never);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task WipeDatabase_InStagingEnvironment_ReturnsForbidden()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(adminId, "admin@example.com", "Admin User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Set environment to Staging
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");

        // Act
        var result = await _controller.WipeDatabase();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);

        _userServiceMock.Verify(s => s.WipeDatabaseAsync(), Times.Never);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task WipeDatabase_InDevelopment_LogsAuditWarningBeforeAndAfter()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(adminId, "admin@example.com", "Admin User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _userServiceMock
            .Setup(s => s.WipeDatabaseAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _controller.WipeDatabase();

        // Assert - Verify warning log was called for COMPLETED
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AUDIT") && v.ToString()!.Contains("COMPLETED")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task WipeDatabase_InProduction_LogsAuditWarningForDenied()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var principal = ClaimsPrincipalBuilder
            .CreateAdmin(adminId, "admin@example.com", "Admin User")
            .Build();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        // Act
        await _controller.WipeDatabase();

        // Assert - Verify warning log was called for DENIED
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AUDIT") && v.ToString()!.Contains("DENIED")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    #endregion
}
