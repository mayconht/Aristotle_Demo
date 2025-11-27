using Aristotle.Domain.Entities;
using Aristotle.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Aristotle.Infrastructure.Persistence;

/// <summary>
///     Context class for the application.
///     Coordinates Entity Framework Core functionality for the User entity.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    ///     Constructor for the ApplicationDbContext class.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    ///     DbSet for the User entity.
    ///     Represents the Users table in the database.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    ///     Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder being used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}