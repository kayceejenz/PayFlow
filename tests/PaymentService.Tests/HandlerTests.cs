using Microsoft.Extensions.Logging;
using NSubstitute;
using PaymentService.Domain;
using PaymentService.Features.AuthorizePayment;
using PaymentService.Features.CapturePayment;
using PaymentService.Features.ReleasePayment;
using PaymentService.Features.GetPayment;
using PaymentService.Infrastructure;

namespace PaymentService.Tests;

public class AuthorizePaymentHandlerTests
{
    private readonly IPaymentRepository _repository;
    private readonly AuthorizePaymentHandler _handler;

    public AuthorizePaymentHandlerTests()
    {
        _repository = Substitute.For<IPaymentRepository>();
        var logger = Substitute.For<ILogger<AuthorizePaymentHandler>>();
        _handler = new AuthorizePaymentHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsAccepted()
    {
        var payerAccountId = Guid.NewGuid();
        var merchantAccountId = Guid.NewGuid();

        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = payerAccountId,
                MerchantAccountId = merchantAccountId,
                Amount = 100,
                Currency = "GBP"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PaymentId);
        Assert.Equal("pending_authorization", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_ZeroAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = Guid.NewGuid(),
                MerchantAccountId = Guid.NewGuid(),
                Amount = 0
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NegativeAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = Guid.NewGuid(),
                MerchantAccountId = Guid.NewGuid(),
                Amount = -50
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SamePayerAndMerchant_ReturnsValidationError()
    {
        var accountId = Guid.NewGuid();
        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = accountId,
                MerchantAccountId = accountId,
                Amount = 100
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SavesOutboxMessage()
    {
        var payerAccountId = Guid.NewGuid();
        var merchantAccountId = Guid.NewGuid();

        OutboxMessage? capturedMessage = null;
        await _repository.SaveOutboxMessageAsync(Arg.Do<OutboxMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = payerAccountId,
                MerchantAccountId = merchantAccountId,
                Amount = 100,
                Currency = "GBP",
                Reference = "test-auth"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMessage);
        Assert.Equal(result.Value.PaymentId, capturedMessage.CorrelationId);
        Assert.Contains("CreateLedgerEntryCommand", capturedMessage.Type);
        Assert.NotEmpty(capturedMessage.Payload);
    }

    [Fact]
    public async Task HandleAsync_PersistsPayment()
    {
        Payment? capturedPayment = null;
        await _repository.AddAsync(Arg.Do<Payment>(p => capturedPayment = p), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(
            new AuthorizePaymentCommand
            {
                PayerAccountId = Guid.NewGuid(),
                MerchantAccountId = Guid.NewGuid(),
                Amount = 100
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedPayment);
        Assert.Equal(result.Value.PaymentId, capturedPayment.Id);
        Assert.Equal(PaymentStatus.PendingAuthorization, capturedPayment.Status);
        Assert.Equal(100m, capturedPayment.Amount);
    }
}

public class CapturePaymentHandlerTests
{
    private readonly IPaymentRepository _repository;
    private readonly CapturePaymentHandler _handler;

    public CapturePaymentHandlerTests()
    {
        _repository = Substitute.For<IPaymentRepository>();
        var logger = Substitute.For<ILogger<CapturePaymentHandler>>();
        _handler = new CapturePaymentHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_AuthorizedPayment_ReturnsAccepted()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.HandleAsync(
            new CapturePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("processing_capture", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_PaymentNotFound_ReturnsNotFound()
    {
        var paymentId = Guid.NewGuid();
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _handler.HandleAsync(
            new CapturePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NotAuthorized_ReturnsConflict()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.PendingAuthorization,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.HandleAsync(
            new CapturePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_UpdatesPaymentStatus()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        await _handler.HandleAsync(
            new CapturePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.Equal(PaymentStatus.ProcessingCapture, payment.Status);
    }

    [Fact]
    public async Task HandleAsync_SavesOutboxMessage()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        OutboxMessage? capturedMessage = null;
        await _repository.SaveOutboxMessageAsync(Arg.Do<OutboxMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(
            new CapturePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMessage);
        Assert.Contains("CreateLedgerEntryCommand", capturedMessage.Type);
        Assert.NotEmpty(capturedMessage.Payload);
    }
}

public class ReleasePaymentHandlerTests
{
    private readonly IPaymentRepository _repository;
    private readonly ReleasePaymentHandler _handler;

    public ReleasePaymentHandlerTests()
    {
        _repository = Substitute.For<IPaymentRepository>();
        var logger = Substitute.For<ILogger<ReleasePaymentHandler>>();
        _handler = new ReleasePaymentHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_AuthorizedPayment_ReturnsAccepted()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.HandleAsync(
            new ReleasePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("processing_release", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_PaymentNotFound_ReturnsNotFound()
    {
        var paymentId = Guid.NewGuid();
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _handler.HandleAsync(
            new ReleasePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NotAuthorized_ReturnsConflict()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Status = PaymentStatus.PendingAuthorization,
            Amount = 100,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.HandleAsync(
            new ReleasePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_UpdatesPaymentStatus()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        await _handler.HandleAsync(
            new ReleasePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.Equal(PaymentStatus.ProcessingRelease, payment.Status);
    }

    [Fact]
    public async Task HandleAsync_SavesOutboxMessage()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        OutboxMessage? capturedMessage = null;
        await _repository.SaveOutboxMessageAsync(Arg.Do<OutboxMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(
            new ReleasePaymentCommand { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMessage);
        Assert.Contains("CreateLedgerEntryCommand", capturedMessage.Type);
        Assert.NotEmpty(capturedMessage.Payload);
    }
}

public class GetPaymentHandlerTests
{
    private readonly IPaymentRepository _repository;
    private readonly GetPaymentHandler _handler;

    public GetPaymentHandlerTests()
    {
        _repository = Substitute.For<IPaymentRepository>();
        _handler = new GetPaymentHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_PaymentExists_ReturnsPayment()
    {
        var paymentId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            Id = paymentId,
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            Reference = "test",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.HandleAsync(
            new GetPaymentQuery { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(paymentId, result.Value.PaymentId);
        Assert.Equal("Authorized", result.Value.Status);
        Assert.Equal(100, result.Value.Amount);
    }

    [Fact]
    public async Task HandleAsync_PaymentNotFound_ReturnsFailure()
    {
        var paymentId = Guid.NewGuid();
        _repository.GetByIdAsync(paymentId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _handler.HandleAsync(
            new GetPaymentQuery { PaymentId = paymentId },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }
}
