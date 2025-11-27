using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Aristotle.Application.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Aristotle.Infrastructure.Extensions;

/// <summary>
///     Service collection extension helpers
/// </summary>
public static class ServiceConfigurationExtensions
{
    private const string RolesClaimType = "roles";
    private const string RealmAccessClaimType = "realm_access";

    /// <summary>
    ///     Adds Swagger/OpenAPI configuration including OAuth2 definition.
    /// </summary>
    public static void AddSwaggerDocumentation(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });

            var authority = configuration["Authentication:Keycloak:Authority"];
            if (string.IsNullOrEmpty(authority))
                throw new InvalidOperationException("Keycloak Authority configuration is missing. Please configure 'Authentication:Keycloak:Authority' in appsettings.json");

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authority.TrimEnd('/') + "/protocol/openid-connect/auth"),
                        TokenUrl = new Uri(authority.TrimEnd('/') + "/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect" },
                            { "profile", "User profile" },
                            { "email", "User email" },
                            { "roles", "Realm roles" }
                        }
                    }
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    ["openid", "profile", "email", "roles"]
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    /// <summary>
    ///     Adds authentication and authorization (policies) configuration.
    /// </summary>
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        const string sectionBase = "Authentication:Keycloak";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => { ConfigureJwtBearerOptions(options, configuration, sectionBase); });

        ConfigureAuthorizationPolicies(services);
    }

    /// <summary>
    ///     Configures JWT Bearer options from configuration.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionBase"></param>
    private static void ConfigureJwtBearerOptions(JwtBearerOptions options, IConfiguration configuration, string sectionBase)
    {
        options.Authority = configuration[$"{sectionBase}:Authority"];
        options.Audience = configuration[$"{sectionBase}:Audience"];
        options.RequireHttpsMetadata = configuration.GetValue<bool>($"{sectionBase}:RequireHttpsMetadata");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = configuration.GetValue<bool>($"{sectionBase}:ValidateIssuer"),
            ValidateAudience = configuration.GetValue<bool>($"{sectionBase}:ValidateAudience"),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuers = new[]
            {
                configuration[$"{sectionBase}:Authority"],
                "http://localhost:8080/realms/userservice" //TODO move to config
            }
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = HandleTokenValidation,
            OnAuthenticationFailed = HandleAuthenticationFailure
        };
    }

    /// <summary>
    ///     Handles token validation event to process custom claims.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static Task HandleTokenValidation(TokenValidatedContext ctx)
    {
        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtEvents");

        if (ctx.Principal == null) return Task.CompletedTask;

        ProcessRealmAccessClaims(ctx.Principal, logger);
        return Task.CompletedTask;
    }


    /// <summary>
    ///     Processes the realm_access claim to extract roles and add them as individual role claims.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="logger"></param>
    private static void ProcessRealmAccessClaims(ClaimsPrincipal principal, ILogger logger)
    {
        var identity = principal.Identities.First();
        var realmAccessClaim = principal.FindFirst(RealmAccessClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(realmAccessClaim)) return;

        try
        {
            AddRoleClaims(principal, identity, realmAccessClaim);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse realm_access.roles claim as JSON. Value: {RealmAccessClaim}", realmAccessClaim);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "realm_access.roles claim JSON did not contain a 'roles' array property. Value: {RealmAccessClaim}", realmAccessClaim);
        }
    }

    /// <summary>
    ///     Adds individual role claims from the realm_access.roles JSON array.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="identity"></param>
    /// <param name="realmAccessClaim"></param>
    private static void AddRoleClaims(ClaimsPrincipal principal, ClaimsIdentity identity, string realmAccessClaim)
    {
        using var doc = JsonDocument.Parse(realmAccessClaim);

        if (!doc.RootElement.TryGetProperty(RolesClaimType, out var rolesElement) ||
            rolesElement.ValueKind != JsonValueKind.Array)
            return;

        var rolesToAdd = rolesElement.EnumerateArray()
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Where(r => !principal.Claims.Any(c => c.Type == RolesClaimType && c.Value == r));

        foreach (var role in rolesToAdd) identity.AddClaim(new Claim(RolesClaimType, role!));
    }

    /// <summary>
    ///     Handles authentication failure events by logging the exception.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static Task HandleAuthenticationFailure(AuthenticationFailedContext ctx)
    {
        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtEvents");
        logger.LogError(ctx.Exception, "JWT authentication failed: {Message}", ctx.Exception.Message);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Configures authorization policies.
    /// </summary>
    /// <param name="services"></param>
    private static void ConfigureAuthorizationPolicies(IServiceCollection services)
    {
        var claimTypes = new[] { "groups", RolesClaimType, "role" };

        services.AddAuthorizationBuilder().AddPolicy(Policies.RequireAdminRole, policy =>
            policy.RequireAssertion(ctx =>
                HasAnyClaimValue(ctx.User, claimTypes, RolesConfiguration.Admin)));
    }

    /// <summary>
    ///     Verifies if the user has any of the required claim values in the specified claim types.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="types"></param>
    /// <param name="required"></param>
    /// <returns></returns>
    private static bool HasAnyClaimValue(ClaimsPrincipal user, string[] types, params string[] required)
    {
        var values = user.Claims
            .Where(c => types.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return required.Any(values.Contains);
    }


}