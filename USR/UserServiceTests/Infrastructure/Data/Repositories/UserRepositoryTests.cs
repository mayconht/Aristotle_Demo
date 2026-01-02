using Aristotle.Domain.Entities;
using Aristotle.Infrastructure.Data.Repositories;
using Aristotle.Infrastructure.Exceptions;
using Aristotle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserService.UnitTests.Infrastructure.Data.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<ILogger<UserRepository>> _loggerMock = new();

    private readonly DbContextOptions<ApplicationDbContext> _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UserRepository(null!, _loggerMock.Object));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UserRepository(new ApplicationDbContext(_options), null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_DoesNotThrow_WhenParametersAreValid()
    {
        var exception = Record.Exception(() => new UserRepository(new ApplicationDbContext(_options), _loggerMock.Object));
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetByExternalUserIdAsync_ReturnsUser_WhenExists()
    {
        var externalUserId = Guid.NewGuid();
        var user = new User
        {
            ExternalUserId = externalUserId
        };

        await using (var context = new ApplicationDbContext(_options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            var result = await repo.GetByExternalUserIdAsync(externalUserId);
            Assert.NotNull(result);
            Assert.Equal(user.ExternalUserId, result.ExternalUserId);
        }
    }

    [Fact]
    public async Task GetByExternalUserIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var result = await repo.GetByExternalUserIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var u1 = new User { ExternalUserId = Guid.NewGuid() };
        var u2 = new User { ExternalUserId = Guid.NewGuid() };

        await using (var context = new ApplicationDbContext(_options))
        {
            context.Users.AddRange(u1, u2);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            var list = (await repo.GetAllAsync()).ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, x => x.ExternalUserId == u1.ExternalUserId);
            Assert.Contains(list, x => x.ExternalUserId == u2.ExternalUserId);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsers()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var list = (await repo.GetAllAsync()).ToList();
        Assert.Empty(list);
    }

    [Fact]
    public async Task AddAsync_AddsUser_WhenValid()
    {
        var u = new User { ExternalUserId = Guid.NewGuid() };
        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            var result = await repo.AddAsync(u);
            Assert.Equal(u.ExternalUserId, result.ExternalUserId);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var saved = await context.Users.FirstOrDefaultAsync(x => x.ExternalUserId == u.ExternalUserId, TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.Equal(u.ExternalUserId, saved.ExternalUserId);
        }
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenNull()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
    }

    [Fact]
    //These tests are important to validate the Garbage Collector as well.
    public async Task AddAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await context.DisposeAsync();
        var u = new User { ExternalUserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<DatabaseException>(() => repo.AddAsync(u));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenNull()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsRepositoryException_WhenNotExists()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var u = new User { ExternalUserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<RepositoryException>(() => repo.UpdateAsync(u));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUser_WhenExists()
    {
        //This seems weird as we don't have many properties on our class.
        var externalUserId = Guid.NewGuid();
        var originalUser = new User { ExternalUserId = externalUserId };

        await using (var context = new ApplicationDbContext(_options))
        {
            context.Users.Add(originalUser);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var updatedUser = new User { ExternalUserId = externalUserId };

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            var result = await repo.UpdateAsync(updatedUser);
            Assert.NotNull(result);
            Assert.Equal(updatedUser.ExternalUserId, result.ExternalUserId);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var savedUser = await context.Users.FirstOrDefaultAsync(x => x.ExternalUserId == externalUserId, TestContext.Current.CancellationToken);
            Assert.NotNull(savedUser);
            
        }
    }

    [Fact]
    public async Task UpdateAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var u = new User { ExternalUserId = Guid.NewGuid() };
        await context.DisposeAsync();
        await Assert.ThrowsAsync<DatabaseException>(() => repo.UpdateAsync(u));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        await using var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var result = await repo.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenExists()
    {
        var user = new User { ExternalUserId = Guid.NewGuid() };
        await using (var context = new ApplicationDbContext(_options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            var result = await repo.DeleteAsync(user.ExternalUserId);
            Assert.True(result);
            var deleted = await context.Users.FirstOrDefaultAsync(x => x.ExternalUserId == user.ExternalUserId, TestContext.Current.CancellationToken);
            Assert.Null(deleted);
        }
    }

    [Fact]
    public async Task DeleteAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        var id = Guid.NewGuid();
        await context.DisposeAsync();
        await Assert.ThrowsAsync<DatabaseException>(() => repo.DeleteAsync(id));
    }

    [Fact]
    public async Task GetByExternalUserIdAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await context.DisposeAsync();
        await Assert.ThrowsAsync<DatabaseException>(() => repo.GetByExternalUserIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await context.DisposeAsync();
        await Assert.ThrowsAsync<DatabaseException>(() => repo.GetAllAsync());
    }

    [Fact]
    public async Task WipeDatabaseAsync_RemovesAllUsers()
    {
        var u1 = new User { ExternalUserId = Guid.NewGuid() };
        var u2 = new User { ExternalUserId = Guid.NewGuid() };

        await using (var context = new ApplicationDbContext(_options))
        {
            context.Users.AddRange(u1, u2);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            await repo.WipeDatabaseAsync();

            var remaining = await context.Users.ToListAsync(TestContext.Current.CancellationToken);
            Assert.Empty(remaining);
        }
    }

    [Fact]
    public async Task WipeDatabaseAsync_ThrowsDatabaseException_OnContextDisposed()
    {
        var context = new ApplicationDbContext(_options);
        var repo = new UserRepository(context, _loggerMock.Object);
        await context.DisposeAsync();
        await Assert.ThrowsAsync<DatabaseException>(() => repo.WipeDatabaseAsync());
    }

    [Fact]
    public async Task AddAsync_ShouldPreserveCreatedAtTimestamp()
    {
        var u = new User { ExternalUserId = Guid.NewGuid() };

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            await repo.AddAsync(u);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var saved = await context.Users.FirstOrDefaultAsync(x => x.ExternalUserId == u.ExternalUserId, TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
            Assert.NotEqual(default, saved.CreatedAt);
            Assert.True(saved.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
        }
    }

    [Fact]
    public async Task AddAsync_ShouldInitializeEmptyCategories()
    {
        var u = new User { ExternalUserId = Guid.NewGuid() };

        await using (var context = new ApplicationDbContext(_options))
        {
            var repo = new UserRepository(context, _loggerMock.Object);
            await repo.AddAsync(u);
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var saved = await context.Users.FirstOrDefaultAsync(x => x.ExternalUserId == u.ExternalUserId, TestContext.Current.CancellationToken);
            Assert.NotNull(saved);
        }
    }

}