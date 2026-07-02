using Microsoft.Extensions.Logging;
using NSubstitute;
using FundingService.Features.Charge;
using FundingService.Infrastructure;

namespace FundingService.Tests;

public class ChargeHandlerTests
{
    private readonly IIdempotencyStore _store;
    private readonly ChargeHandler _handler;

    public ChargeHandlerTests()
    {
        _store = Substitute.For<IIdempotencyStore>();
        var logger = Substitute.For<ILogger<ChargeHandler>>();
        _handler = new ChargeHandler(_store, logger);
    }

    [Fact]
    public async Task HandleAsync_ZeroAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync("key-1", new ChargeCommand { Amount = 0 }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_CachedResponse_ReturnsCached()
    {
        var cached = new ChargeResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "succeeded",
            Amount = 100,
            Currency = "GBP"
        };
        _store.GetAsync("key-1", Arg.Any<CancellationToken>()).Returns(cached);

        var result = await _handler.HandleAsync("key-1", new ChargeCommand { Amount = 100 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(cached.TransactionId, result.Value.TransactionId);
        Assert.Equal("succeeded", result.Value.Status);
    }

[Fact]
public async Task HandleAsync_StoresResultInIdempotencyStore()
{
    _store.GetAsync("key-1", Arg.Any<CancellationToken>()).Returns((ChargeResponse?)null);

    ChargeResponse? stored = null;
    _store.When(x => x.SetAsync(Arg.Any<string>(), Arg.Any<ChargeResponse>(), Arg.Any<CancellationToken>()))
        .Do(callInfo => stored = callInfo.ArgAt<ChargeResponse>(1));

    var result = await _handler.HandleAsync("key-1", new ChargeCommand { Amount = 100, Currency = "GBP" }, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotNull(stored);
    Assert.Equal(result.Value.TransactionId, stored.TransactionId);
}
}
