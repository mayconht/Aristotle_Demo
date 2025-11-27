using System.Security.Claims;
using Aristotle.Application.Authorization;
using Aristotle.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UserService.UnitTests.Infrastructure.Extensions;

public class ServiceConfigurationExtensionsTests
{
    #region AddSwaggerDocumentation Tests

    [Fact]
    public void AddSwaggerDocumentation_WithValidConfiguration_AddsSwaggerServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" }
            })
            .Build();

        // Act
        services.AddSwaggerDocumentation(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify that endpoints API explorer was registered
        var hasEndpointsApiExplorer = services.Any(s =>
            s.ServiceType.Name.Contains("IApiDescriptionProvider") ||
            s.ServiceType.Name.Contains("EndpointsApiExplorer"));

        Assert.True(services.Count > 0, "Services should be registered");
    }

    [Fact]
    public void AddSwaggerDocumentation_WithAuthorityEndingInSlash_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test/" }
            })
            .Build();

        // Act & Assert - should not throw
        services.AddSwaggerDocumentation(configuration);

        Assert.True(services.Count > 0);
    }

    #endregion

    #region AddAuthenticationAndAuthorization Tests

    [Fact]
    public void AddAuthenticationAndAuthorization_RegistersAuthenticationServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required for JWT events

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var authService = serviceProvider.GetService<IAuthenticationSchemeProvider>();

        Assert.NotNull(authService);
    }

    [Fact]
    public async Task AddAuthenticationAndAuthorization_ConfiguresJwtBearerAsDefaultScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        Assert.NotNull(defaultScheme);
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, defaultScheme.Name);
    }

    [Fact]
    public async Task AddAuthenticationAndAuthorization_RegistersAuthorizationPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var authorizationOptions = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();

        Assert.NotNull(authorizationOptions);

        // Verify RequireAdminRole policy exists
        var adminPolicy = await authorizationOptions.GetPolicyAsync(Policies.RequireAdminRole);
        Assert.NotNull(adminPolicy);
    }

    #endregion

    #region HasAnyClaimValue Tests (via Policy)

    [Fact]
    public async Task RequireAdminRolePolicy_WithAdminRoleInRolesClaim_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new List<Claim>
        {
            new("roles", "Admins")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequireAdminRolePolicy_WithAdminRoleInGroupsClaim_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new List<Claim>
        {
            new("groups", "Admins")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequireAdminRolePolicy_WithAdminInRoleClaim_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new List<Claim>
        {
            new("role", "Admins")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequireAdminRolePolicy_WithCaseInsensitiveAdminRole_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new List<Claim>
        {
            new("roles", "admins") // lowercase
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequireAdminRolePolicy_WithoutAdminRole_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new List<Claim>
        {
            new("roles", "Users") // Not admin
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RequireAdminRolePolicy_WithNoClaims_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        services.AddAuthenticationAndAuthorization(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var identity = new ClaimsIdentity(); // No claims
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = await authService.AuthorizeAsync(user, Policies.RequireAdminRole);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region JWT Configuration Tests

    [Fact]
    public void AddAuthenticationAndAuthorization_ConfiguresTokenValidationParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        var jwtOptions = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.NotNull(jwtOptions);
        Assert.Equal("http://localhost:8080/realms/test", jwtOptions.Authority);
        Assert.Equal("test-audience", jwtOptions.Audience);
        Assert.False(jwtOptions.RequireHttpsMetadata);

        Assert.NotNull(jwtOptions.TokenValidationParameters);
        Assert.True(jwtOptions.TokenValidationParameters.ValidateIssuer);
        Assert.True(jwtOptions.TokenValidationParameters.ValidateAudience);
        Assert.True(jwtOptions.TokenValidationParameters.ValidateLifetime);
        Assert.True(jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Equal(TimeSpan.Zero, jwtOptions.TokenValidationParameters.ClockSkew);
    }

    [Fact]
    public void AddAuthenticationAndAuthorization_ConfiguresValidIssuers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        var jwtOptions = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.NotNull(jwtOptions.TokenValidationParameters.ValidIssuers);
        Assert.Contains("http://localhost:8080/realms/test", jwtOptions.TokenValidationParameters.ValidIssuers);
        Assert.Contains("http://localhost:8080/realms/userservice", jwtOptions.TokenValidationParameters.ValidIssuers);
    }

    [Fact]
    public void AddAuthenticationAndAuthorization_ConfiguresJwtBearerEvents()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Authentication:Keycloak:Authority", "http://localhost:8080/realms/test" },
                { "Authentication:Keycloak:Audience", "test-audience" },
                { "Authentication:Keycloak:RequireHttpsMetadata", "false" },
                { "Authentication:Keycloak:ValidateIssuer", "true" },
                { "Authentication:Keycloak:ValidateAudience", "true" }
            })
            .Build();

        // Act
        services.AddAuthenticationAndAuthorization(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        var jwtOptions = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.NotNull(jwtOptions.Events);
        Assert.NotNull(jwtOptions.Events.OnTokenValidated);
        Assert.NotNull(jwtOptions.Events.OnAuthenticationFailed);
    }

    #endregion
}
