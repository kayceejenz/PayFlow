using System.Diagnostics;
using LedgerService.Domain;
using LedgerService.Infrastructure;
using PayFlow.Shared.Observability;

namespace LedgerService.Features.CreateEntry;

public class CreateEntryHandler
{
    private readonly ILedgerRepository _repository;
    private readonly ILogger<CreateEntryHandler> _logger;

    public CreateEntryHandler(ILedgerRepository repository, ILogger<CreateEntryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<CreateEntryResponse>> HandleAsync(CreateEntryCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("CreateEntry");
        activity?.SetTag("account.id", command.AccountId);
        activity?.SetTag("entry.type", command.EntryType);
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);

        if (command.Amount <= 0)
            return Result.Failure<CreateEntryResponse>(LedgerErrors.InvalidEntry("Amount must be positive."));

        if (!Enum.TryParse<EntryType>(command.EntryType, true, out var entryType))
            return Result.Failure<CreateEntryResponse>(LedgerErrors.InvalidEntry($"Invalid entry type: {command.EntryType}"));

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = command.TransactionId,
            AccountId = command.AccountId,
            EntryType = entryType,
            Amount = command.Amount,
            Currency = command.Currency,
            CreatedAtUtc = DateTime.UtcNow,
            Reference = command.Reference
        };

        await _repository.AddEntriesAsync([entry], ct);

        activity?.SetTag("entry.id", entry.Id);

        _logger.LogInformation(
            "Created {EntryType} entry {EntryId} for account {AccountId}, amount {Amount} {Currency}",
            entryType, entry.Id, command.AccountId, command.Amount, command.Currency);

        return Result.Success(new CreateEntryResponse(entry.Id));
    }

    public async Task<Result<CreateEntryPairResponse>> HandlePairAsync(
        CreateEntryPairCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("CreateTransaction");
        activity?.SetTag("debit.account", command.DebitAccountId);
        activity?.SetTag("credit.account", command.CreditAccountId);
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);

        if (command.Amount <= 0)
            return Result.Failure<CreateEntryPairResponse>(LedgerErrors.InvalidEntry("Amount must be positive."));

        var transactionId = Guid.NewGuid();

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            AccountId = command.DebitAccountId,
            EntryType = EntryType.Debit,
            Amount = command.Amount,
            Currency = command.Currency,
            CreatedAtUtc = DateTime.UtcNow,
            Reference = command.Reference
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            AccountId = command.CreditAccountId,
            EntryType = EntryType.Credit,
            Amount = command.Amount,
            Currency = command.Currency,
            CreatedAtUtc = DateTime.UtcNow,
            Reference = command.Reference
        };

        await _repository.AddEntriesAsync([debitEntry, creditEntry], ct);

        activity?.SetTag("transaction.id", transactionId);
        activity?.SetTag("debit.entry.id", debitEntry.Id);
        activity?.SetTag("credit.entry.id", creditEntry.Id);

        _logger.LogInformation(
            "Created transaction {TransactionId}: debit {DebitAmount} to {DebitAccount}, credit {CreditAmount} to {CreditAccount}",
            transactionId, command.Amount, command.DebitAccountId, command.Amount, command.CreditAccountId);

        return Result.Success(new CreateEntryPairResponse(
            transactionId, debitEntry.Id, creditEntry.Id));
    }
}
