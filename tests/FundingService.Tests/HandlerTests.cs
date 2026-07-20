using System.Collections.Concurrent;
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
public async Task HandleAsync_DoubleFireWithSameKey_ReturnsCachedResultOnce()
{
    var callCount = 0;
    ChargeResponse? cached = null;
    _store.GetAsync("key-double", Arg.Any<CancellationToken>())
        .Returns(_ => cached);
    _store.When(x => x.SetAsync("key-double", Arg.Any<ChargeResponse>(), Arg.Any<CancellationToken>()))
        .Do(callInfo => { cached = callInfo.ArgAt<ChargeResponse>(1); Interlocked.Increment(ref callCount); });

    var result1 = await _handler.HandleAsync("key-double", new ChargeCommand { Amount = 100 }, CancellationToken.None);
    var result2 = await _handler.HandleAsync("key-double", new ChargeCommand { Amount = 100 }, CancellationToken.None);

    Assert.True(result1.IsSuccess);
    Assert.True(result2.IsSuccess);
    Assert.Equal(result1.Value.TransactionId, result2.Value.TransactionId);
    Assert.Equal(1, callCount);
}

[Fact]
public async Task HandleAsync_DifferentKeys_ExecuteIndependently()
{
    _store.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ChargeResponse?)null);

    var result1 = await _handler.HandleAsync("key-a", new ChargeCommand { Amount = 50 }, CancellationToken.None);
    var result2 = await _handler.HandleAsync("key-b", new ChargeCommand { Amount = 100 }, CancellationToken.None);

    Assert.True(result1.IsSuccess);
    Assert.True(result2.IsSuccess);
    Assert.NotEqual(result1.Value.TransactionId, result2.Value.TransactionId);
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
