using System.Diagnostics;
using MassTransit;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace PaymentService.Consumers;

public class LedgerEntryCreatedConsumer : IConsumer<LedgerEntryCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;

    public LedgerEntryCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<LedgerEntryCreatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LedgerEntryCreatedEvent> context)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("PaymentLedgerEntryCreated");
        var message = context.Message;
        activity?.SetTag("correlation.id", message.CorrelationId);
        activity?.SetTag("transaction.id", message.TransactionId);

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

        payment.Status = payment.Status switch
        {
            PaymentStatus.PendingAuthorization => PaymentStatus.Authorized,
            PaymentStatus.ProcessingCapture => PaymentStatus.Captured,
            PaymentStatus.ProcessingRelease => PaymentStatus.Released,
            _ => payment.Status
        };
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(payment, context.CancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} advanced to {Status} after ledger entry created (transaction {TransactionId})",
            payment.Id, payment.Status, message.TransactionId);
    }
}
