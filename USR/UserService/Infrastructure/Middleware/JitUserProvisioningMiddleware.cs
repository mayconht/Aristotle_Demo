using Aristotle.Application.Services;

namespace Aristotle.Infrastructure.Middleware;

/// <summary>
///     Middleware for Just-In-Time (JIT) user provisioning.
///     Automatically provisions users from Keycloak on first login.
/// </summary>
public class JitUserProvisioningMiddleware
{
    private readonly ILogger<JitUserProvisioningMiddleware> _logger;
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Initializes a new instance of the JitUserProvisioningMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger for middleware operations.</param>
    public JitUserProvisioningMiddleware(RequestDelegate next, ILogger<JitUserProvisioningMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    ///     Invokes the middleware to provision users automatically on first login.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="userService">The user service (injected per request).</param>
    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userService.GetOrProvisionUserAsync(context.User);
            if (user != null)
            {
                context.Items["ProvisionedUser"] = user;
                _logger.LogDebug("User {ExternalUserId} provisioned in request context", user.ExternalUserId);
            }
        }

        await _next(context);
    }
}