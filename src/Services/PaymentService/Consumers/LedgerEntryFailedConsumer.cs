using System.Diagnostics;
using MassTransit;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace PaymentService.Consumers;

public class LedgerEntryFailedConsumer : IConsumer<LedgerEntryFailedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LedgerEntryFailedConsumer> _logger;

    public LedgerEntryFailedConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<LedgerEntryFailedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LedgerEntryFailedEvent> context)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("PaymentLedgerEntryFailed");
        var message = context.Message;
        activity?.SetTag("correlation.id", message.CorrelationId);
        activity?.SetTag("error.code", message.ErrorCode);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        if (!Guid.TryParse(message.CorrelationId, out var paymentId))
        {
            _logger.LogWarning("Invalid correlation ID format: {CorrelationId}", message.CorrelationId);
            return;
        }

        var payment = await repository.GetByIdAsync(paymentId, context.CancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found for correlation {CorrelationId}", paymentId, message.CorrelationId);
            return;
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = $"{message.ErrorCode}: {message.ErrorMessage}";
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(payment, context.CancellationToken);

        _logger.LogWarning(
            "Payment {PaymentId} failed: {ErrorCode} - {ErrorMessage}",
            payment.Id, message.ErrorCode, message.ErrorMessage);
    }
}
