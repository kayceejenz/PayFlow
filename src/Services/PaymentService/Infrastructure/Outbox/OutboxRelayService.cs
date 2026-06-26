using System.Text.Json;
using MassTransit;
using PayFlow.Shared.Messaging;

namespace PaymentService.Infrastructure;

public class OutboxRelayService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxRelayService> _logger;

    public OutboxRelayService(IServiceScopeFactory scopeFactory, ILogger<OutboxRelayService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox relay service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IBus>();

                var messages = await repository.GetUnprocessedOutboxMessagesAsync(50, stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var payloadType = Type.GetType(message.Type);
                        if (payloadType == null)
                        {
                            _logger.LogWarning("Unknown outbox message type: {Type}", message.Type);
                            await repository.MarkOutboxMessageProcessedAsync(message.Id, stoppingToken);
                            continue;
                        }

                        var payload = JsonSerializer.Deserialize(message.Payload, payloadType);
                        if (payload == null)
                        {
                            _logger.LogWarning("Failed to deserialize outbox message: {Id}", message.Id);
                            await repository.MarkOutboxMessageProcessedAsync(message.Id, stoppingToken);
                            continue;
                        }

                        await bus.Publish(payload, payloadType, stoppingToken);

                        await repository.MarkOutboxMessageProcessedAsync(message.Id, stoppingToken);

                        _logger.LogInformation("Published outbox message {Id} of type {Type}", message.Id, message.Type);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
                        message.RetryCount++;
                        if (message.RetryCount >= 5)
                        {
                            _logger.LogWarning("Outbox message {Id} exceeded max retries, marking as processed", message.Id);
                            await repository.MarkOutboxMessageProcessedAsync(message.Id, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox relay cycle failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Outbox relay service stopped");
    }
}
