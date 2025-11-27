using System.Security.Claims;

namespace UserService.UnitTests.Builders;

/// <summary>
/// Builder for creating ClaimsPrincipal objects for testing purposes.
/// </summary>
public class ClaimsPrincipalBuilder
{
    private readonly List<Claim> _claims = new();
    private string _authenticationType = "TestAuth";

    /// <summary>
    /// Sets the subject (sub) claim with a GUID value.
    /// </summary>
    public ClaimsPrincipalBuilder WithSubject(Guid externalUserId)
    {
        _claims.Add(new Claim("sub", externalUserId.ToString()));
        return this;
    }

    /// <summary>
    /// Sets the NameIdentifier claim with a GUID value.
    /// </summary>
    public ClaimsPrincipalBuilder WithNameIdentifier(Guid externalUserId)
    {
        _claims.Add(new Claim(ClaimTypes.NameIdentifier, externalUserId.ToString()));
        return this;
    }

    /// <summary>
    /// Sets the email claim.
    /// </summary>
    public ClaimsPrincipalBuilder WithEmail(string email)
    {
        _claims.Add(new Claim(ClaimTypes.Email, email));
        _claims.Add(new Claim("email", email)); // Also add lowercase version for JWT compatibility
        return this;
    }

    /// <summary>
    /// Sets the name claim.
    /// </summary>
    public ClaimsPrincipalBuilder WithName(string name)
    {
        _claims.Add(new Claim(ClaimTypes.Name, name));
        _claims.Add(new Claim("name", name)); // Also add lowercase version for JWT compatibility
        return this;
    }

    /// <summary>
    /// Adds a group claim.
    /// </summary>
    public ClaimsPrincipalBuilder WithGroup(string group)
    {
        _claims.Add(new Claim("groups", group));
        return this;
    }

    /// <summary>
    /// Adds multiple group claims.
    /// </summary>
    public ClaimsPrincipalBuilder WithGroups(params string[] groups)
    {
        foreach (var group in groups)
        {
            _claims.Add(new Claim("groups", group));
        }
        return this;
    }

    /// <summary>
    /// Adds a role claim.
    /// </summary>
    public ClaimsPrincipalBuilder WithRole(string role)
    {
        _claims.Add(new Claim("roles", role));
        return this;
    }

    /// <summary>
    /// Adds multiple role claims.
    /// </summary>
    public ClaimsPrincipalBuilder WithRoles(params string[] roles)
    {
        foreach (var role in roles)
        {
            _claims.Add(new Claim("roles", role));
        }
        return this;
    }

    /// <summary>
    /// Adds a "role" claim (singular form).
    /// </summary>
    public ClaimsPrincipalBuilder WithRoleSingular(string role)
    {
        _claims.Add(new Claim("role", role));
        return this;
    }

    /// <summary>
    /// Adds a custom claim.
    /// </summary>
    public ClaimsPrincipalBuilder WithClaim(string type, string value)
    {
        _claims.Add(new Claim(type, value));
        return this;
    }

    /// <summary>
    /// Sets a custom authentication type (default is "TestAuth").
    /// </summary>
    public ClaimsPrincipalBuilder WithAuthenticationType(string authenticationType)
    {
        _authenticationType = authenticationType;
        return this;
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with an admin user (includes ExternalUserId, Email, Name, and Admin role/group).
    /// </summary>
    public static ClaimsPrincipalBuilder CreateAdmin(Guid? externalUserId = null, string? email = null, string? name = null)
    {
        var id = externalUserId ?? Guid.NewGuid();
        var builder = new ClaimsPrincipalBuilder()
            .WithSubject(id)
            .WithNameIdentifier(id)
            .WithEmail(email ?? "admin@example.com")
            .WithName(name ?? "Admin User")
            .WithGroup("admin-group")
            .WithRole("Admins");

        return builder;
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with a regular user (includes ExternalUserId, Email, Name, and User role/group).
    /// </summary>
    public static ClaimsPrincipalBuilder CreateUser(Guid? externalUserId = null, string? email = null, string? name = null)
    {
        var id = externalUserId ?? Guid.NewGuid();
        var builder = new ClaimsPrincipalBuilder()
            .WithSubject(id)
            .WithNameIdentifier(id)
            .WithEmail(email ?? "user@example.com")
            .WithName(name ?? "Regular User")
            .WithGroup("user-group")
            .WithRole("Users");

        return builder;
    }

    /// <summary>
    /// Builds the ClaimsPrincipal with all configured claims.
    /// </summary>
    public ClaimsPrincipal Build()
    {
        var identity = new ClaimsIdentity(_claims, _authenticationType);
        return new ClaimsPrincipal(identity);
    }
}
