using System.Diagnostics;
using System.Text.Json;
using WalletService.Domain;
using WalletService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace WalletService.Features.TopUp;

public class TopUpHandler
{
    private readonly IWalletRepository _repository;
    private readonly IFundingServiceClient _fundingClient;
    private readonly ILogger<TopUpHandler> _logger;

    public TopUpHandler(
        IWalletRepository repository,
        IFundingServiceClient fundingClient,
        ILogger<TopUpHandler> logger)
    {
        _repository = repository;
        _fundingClient = fundingClient;
        _logger = logger;
    }

    public async Task<Result<TopUpResponse>> HandleAsync(TopUpCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("TopUp");
        activity?.SetTag("wallet.id", command.WalletId);
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);

        if (command.Amount <= 0)
            return Result.Failure<TopUpResponse>(Error.Validation("Amount must be positive."));

        var wallet = await _repository.GetByIdAsync(command.WalletId, ct);

        if (wallet == null)
            return Result.Failure<TopUpResponse>(WalletErrors.NotFound);

        if (wallet.Status == WalletStatus.Frozen)
            return Result.Failure<TopUpResponse>(WalletErrors.Frozen);

        if (wallet.Status == WalletStatus.Closed)
            return Result.Failure<TopUpResponse>(WalletErrors.Closed);

        var idempotencyKey = command.IdempotencyKey ?? Guid.NewGuid().ToString();

        var fundingRequest = new FundingChargeRequest
        {
            Amount = command.Amount,
            Currency = command.Currency,
            Reference = command.Reference
        };

        FundingChargeResponse fundingResponse;
        try
        {
            fundingResponse = await _fundingClient.ChargeAsync(idempotencyKey, fundingRequest, ct);
        }
        catch (Exception ex)
        {
            activity?.SetTag("funding.error", ex.Message);
            _logger.LogError(ex, "Funding service call failed for top-up to wallet {WalletId}", command.WalletId);
            return Result.Failure<TopUpResponse>(Error.Unexpected("Funding service unavailable."));
        }

        if (fundingResponse.Status == "failed")
        {
            activity?.SetTag("funding.status", "failed");
            _logger.LogWarning(
                "Funding charge failed for top-up to wallet {WalletId}: {Reason}",
                command.WalletId, fundingResponse.FailureReason);
            return Result.Failure<TopUpResponse>(Error.Conflict(
                $"Funding charge failed: {fundingResponse.FailureReason}"));
        }

        activity?.SetTag("funding.status", "succeeded");
        activity?.SetTag("funding.transaction.id", fundingResponse.TransactionId);

        var correlationId = Guid.NewGuid();
        var externalAccountId = Guid.Empty;

        var ledgerCommand = new CreateLedgerEntryCommand
        {
            CorrelationId = correlationId.ToString(),
            DebitAccountId = externalAccountId,
            CreditAccountId = wallet.AccountId,
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
            "Queued top-up of {Amount} {Currency} to wallet {WalletId} (correlation {CorrelationId}, funding txn {FundingTxn})",
            command.Amount, command.Currency, command.WalletId, correlationId, fundingResponse.TransactionId);

        return Result.Success(new TopUpResponse
        {
            CorrelationId = correlationId,
            Status = "pending"
        });
    }
}
