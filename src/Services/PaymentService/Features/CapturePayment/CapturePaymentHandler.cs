using System.Diagnostics;
using System.Text.Json;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace PaymentService.Features.CapturePayment;

public class CapturePaymentHandler
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<CapturePaymentHandler> _logger;

    public CapturePaymentHandler(IPaymentRepository repository, ILogger<CapturePaymentHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<CapturePaymentResponse>> HandleAsync(CapturePaymentCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("CapturePayment");
        activity?.SetTag("payment.id", command.PaymentId);

        var payment = await _repository.GetByIdAsync(command.PaymentId, ct);

        if (payment == null)
            return Result.Failure<CapturePaymentResponse>(PaymentErrors.NotFound);

        if (payment.Status != PaymentStatus.Authorized)
            return Result.Failure<CapturePaymentResponse>(PaymentErrors.NotAuthorized);

        payment.Status = PaymentStatus.ProcessingCapture;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        var captureCommand = new CreateLedgerEntryCommand
        {
            CorrelationId = payment.Id.ToString(),
            DebitAccountId = HoldsAccount.AccountId,
            CreditAccountId = payment.MerchantAccountId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Reference = $"Payment capture: {payment.Reference ?? payment.Id.ToString()}"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(CreateLedgerEntryCommand).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(captureCommand),
            CorrelationId = payment.Id,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await _repository.UpdateAsync(payment, ct);
        await _repository.SaveOutboxMessageAsync(outboxMessage, ct);

        activity?.SetTag("correlation.id", payment.Id);

        _logger.LogInformation(
            "Capturing payment {PaymentId}: {Amount} {Currency} from holds to merchant {Merchant}",
            payment.Id, payment.Amount, payment.Currency, payment.MerchantAccountId);

        return Result.Success(new CapturePaymentResponse
        {
            PaymentId = payment.Id,
            Status = "processing_capture"
        });
    }
}
