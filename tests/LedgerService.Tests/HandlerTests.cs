using LedgerService.Domain;
using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using LedgerService.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PayFlow.Shared.Primitives;

namespace LedgerService.Tests;

public class CreateEntryHandlerTests
{
    private readonly ILedgerRepository _repository;
    private readonly CreateEntryHandler _handler;

    public CreateEntryHandlerTests()
    {
        _repository = Substitute.For<ILedgerRepository>();
        var logger = Substitute.For<ILogger<CreateEntryHandler>>();
        _handler = new CreateEntryHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_ValidEntry_ReturnsSuccessWithEntryId()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "Credit",
            Amount = 100m,
            Currency = "GBP",
            Reference = "test-ref"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.EntryId);
        await _repository.Received(1).AddEntriesAsync(
            Arg.Is<IEnumerable<LedgerEntry>>(e => e != null && e.Count() == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NegativeAmount_ReturnsValidationError()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "Debit",
            Amount = -10m,
            Currency = "GBP"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
        await _repository.DidNotReceive().AddEntriesAsync(
            Arg.Any<IEnumerable<LedgerEntry>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ZeroAmount_ReturnsValidationError()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "Debit",
            Amount = 0m,
            Currency = "GBP"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidEntryType_ReturnsValidationError()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "InvalidType",
            Amount = 100m,
            Currency = "GBP"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_EntryTypeIsCaseInsensitive()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "credit",
            Amount = 100m,
            Currency = "GBP"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_EntryTypeUpperIsRecognized()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "DEBIT",
            Amount = 50m,
            Currency = "GBP"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_CurrenciesArePassedThrough()
    {
        var command = new CreateEntryCommand
        {
            TransactionId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            EntryType = "Credit",
            Amount = 200m,
            Currency = "USD"
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).AddEntriesAsync(
            Arg.Is<IEnumerable<LedgerEntry>>(entries =>
                entries != null && entries.Any() && entries.First().Currency == "USD"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandlePairAsync_ValidTransaction_ReturnsSuccessWithBothEntryIds()
    {
        var command = new CreateEntryPairCommand
        {
            DebitAccountId = Guid.NewGuid(),
            CreditAccountId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "GBP",
            Reference = "test-txn"
        };

        var result = await _handler.HandlePairAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.TransactionId);
        Assert.NotEqual(Guid.Empty, result.Value.DebitEntryId);
        Assert.NotEqual(Guid.Empty, result.Value.CreditEntryId);
        Assert.NotEqual(result.Value.DebitEntryId, result.Value.CreditEntryId);
        await _repository.Received(1).AddEntriesAsync(
            Arg.Is<IEnumerable<LedgerEntry>>(e => e != null && e.Count() == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandlePairAsync_NegativeAmount_ReturnsValidationError()
    {
        var command = new CreateEntryPairCommand
        {
            DebitAccountId = Guid.NewGuid(),
            CreditAccountId = Guid.NewGuid(),
            Amount = -100m,
            Currency = "GBP"
        };

        var result = await _handler.HandlePairAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
        await _repository.DidNotReceive().AddEntriesAsync(
            Arg.Any<IEnumerable<LedgerEntry>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandlePairAsync_CreatesMatchingDebitAndCredit()
    {
        var command = new CreateEntryPairCommand
        {
            DebitAccountId = Guid.NewGuid(),
            CreditAccountId = Guid.NewGuid(),
            Amount = 250m,
            Currency = "EUR",
            Reference = "fx-trade"
        };

        var result = await _handler.HandlePairAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).AddEntriesAsync(
            Arg.Is<IEnumerable<LedgerEntry>>(entries =>
                entries != null
                && entries.Count() == 2
                && entries.Any(e => e.EntryType == EntryType.Debit && e.Amount == 250m && e.Currency == "EUR")
                && entries.Any(e => e.EntryType == EntryType.Credit && e.Amount == 250m && e.Currency == "EUR")
                && entries.Select(e => e.TransactionId).Distinct().Count() == 1),
            Arg.Any<CancellationToken>());
    }
}

public class GetBalanceHandlerTests
{
    private readonly ILedgerRepository _repository;
    private readonly GetBalanceHandler _handler;

    public GetBalanceHandlerTests()
    {
        _repository = Substitute.For<ILedgerRepository>();
        _handler = new GetBalanceHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ExistingAccount_ReturnsCorrectBalance()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 200m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 50m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(250m, result.Value.Balance);
        Assert.Equal(accountId, result.Value.AccountId);
        Assert.Equal("GBP", result.Value.Currency);
    }

    [Fact]
    public async Task HandleAsync_BalanceIsCreditsMinusDebits()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 30m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(-70m, result.Value.Balance);
    }

    [Fact]
    public async Task HandleAsync_EmptyAccount_ReturnsAccountNotFound()
    {
        var accountId = Guid.NewGuid();
        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(new List<LedgerEntry>());

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ZeroBalance_ReturnsSuccess()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 50m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 50m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Balance);
    }

    [Fact]
    public async Task HandleAsync_OnlyDebits_ReturnsNegativeBalance()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(-100m, result.Value.Balance);
    }

    [Fact]
    public async Task HandleAsync_UsesFirstEntryCurrency()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 50m, Currency = "USD", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBalanceQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("USD", result.Value.Currency);
    }
}

public class GetTransactionHistoryHandlerTests
{
    private readonly ILedgerRepository _repository;
    private readonly GetTransactionHistoryHandler _handler;

    public GetTransactionHistoryHandlerTests()
    {
        _repository = Substitute.For<ILedgerRepository>();
        _handler = new GetTransactionHistoryHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ExistingAccount_ReturnsAllEntries()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Credit, Amount = 100m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow, Reference = "ref1" },
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 30m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow.AddMinutes(1), Reference = "ref2" }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetTransactionHistoryQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(accountId, result.Value.AccountId);
        Assert.Equal(2, result.Value.Entries.Count);
        Assert.Contains(result.Value.Entries, e => e.Reference == "ref1");
        Assert.Contains(result.Value.Entries, e => e.Reference == "ref2");
    }

    [Fact]
    public async Task HandleAsync_EmptyAccount_ReturnsAccountNotFound()
    {
        var accountId = Guid.NewGuid();
        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(new List<LedgerEntry>());

        var query = new GetTransactionHistoryQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_EntryMapping_PreservesAllFields()
    {
        var accountId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var entries = new List<LedgerEntry>
        {
            new()
            {
                Id = entryId,
                TransactionId = transactionId,
                AccountId = accountId,
                EntryType = EntryType.Credit,
                Amount = 75.50m,
                Currency = "EUR",
                CreatedAtUtc = createdAt,
                Reference = "payment"
            }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetTransactionHistoryQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var entry = result.Value.Entries.Single();
        Assert.Equal(entryId, entry.EntryId);
        Assert.Equal(transactionId, entry.TransactionId);
        Assert.Equal("Credit", entry.EntryType);
        Assert.Equal(75.50m, entry.Amount);
        Assert.Equal("EUR", entry.Currency);
        Assert.Equal(createdAt, entry.CreatedAtUtc);
        Assert.Equal("payment", entry.Reference);
    }

    [Fact]
    public async Task HandleAsync_SingleEntry_ReturnsSingleItem()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            new() { Id = Guid.NewGuid(), TransactionId = Guid.NewGuid(), AccountId = accountId, EntryType = EntryType.Debit, Amount = 10m, Currency = "GBP", CreatedAtUtc = DateTime.UtcNow }
        };

        _repository.GetEntriesByAccountIdAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetTransactionHistoryQuery(accountId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Entries);
    }
}