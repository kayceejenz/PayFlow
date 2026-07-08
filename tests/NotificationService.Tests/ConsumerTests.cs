using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NotificationService.Consumers;
using PayFlow.Shared.Messaging;

namespace NotificationService.Tests;

public class LedgerEntryCreatedConsumerTests
{
    private readonly ILogger<LedgerEntryCreatedConsumer> _logger;
    private readonly LedgerEntryCreatedConsumer _consumer;

    public LedgerEntryCreatedConsumerTests()
    {
        _logger = Substitute.For<ILogger<LedgerEntryCreatedConsumer>>();
        _consumer = new LedgerEntryCreatedConsumer(_logger);
    }

    [Fact]
    public async Task Consume_LogsTwoNotifications()
    {
        var context = Substitute.For<ConsumeContext<LedgerEntryCreatedEvent>>();
        context.Message.Returns(new LedgerEntryCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100,
            Currency = "GBP",
            CorrelationId = "corr-1"
        });

        await _consumer.Consume(context);

        _logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Consume_CompletesWithoutError()
    {
        var context = Substitute.For<ConsumeContext<LedgerEntryCreatedEvent>>();
        context.Message.Returns(new LedgerEntryCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            Amount = 50,
            Currency = "GBP",
            CorrelationId = "corr-2"
        });

        var exception = await Record.ExceptionAsync(() => _consumer.Consume(context));

        Assert.Null(exception);
    }
}

public class LedgerEntryFailedConsumerTests
{
    private readonly ILogger<LedgerEntryFailedConsumer> _logger;
    private readonly LedgerEntryFailedConsumer _consumer;

    public LedgerEntryFailedConsumerTests()
    {
        _logger = Substitute.For<ILogger<LedgerEntryFailedConsumer>>();
        _consumer = new LedgerEntryFailedConsumer(_logger);
    }

    [Fact]
    public async Task Consume_LogsTwoNotifications()
    {
        var context = Substitute.For<ConsumeContext<LedgerEntryFailedEvent>>();
        context.Message.Returns(new LedgerEntryFailedEvent
        {
            ErrorCode = "INSUFFICIENT_FUNDS",
            ErrorMessage = "Not enough balance",
            CorrelationId = "corr-1"
        });

        await _consumer.Consume(context);

        _logger.Received(2).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Consume_CompletesWithoutError()
    {
        var context = Substitute.For<ConsumeContext<LedgerEntryFailedEvent>>();
        context.Message.Returns(new LedgerEntryFailedEvent
        {
            ErrorCode = "ERROR",
            ErrorMessage = "Something failed",
            CorrelationId = "corr-2"
        });

        var exception = await Record.ExceptionAsync(() => _consumer.Consume(context));

        Assert.Null(exception);
    }
}
