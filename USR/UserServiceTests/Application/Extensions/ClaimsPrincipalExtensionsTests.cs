using System.Security.Claims;
using Aristotle.Application.Extensions;
using Xunit;

namespace UserService.UnitTests.Application.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    #region GetUserId Tests

    [Fact]
    public void GetUserId_WithNameIdentifierClaim_ReturnsGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUserId();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_WithSubClaim_ReturnsGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("sub", userId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUserId();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_WithBothClaims_PrefersNameIdentifier()
    {
        // Arrange
        var nameIdentifierId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, nameIdentifierId.ToString()),
            new("sub", subId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetUserId();

        // Assert
        Assert.Equal(nameIdentifierId, result);
    }

    [Fact]
    public void GetUserId_WithoutClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_WithInvalidGuid_ThrowsFormatException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act & Assert
        Assert.Throws<FormatException>(() => principal.GetUserId());
    }

    #endregion

    #region GetEmail Tests

    [Fact]
    public void GetEmail_WithEmailClaim_ReturnsEmail()
    {
        // Arrange
        const string email = "test@example.com";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetEmail();

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void GetEmail_WithLowercaseEmailClaim_ReturnsEmail()
    {
        // Arrange
        const string email = "test@example.com";
        var claims = new List<Claim>
        {
            new("email", email)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetEmail();

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void GetEmail_WithBothClaims_PrefersClaimTypesEmail()
    {
        // Arrange
        const string claimTypesEmail = "preferred@example.com";
        const string lowercaseEmail = "alternate@example.com";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, claimTypesEmail),
            new("email", lowercaseEmail)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetEmail();

        // Assert
        Assert.Equal(claimTypesEmail, result);
    }

    [Fact]
    public void GetEmail_WithoutClaim_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetEmail();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetName Tests

    [Fact]
    public void GetName_WithNameClaim_ReturnsName()
    {
        // Arrange
        const string name = "John Doe";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void GetName_WithPreferredUsernameClaim_ReturnsName()
    {
        // Arrange
        const string username = "johndoe";
        var claims = new List<Claim>
        {
            new("preferred_username", username)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Equal(username, result);
    }

    [Fact]
    public void GetName_WithLowercaseNameClaim_ReturnsName()
    {
        // Arrange
        const string name = "John Doe";
        var claims = new List<Claim>
        {
            new("name", name)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void GetName_WithMultipleClaims_PrefersClaimTypesName()
    {
        // Arrange
        const string claimTypesName = "Preferred Name";
        const string preferredUsername = "username";
        const string lowercaseName = "lowercase name";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, claimTypesName),
            new("preferred_username", preferredUsername),
            new("name", lowercaseName)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Equal(claimTypesName, result);
    }

    [Fact]
    public void GetName_WithoutNameButWithPreferredUsername_ReturnsPreferredUsername()
    {
        // Arrange
        const string preferredUsername = "username";
        var claims = new List<Claim>
        {
            new("preferred_username", preferredUsername),
            new("name", "lowercase name")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Equal(preferredUsername, result);
    }

    [Fact]
    public void GetName_WithoutClaim_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetName();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetGroups Tests

    [Fact]
    public void GetGroups_WithMultipleGroupClaims_ReturnsAllGroups()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("groups", "admin"),
            new("groups", "users"),
            new("groups", "moderators")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetGroups();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("admin", result);
        Assert.Contains("users", result);
        Assert.Contains("moderators", result);
    }

    [Fact]
    public void GetGroups_WithSingleGroupClaim_ReturnsSingleGroup()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("groups", "admin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.GetGroups();

        // Assert
        Assert.Single(result);
        Assert.Contains("admin", result);
    }

    [Fact]
    public void GetGroups_WithoutGroupClaims_ReturnsEmptyList()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetGroups();

        // Assert
        Assert.Empty(result);
    }

    #endregion
}