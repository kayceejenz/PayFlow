using MassTransit;
using NotificationService.Domain;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace NotificationService.Consumers;

public class LedgerEntryFailedConsumer : IConsumer<LedgerEntryFailedEvent>
{
    private readonly ILogger<LedgerEntryFailedConsumer> _logger;

    public LedgerEntryFailedConsumer(ILogger<LedgerEntryFailedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LedgerEntryFailedEvent> context)
    {
        var msg = context.Message;

        using var activity = Telemetry.ActivitySource.StartActivity("NotificationFailed");
        activity?.SetTag("error.code", msg.ErrorCode);
        activity?.SetTag("error.message", msg.ErrorMessage);
        activity?.SetTag("correlation.id", msg.CorrelationId);

        _logger.LogWarning(
            "Sending email notification — entry failed: {ErrorCode}: {ErrorMessage}, correlation {CorrelationId}",
            msg.ErrorCode, msg.ErrorMessage, msg.CorrelationId);

        _logger.LogWarning(
            "Sending push notification — entry failed: {ErrorCode}: {ErrorMessage}, correlation {CorrelationId}",
            msg.ErrorCode, msg.ErrorMessage, msg.CorrelationId);

        return Task.CompletedTask;
    }
}
