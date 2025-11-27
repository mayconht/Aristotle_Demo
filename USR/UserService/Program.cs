using Aristotle.Infrastructure.Config;
using Aristotle.Infrastructure.Extensions;
using Aristotle.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

DatabaseConfigurator.ConfigureDatabase(builder);

RegisterService.Initialize(builder);

builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddSwaggerDocumentation(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API v1");
        options.OAuthClientId(app.Configuration["Authentication:Keycloak:ClientId"]);
        options.OAuthClientSecret(app.Configuration["Authentication:Keycloak:ClientSecret"]);
        options.OAuthUsePkce();
    });
}

MiddlewareConfigurator.ConfigureMiddleware(app);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying database migrations...");
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw new InvalidOperationException("Failed to apply database migrations. See inner exception for details.", ex);
    }
}

app.Logger.LogInformation("UserService API starting...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

await app.RunAsync();

/// <summary>
///     Program class for test access
/// </summary>
public abstract partial class Program
{
    /// <summary>
    ///     Protected constructor for testing
    /// </summary>
    protected Program()
    {
    }
}