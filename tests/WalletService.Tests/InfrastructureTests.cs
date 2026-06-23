using Microsoft.EntityFrameworkCore;
using WalletService.Domain;
using WalletService.Infrastructure;

namespace WalletService.Tests;

public class WalletDbContextTests
{
    private static WalletDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new WalletDbContext(options);
    }

    [Fact]
    public void Wallets_TableExists()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        Assert.NotNull(db.Wallets);
    }

    [Fact]
    public void OutboxMessages_TableExists()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        Assert.NotNull(db.OutboxMessages);
    }
}

public class WalletRepositoryTests
{
    private static WalletDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new WalletDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsWallet()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(wallet, CancellationToken.None);

        var saved = await db.Wallets.FindAsync([wallet.Id]);
        Assert.NotNull(saved);
        Assert.Equal(wallet.Id, saved.Id);
        Assert.Equal(wallet.AccountId, saved.AccountId);
        Assert.Equal(WalletStatus.Active, saved.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWallet_WhenFound()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();

        var result = await repository.GetByIdAsync(wallet.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(wallet.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesWallet()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();

        wallet.Status = WalletStatus.Frozen;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        await repository.UpdateAsync(wallet, CancellationToken.None);

        var saved = await db.Wallets.FindAsync([wallet.Id]);
        Assert.NotNull(saved);
        Assert.Equal(WalletStatus.Frozen, saved.Status);
    }

    [Fact]
    public async Task SaveOutboxMessage_PersistsMessage()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestType",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await repository.SaveOutboxMessageAsync(message, CancellationToken.None);

        var saved = await db.OutboxMessages.FindAsync([message.Id]);
        Assert.NotNull(saved);
        Assert.Equal(message.Id, saved.Id);
        Assert.Equal("TestType", saved.Type);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessagesAsync_ReturnsOnlyUnprocessed()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);

        var processed = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Processed",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            ProcessedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        var unprocessed = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Unprocessed",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        db.OutboxMessages.AddRange(processed, unprocessed);
        await db.SaveChangesAsync();

        var results = await repository.GetUnprocessedOutboxMessagesAsync(10, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(unprocessed.Id, results[0].Id);
    }

    [Fact]
    public async Task MarkOutboxMessageProcessedAsync_SetsProcessedAt()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new WalletRepository(db);
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync();

        await repository.MarkOutboxMessageProcessedAsync(message.Id, CancellationToken.None);

        var saved = await db.OutboxMessages.FindAsync([message.Id]);
        Assert.NotNull(saved);
        Assert.NotNull(saved.ProcessedAtUtc);
    }
}
