using MassTransit;
using Microsoft.EntityFrameworkCore;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;
using StatementService.Domain;
using StatementService.Infrastructure;

namespace StatementService.Consumers;

public class LedgerEntryCreatedConsumer : IConsumer<LedgerEntryCreatedEvent>
{
    private readonly StatementDbContext _db;
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;

    public LedgerEntryCreatedConsumer(
        StatementDbContext db,
        ILogger<LedgerEntryCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LedgerEntryCreatedEvent> context)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ConsumeLedgerEntryForStatement");
        activity?.SetTag("transaction.id", context.Message.TransactionId);
        activity?.SetTag("debit.account", context.Message.DebitAccountId);
        activity?.SetTag("credit.account", context.Message.CreditAccountId);
        activity?.SetTag("amount", context.Message.Amount);

        var debitEntry = new StatementEntry
        {
            Id = context.Message.DebitEntryId,
            WalletId = context.Message.DebitAccountId,
            TransactionId = context.Message.TransactionId,
            EntryType = "Debit",
            Amount = context.Message.Amount,
            Currency = context.Message.Currency,
            CounterpartyId = context.Message.CreditAccountId,
            Reference = context.Message.Reference,
            CreatedAtUtc = context.Message.OccurredAtUtc
        };

        var creditEntry = new StatementEntry
        {
            Id = context.Message.CreditEntryId,
            WalletId = context.Message.CreditAccountId,
            TransactionId = context.Message.TransactionId,
            EntryType = "Credit",
            Amount = context.Message.Amount,
            Currency = context.Message.Currency,
            CounterpartyId = context.Message.DebitAccountId,
            Reference = context.Message.Reference,
            CreatedAtUtc = context.Message.OccurredAtUtc
        };

        _db.StatementEntries.Add(debitEntry);
        _db.StatementEntries.Add(creditEntry);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Stored statement entries for transaction {TransactionId}: debit {DebitAccount}, credit {CreditAccount}",
            context.Message.TransactionId, context.Message.DebitAccountId, context.Message.CreditAccountId);
    }
}
