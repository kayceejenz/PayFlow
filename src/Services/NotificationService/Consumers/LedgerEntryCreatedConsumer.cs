using MassTransit;
using PayFlow.Shared.Messaging;

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

        _logger.LogInformation(
            "Sending email notification: transaction {TransactionId}, amount {Amount} {Currency}, correlation {CorrelationId}",
            msg.TransactionId, msg.Amount, msg.Currency, msg.CorrelationId);

        _logger.LogInformation(
            "Sending push notification: transaction {TransactionId}, amount {Amount} {Currency}, correlation {CorrelationId}",
            msg.TransactionId, msg.Amount, msg.Currency, msg.CorrelationId);

        return Task.CompletedTask;
    }
}
