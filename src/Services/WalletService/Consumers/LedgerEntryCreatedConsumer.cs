using MassTransit;
using PayFlow.Shared.Messaging;

namespace WalletService.Consumers;

public class LedgerEntryCreatedConsumer : IConsumer<LedgerEntryCreatedEvent>
{
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;

    public LedgerEntryCreatedConsumer(ILogger<LedgerEntryCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LedgerEntryCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Ledger entry created for correlation {CorrelationId}: transaction {TransactionId}, amount {Amount} {Currency}",
            message.CorrelationId, message.TransactionId, message.Amount, message.Currency);

        return Task.CompletedTask;
    }
}
