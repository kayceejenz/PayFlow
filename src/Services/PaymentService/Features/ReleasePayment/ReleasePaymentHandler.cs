using System.Diagnostics;
using System.Text.Json;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace PaymentService.Features.ReleasePayment;

public class ReleasePaymentHandler
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<ReleasePaymentHandler> _logger;

    public ReleasePaymentHandler(IPaymentRepository repository, ILogger<ReleasePaymentHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ReleasePaymentResponse>> HandleAsync(ReleasePaymentCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ReleasePayment");
        activity?.SetTag("payment.id", command.PaymentId);

        var payment = await _repository.GetByIdAsync(command.PaymentId, ct);

        if (payment == null)
            return Result.Failure<ReleasePaymentResponse>(PaymentErrors.NotFound);

        if (payment.Status != PaymentStatus.Authorized)
            return Result.Failure<ReleasePaymentResponse>(PaymentErrors.NotAuthorized);

        payment.Status = PaymentStatus.ProcessingRelease;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        var releaseCommand = new CreateLedgerEntryCommand
        {
            CorrelationId = payment.Id.ToString(),
            DebitAccountId = HoldsAccount.AccountId,
            CreditAccountId = payment.PayerAccountId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Reference = $"Payment hold release: {payment.Reference ?? payment.Id.ToString()}"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(CreateLedgerEntryCommand).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(releaseCommand),
            CorrelationId = payment.Id,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await _repository.UpdateAsync(payment, ct);
        await _repository.SaveOutboxMessageAsync(outboxMessage, ct);

        activity?.SetTag("correlation.id", payment.Id);

        _logger.LogInformation(
            "Releasing payment hold {PaymentId}: {Amount} {Currency} from holds back to payer {Payer}",
            payment.Id, payment.Amount, payment.Currency, payment.PayerAccountId);

        return Result.Success(new ReleasePaymentResponse
        {
            PaymentId = payment.Id,
            Status = "processing_release"
        });
    }
}
