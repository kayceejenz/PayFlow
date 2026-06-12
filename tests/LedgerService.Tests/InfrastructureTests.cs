using LedgerService.Domain;
using LedgerService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Tests;

public class LedgerDbContextTests
{
    private static LedgerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerDbContext(options);
    }

    [Fact]
    public async Task LedgerEntries_DbSet_CanInsertAndRetrieve()
    {
        using var db = CreateDbContext();
        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = EntryType.Credit,
            Amount = 100m,
            Currency = "GBP",
            CreatedAtUtc = DateTime.UtcNow,
            Reference = "test"
        };

        db.LedgerEntries.Add(entry);
        await db.SaveChangesAsync();

        var retrieved = await db.LedgerEntries.FirstOrDefaultAsync(e => e.Id == entry.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(entry.Amount, retrieved.Amount);
        Assert.Equal(entry.EntryType, retrieved.EntryType);
        Assert.Equal(entry.Reference, retrieved.Reference);
    }

    [Fact]
    public async Task LedgerEntries_TableName_IsLedgerEntries()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);
        Assert.Equal("ledger_entries", entityType.GetTableName());
    }

    [Fact]
    public async Task LedgerEntry_Id_IsValueGeneratedNever()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);
        var key = entityType.FindPrimaryKey();
        Assert.NotNull(key);
        var idProperty = entityType.FindProperty(nameof(LedgerEntry.Id));
        Assert.NotNull(idProperty);
        Assert.True(idProperty.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public async Task LedgerEntry_HasIndexesOnAccountIdAndTransactionId()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);

        var accountIdIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(LedgerEntry.AccountId)) && i.Properties.Count == 1);
        Assert.NotNull(accountIdIndex);

        var transactionIdIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(LedgerEntry.TransactionId)) && i.Properties.Count == 1);
        Assert.NotNull(transactionIdIndex);
    }

    [Fact]
    public async Task LedgerEntry_HasCompositeIndexOnAccountIdAndCreatedAtUtc()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);

        var compositeIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 2
                && i.Properties.Any(p => p.Name == nameof(LedgerEntry.AccountId))
                && i.Properties.Any(p => p.Name == nameof(LedgerEntry.CreatedAtUtc)));
        Assert.NotNull(compositeIndex);
    }

    [Fact]
    public async Task LedgerEntry_Currency_HasMaxLength3()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);
        var currencyProperty = entityType.FindProperty(nameof(LedgerEntry.Currency));
        Assert.NotNull(currencyProperty);
        Assert.Equal(3, currencyProperty.GetMaxLength());
    }

    [Fact]
    public async Task LedgerEntry_Amount_HasDecimalPrecision()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(LedgerEntry));
        Assert.NotNull(entityType);
        var amountProperty = entityType.FindProperty(nameof(LedgerEntry.Amount));
        Assert.NotNull(amountProperty);
        Assert.Equal(typeof(decimal), amountProperty.ClrType);
    }
}

public class LedgerRepositoryTests : IDisposable
{
    private readonly LedgerDbContext _dbContext;
    private readonly LedgerRepository _repository;
    private readonly Guid _accountId;
    private readonly Guid _transactionId;

    public LedgerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new LedgerDbContext(options);
        _repository = new LedgerRepository(_dbContext);
        _accountId = Guid.NewGuid();
        _transactionId = Guid.NewGuid();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task AddEntriesAsync_AddsSingleEntry()
    {
        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = _transactionId,
            AccountId = _accountId,
            EntryType = EntryType.Credit,
            Amount = 50m,
            Currency = "GBP",
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddEntriesAsync([entry], CancellationToken.None);

        var entries = await _dbContext.LedgerEntries.ToListAsync();
        Assert.Single(entries);
    }

    [Fact]
    public async Task AddEntriesAsync_AddsMultipleEntries()
    {
        var entries = new[]
        {
            new LedgerEntry
            {
                Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
                EntryType = EntryType.Debit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
            },
            new LedgerEntry
            {
                Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = Guid.NewGuid(),
                EntryType = EntryType.Credit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
            }
        };

        await _repository.AddEntriesAsync(entries, CancellationToken.None);

        var allEntries = await _dbContext.LedgerEntries.ToListAsync();
        Assert.Equal(2, allEntries.Count);
    }

    [Fact]
    public async Task GetEntriesByAccountIdAsync_ReturnsEntriesForAccount()
    {
        var entry1 = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
            EntryType = EntryType.Credit, Amount = 50m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
        };
        var entry2 = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
            EntryType = EntryType.Debit, Amount = 20m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddMinutes(1)
        };
        var otherEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = Guid.NewGuid(),
            EntryType = EntryType.Credit, Amount = 999m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.LedgerEntries.AddRange(entry1, entry2, otherEntry);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetEntriesByAccountIdAsync(_accountId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(_accountId, e.AccountId));
    }

    [Fact]
    public async Task GetEntriesByAccountIdAsync_ReturnsEntriesOrderedByCreatedAtUtc()
    {
        var earlier = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
            EntryType = EntryType.Credit, Amount = 10m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
        };
        var later = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
            EntryType = EntryType.Debit, Amount = 5m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddHours(-1)
        };
        _dbContext.LedgerEntries.AddRange(later, earlier);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetEntriesByAccountIdAsync(_accountId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].CreatedAtUtc <= result[1].CreatedAtUtc);
    }

    [Fact]
    public async Task GetEntriesByAccountIdAsync_ReturnsEmptyListForUnknownAccount()
    {
        var result = await _repository.GetEntriesByAccountIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEntriesByTransactionIdAsync_ReturnsEntriesForTransaction()
    {
        var matching = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
            EntryType = EntryType.Debit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
        };
        var nonMatching = new LedgerEntry
        {
            Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = _accountId,
            EntryType = EntryType.Credit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.LedgerEntries.AddRange(matching, nonMatching);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetEntriesByTransactionIdAsync(_transactionId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(_transactionId, result[0].TransactionId);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsCreditMinusDebit()
    {
        var entries = new[]
        {
            new LedgerEntry
            {
                Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
                EntryType = EntryType.Credit, Amount = 200m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow
            },
            new LedgerEntry
            {
                Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
                EntryType = EntryType.Credit, Amount = 50m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddMinutes(1)
            },
            new LedgerEntry
            {
                Id = Guid.NewGuid(), TransactionId = _transactionId, AccountId = _accountId,
                EntryType = EntryType.Debit, Amount = 30m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddMinutes(2)
            }
        };
        _dbContext.LedgerEntries.AddRange(entries);
        await _dbContext.SaveChangesAsync();

        var balance = await _repository.GetBalanceAsync(_accountId, CancellationToken.None);

        Assert.Equal(220m, balance);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsZeroForUnknownAccount()
    {
        var balance = await _repository.GetBalanceAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(0m, balance);
    }
}