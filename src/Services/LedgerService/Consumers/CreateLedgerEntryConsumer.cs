using System.Diagnostics;
using MassTransit;
using LedgerService.Features.CreateEntry;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

namespace LedgerService.Consumers;

public class CreateLedgerEntryConsumer : IConsumer<CreateLedgerEntryCommand>
{
    private readonly CreateEntryHandler _handler;
    private readonly ILogger<CreateLedgerEntryConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateLedgerEntryConsumer(
        CreateEntryHandler handler,
        ILogger<CreateLedgerEntryConsumer> logger,
        IPublishEndpoint publishEndpoint)
    {
        _handler = handler;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CreateLedgerEntryCommand> context)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ConsumeCreateLedgerEntry");
        activity?.SetTag("correlation.id", context.Message.CorrelationId);
        activity?.SetTag("debit.account", context.Message.DebitAccountId);
        activity?.SetTag("credit.account", context.Message.CreditAccountId);
        activity?.SetTag("amount", context.Message.Amount);

        var command = new CreateEntryPairCommand
        {
            DebitAccountId = context.Message.DebitAccountId,
            CreditAccountId = context.Message.CreditAccountId,
            Amount = context.Message.Amount,
            Currency = context.Message.Currency,
            Reference = context.Message.Reference
        };

        var result = await _handler.HandlePairAsync(command, context.CancellationToken);

        if (result.IsSuccess)
        {
            var response = result.Value;

            await _publishEndpoint.Publish(new LedgerEntryCreatedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                TransactionId = response.TransactionId,
                DebitEntryId = response.DebitEntryId,
                CreditEntryId = response.CreditEntryId,
                DebitAccountId = context.Message.DebitAccountId,
                CreditAccountId = context.Message.CreditAccountId,
                Amount = context.Message.Amount,
                Currency = context.Message.Currency,
                Reference = context.Message.Reference
            }, context.CancellationToken);

            _logger.LogInformation(
                "Processed ledger entry command {CorrelationId}: transaction {TransactionId}",
                context.Message.CorrelationId, response.TransactionId);
        }
        else
        {
            await _publishEndpoint.Publish(new LedgerEntryFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                ErrorCode = result.Error.Code,
                ErrorMessage = result.Error.Message
            }, context.CancellationToken);

            _logger.LogWarning(
                "Failed to process ledger entry command {CorrelationId}: {ErrorCode} - {ErrorMessage}",
                context.Message.CorrelationId, result.Error.Code, result.Error.Message);
        }
    }
}
