using Microsoft.Extensions.Logging;
using NSubstitute;
using FundingService.Features.Charge;
using FundingService.Infrastructure;

namespace FundingService.Tests;

public class IdempotencyStoreTests
{
    private readonly IdempotencyStore _store;

    public IdempotencyStoreTests()
    {
        var logger = Substitute.For<ILogger<IdempotencyStore>>();
        _store = new IdempotencyStore(logger);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotFound()
    {
        var result = await _store.GetAsync("nonexistent", CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAndGet_ReturnsStoredValue()
    {
        var response = new ChargeResponse
        {
            TransactionId = Guid.NewGuid(),
            Status = "succeeded",
            Amount = 100,
            Currency = "GBP"
        };

        await _store.SetAsync("key-1", response, CancellationToken.None);
        var result = await _store.GetAsync("key-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(response.TransactionId, result.TransactionId);
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task Set_OverwritesExistingKey()
    {
        var first = new ChargeResponse { TransactionId = Guid.NewGuid(), Status = "failed", Amount = 50, Currency = "GBP" };
        var second = new ChargeResponse { TransactionId = Guid.NewGuid(), Status = "succeeded", Amount = 100, Currency = "GBP" };

        await _store.SetAsync("key-1", first, CancellationToken.None);
        await _store.SetAsync("key-1", second, CancellationToken.None);
        var result = await _store.GetAsync("key-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(second.TransactionId, result.TransactionId);
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task MultipleKeys_AreIndependent()
    {
        var a = new ChargeResponse { TransactionId = Guid.NewGuid(), Status = "succeeded", Amount = 10, Currency = "GBP" };
        var b = new ChargeResponse { TransactionId = Guid.NewGuid(), Status = "failed", Amount = 20, Currency = "GBP" };

        await _store.SetAsync("key-a", a, CancellationToken.None);
        await _store.SetAsync("key-b", b, CancellationToken.None);

        var resultA = await _store.GetAsync("key-a", CancellationToken.None);
        var resultB = await _store.GetAsync("key-b", CancellationToken.None);

        Assert.Equal(10, resultA!.Amount);
        Assert.Equal(20, resultB!.Amount);
    }
}
