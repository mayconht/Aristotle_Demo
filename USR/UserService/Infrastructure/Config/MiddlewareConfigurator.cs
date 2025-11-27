using System.Text.Json;
using Aristotle.Infrastructure.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Aristotle.Infrastructure.Config;

/// <summary>
///     Middleware configuration helper class.
/// </summary>
public static class MiddlewareConfigurator
{
    /// <summary>
    ///     Configures the middleware for the application, including exception handling, HTTPS redirection, controllers, and
    ///     health checks.
    /// </summary>
    /// <param name="internalApp"></param>
    public static void ConfigureMiddleware(WebApplication internalApp)
    {
        // Redirect root requests to Swagger UI
        internalApp.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger");
                return;
            }

            await next();
        });
        internalApp.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        var enableHttps = Environment.GetEnvironmentVariable("ENABLE_HTTPS_REDIRECT");
        if (string.Equals(enableHttps, "true", StringComparison.OrdinalIgnoreCase)) internalApp.UseHttpsRedirection();

        internalApp.UseAuthentication();
        internalApp.UseMiddleware<JitUserProvisioningMiddleware>();
        internalApp.UseAuthorization();

        internalApp.MapControllers();
        // TODO: Should the health endpoint live here or in API gateway?
        internalApp.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        error = e.Value.Exception?.Message
                    }),
                    duration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });
    }
}