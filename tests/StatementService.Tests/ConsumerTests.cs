using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PayFlow.Shared.Messaging;
using StatementService.Consumers;
using StatementService.Infrastructure;

namespace StatementService.Tests;

public class LedgerEntryCreatedConsumerTests
{
    private readonly StatementDbContext _db;
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;
    private readonly LedgerEntryCreatedConsumer _consumer;

    public LedgerEntryCreatedConsumerTests()
    {
        var options = new DbContextOptionsBuilder<StatementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new StatementDbContext(options);
        _logger = Substitute.For<ILogger<LedgerEntryCreatedConsumer>>();
        _consumer = new LedgerEntryCreatedConsumer(_db, _logger);
    }

    [Fact]
    public async Task Consume_CreatesTwoStatementEntries()
    {
        var debitId = Guid.NewGuid();
        var creditId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var debitAccountId = Guid.NewGuid();
        var creditAccountId = Guid.NewGuid();

        var context = Substitute.For<ConsumeContext<LedgerEntryCreatedEvent>>();
        context.Message.Returns(new LedgerEntryCreatedEvent
        {
            TransactionId = transactionId,
            DebitEntryId = debitId,
            CreditEntryId = creditId,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = 100,
            Currency = "GBP",
            CorrelationId = "corr-1",
            Reference = "top-up"
        });

        await _consumer.Consume(context);

        var entries = await _db.StatementEntries.ToListAsync();
        Assert.Equal(2, entries.Count);

        var debit = Assert.Single(entries, e => e.Id == debitId);
        Assert.Equal(debitAccountId, debit.WalletId);
        Assert.Equal("Debit", debit.EntryType);
        Assert.Equal(creditAccountId, debit.CounterpartyId);
        Assert.Equal("top-up", debit.Reference);

        var credit = Assert.Single(entries, e => e.Id == creditId);
        Assert.Equal(creditAccountId, credit.WalletId);
        Assert.Equal("Credit", credit.EntryType);
        Assert.Equal(debitAccountId, credit.CounterpartyId);
        Assert.Equal("top-up", credit.Reference);
    }

    [Fact]
    public async Task Consume_SetsSameTransactionIdOnBothEntries()
    {
        var transactionId = Guid.NewGuid();
        var context = Substitute.For<ConsumeContext<LedgerEntryCreatedEvent>>();
        context.Message.Returns(new LedgerEntryCreatedEvent
        {
            TransactionId = transactionId,
            DebitEntryId = Guid.NewGuid(),
            CreditEntryId = Guid.NewGuid(),
            DebitAccountId = Guid.NewGuid(),
            CreditAccountId = Guid.NewGuid(),
            Amount = 50,
            Currency = "GBP",
            CorrelationId = "corr-2"
        });

        await _consumer.Consume(context);

        var entries = await _db.StatementEntries.ToListAsync();
        Assert.All(entries, e => Assert.Equal(transactionId, e.TransactionId));
    }

    [Fact]
    public async Task Consume_CompletesWithoutError()
    {
        var context = Substitute.For<ConsumeContext<LedgerEntryCreatedEvent>>();
        context.Message.Returns(new LedgerEntryCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            DebitEntryId = Guid.NewGuid(),
            CreditEntryId = Guid.NewGuid(),
            DebitAccountId = Guid.NewGuid(),
            CreditAccountId = Guid.NewGuid(),
            Amount = 75,
            Currency = "GBP",
            CorrelationId = "corr-3"
        });

        var exception = await Record.ExceptionAsync(() => _consumer.Consume(context));

        Assert.Null(exception);
    }
}
