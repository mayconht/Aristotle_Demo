using Aristotle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aristotle.Infrastructure.Config;

/// <summary>
///     Database configuration helper class for PostgreSQL.
/// </summary>
public static class DatabaseConfigurator
{
    /// <summary>
    ///     Configures PostgreSQL database from connection string.
    /// </summary>
    /// <param name="internalBuilder">The web application builder.</param>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing or invalid.</exception>
    public static void ConfigureDatabase(WebApplicationBuilder internalBuilder)
    {
        var connectionString = internalBuilder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Missing connection string for DefaultConnection.");

        internalBuilder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (internalBuilder.Environment.IsDevelopment()) options.EnableSensitiveDataLogging();
        });

        Console.WriteLine("Using PostgreSQL database.");
    }
}