using MassTransit;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace NotificationService.Consumers;

public class LedgerEntryCreatedConsumer : IConsumer<LedgerEntryCreatedEvent>
{
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;

    public LedgerEntryCreatedConsumer(ILogger<LedgerEntryCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LedgerEntryCreatedEvent> context)
    {
        var msg = context.Message;

        using var activity = Telemetry.ActivitySource.StartActivity("NotificationCreated");
        activity?.SetTag("transaction.id", msg.TransactionId);
        activity?.SetTag("amount", msg.Amount);
        activity?.SetTag("currency", msg.Currency);
        activity?.SetTag("correlation.id", msg.CorrelationId);

        _logger.LogInformation(
            "Sending email notification — transaction {TransactionId}, amount {Amount} {Currency}, correlation {CorrelationId}",
            msg.TransactionId, msg.Amount, msg.Currency, msg.CorrelationId);

        _logger.LogInformation(
            "Sending push notification — transaction {TransactionId}, amount {Amount} {Currency}, correlation {CorrelationId}",
            msg.TransactionId, msg.Amount, msg.Currency, msg.CorrelationId);

        return Task.CompletedTask;
    }
}
