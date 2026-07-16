using Microsoft.EntityFrameworkCore;
using StatementService.Domain;
using StatementService.Infrastructure;

namespace StatementService.Tests;

public class StatementRepositoryTests
{
    private StatementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<StatementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new StatementDbContext(options);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        var db = CreateDbContext();
        var walletId = Guid.NewGuid();
        db.StatementEntries.AddRange(
            new StatementEntry
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                TransactionId = Guid.NewGuid(),
                EntryType = "Debit",
                Amount = 100,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow
            },
            new StatementEntry
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                TransactionId = Guid.NewGuid(),
                EntryType = "Credit",
                Amount = 50,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow
            },
            new StatementEntry
            {
                Id = Guid.NewGuid(),
                WalletId = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                EntryType = "Debit",
                Amount = 200,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var repo = new StatementRepository(db);
        var count = await repo.GetTotalCountAsync(walletId, CancellationToken.None);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsEntriesOrderedByDateDesc()
    {
        var db = CreateDbContext();
        var walletId = Guid.NewGuid();
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();

        db.StatementEntries.AddRange(
            new StatementEntry
            {
                Id = olderId,
                WalletId = walletId,
                TransactionId = Guid.NewGuid(),
                EntryType = "Debit",
                Amount = 100,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = new DateTime(2026, 1, 1)
            },
            new StatementEntry
            {
                Id = newerId,
                WalletId = walletId,
                TransactionId = Guid.NewGuid(),
                EntryType = "Credit",
                Amount = 50,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = new DateTime(2026, 6, 1)
            });
        await db.SaveChangesAsync();

        var repo = new StatementRepository(db);
        var entries = await repo.GetPagedAsync(walletId, 0, 10, CancellationToken.None);

        Assert.Equal(2, entries.Count);
        Assert.Equal(newerId, entries[0].Id);
        Assert.Equal(olderId, entries[1].Id);
    }

    [Fact]
    public async Task GetPagedAsync_RespectsPagination()
    {
        var db = CreateDbContext();
        var walletId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
        {
            db.StatementEntries.Add(new StatementEntry
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                TransactionId = Guid.NewGuid(),
                EntryType = "Debit",
                Amount = 10,
                Currency = "GBP",
                CounterpartyId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-i)
            });
        }
        await db.SaveChangesAsync();

        var repo = new StatementRepository(db);
        var page1 = await repo.GetPagedAsync(walletId, 0, 2, CancellationToken.None);
        var page2 = await repo.GetPagedAsync(walletId, 2, 2, CancellationToken.None);

        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
        Assert.NotEqual(page1[0].Id, page2[0].Id);
    }
}
