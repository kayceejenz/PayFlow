using System.Diagnostics;
using System.Text.Json;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace PaymentService.Features.AuthorizePayment;

public class AuthorizePaymentHandler
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<AuthorizePaymentHandler> _logger;

    public AuthorizePaymentHandler(IPaymentRepository repository, ILogger<AuthorizePaymentHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<AuthorizePaymentResponse>> HandleAsync(AuthorizePaymentCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AuthorizePayment");
        activity?.SetTag("payer.account", command.PayerAccountId);
        activity?.SetTag("merchant.account", command.MerchantAccountId);
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);

        if (command.Amount <= 0)
            return Result.Failure<AuthorizePaymentResponse>(Error.Validation("Amount must be positive."));

        if (command.PayerAccountId == command.MerchantAccountId)
            return Result.Failure<AuthorizePaymentResponse>(Error.Validation("Payer and merchant accounts must be different."));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PayerAccountId = command.PayerAccountId,
            MerchantAccountId = command.MerchantAccountId,
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.PendingAuthorization,
            Reference = command.Reference,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var correlationId = payment.Id;

        var holdCommand = new CreateLedgerEntryCommand
        {
            CorrelationId = correlationId.ToString(),
            DebitAccountId = command.PayerAccountId,
            CreditAccountId = HoldsAccount.AccountId,
            Amount = command.Amount,
            Currency = command.Currency,
            Reference = $"Payment authorization hold: {command.Reference ?? payment.Id.ToString()}"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(CreateLedgerEntryCommand).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(holdCommand),
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await _repository.AddAsync(payment, ct);
        await _repository.SaveOutboxMessageAsync(outboxMessage, ct);

        activity?.SetTag("payment.id", payment.Id);
        activity?.SetTag("correlation.id", correlationId);

        _logger.LogInformation(
            "Authorized payment {PaymentId}: {Amount} {Currency} from payer {Payer} to merchant {Merchant} (hold placed via outbox)",
            payment.Id, command.Amount, command.Currency, command.PayerAccountId, command.MerchantAccountId);

        return Result.Success(new AuthorizePaymentResponse
        {
            PaymentId = payment.Id,
            Status = "pending_authorization"
        });
    }
}
