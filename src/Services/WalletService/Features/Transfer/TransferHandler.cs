using System.Diagnostics;
using System.Text.Json;
using WalletService.Domain;
using WalletService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace WalletService.Features.Transfer;

public class TransferHandler
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<TransferHandler> _logger;

    public TransferHandler(IWalletRepository repository, ILogger<TransferHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<TransferResponse>> HandleAsync(TransferCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("Transfer");
        activity?.SetTag("source.wallet.id", command.WalletId);
        activity?.SetTag("dest.wallet.id", command.DestinationWalletId);
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);

        if (command.Amount <= 0)
            return Result.Failure<TransferResponse>(Error.Validation("Amount must be positive."));

        if (command.WalletId == command.DestinationWalletId)
            return Result.Failure<TransferResponse>(Error.Validation("Cannot transfer to the same wallet."));

        var sourceWallet = await _repository.GetByIdAsync(command.WalletId, ct);

        if (sourceWallet == null)
            return Result.Failure<TransferResponse>(WalletErrors.NotFound);

        if (sourceWallet.Status == WalletStatus.Frozen)
            return Result.Failure<TransferResponse>(WalletErrors.Frozen);

        if (sourceWallet.Status == WalletStatus.Closed)
            return Result.Failure<TransferResponse>(WalletErrors.Closed);

        var destWallet = await _repository.GetByIdAsync(command.DestinationWalletId, ct);

        if (destWallet == null)
            return Result.Failure<TransferResponse>(Error.NotFound("Destination wallet not found."));

        if (destWallet.Status != WalletStatus.Active)
            return Result.Failure<TransferResponse>(Error.Conflict("Destination wallet is not active."));

        var correlationId = Guid.NewGuid();

        var ledgerCommand = new CreateLedgerEntryCommand
        {
            CorrelationId = correlationId.ToString(),
            DebitAccountId = sourceWallet.AccountId,
            CreditAccountId = destWallet.AccountId,
            Amount = command.Amount,
            Currency = command.Currency,
            Reference = command.Reference
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(CreateLedgerEntryCommand).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(ledgerCommand),
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await _repository.SaveOutboxMessageAsync(outboxMessage, ct);

        activity?.SetTag("correlation.id", correlationId);

        _logger.LogInformation(
            "Queued transfer of {Amount} {Currency} from wallet {SourceWallet} to wallet {DestWallet} (correlation {CorrelationId})",
            command.Amount, command.Currency, command.WalletId, command.DestinationWalletId, correlationId);

        return Result.Success(new TransferResponse
        {
            CorrelationId = correlationId,
            Status = "pending"
        });
    }
}
