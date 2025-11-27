using Aristotle.Application;
using Aristotle.Application.Services;
using Aristotle.Application.Validators;
using Aristotle.Domain.Interfaces;
using Aristotle.Infrastructure.Data.Repositories;
using Aristotle.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aristotle.Infrastructure.Config;

/// <summary>
///     Service registration helper class.
/// </summary>
public static class RegisterService
{
    /// <summary>
    ///     Registers application services and repositories for dependency injection.
    /// </summary>
    /// <param name="internalBuilder"></param>
    public static void Initialize(WebApplicationBuilder internalBuilder)
    {
        internalBuilder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        internalBuilder.Services.AddScoped<IUserRepository, UserRepository>();
        internalBuilder.Services.AddScoped<IUserValidator, UserValidator>();
        internalBuilder.Services.AddScoped<IUserService, UserService>();

        internalBuilder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        internalBuilder.Services.AddHealthChecks()
            .AddCheck("Liveness", () => HealthCheckResult.Healthy("Service is running")) //TODO: Add a proper liveness check 
            .AddCheck<DatabaseHealthCheck>("Database")
            .AddNpgSql(internalBuilder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty, name: "PostgreSQL");
    }
}