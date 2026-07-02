using System.Collections.Concurrent;
using FundingService.Features.Charge;

namespace FundingService.Infrastructure;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, ChargeResponse> _store = new();
    private readonly ILogger<IdempotencyStore> _logger;

    public IdempotencyStore(ILogger<IdempotencyStore> logger)
    {
        _logger = logger;
    }

    public Task<ChargeResponse?> GetAsync(string key, CancellationToken ct)
    {
        _store.TryGetValue(key, out var response);
        return Task.FromResult(response);
    }

    public Task SetAsync(string key, ChargeResponse response, CancellationToken ct)
    {
        _store[key] = response;
        _logger.LogDebug("Stored idempotency result for key {Key}", key);
        return Task.CompletedTask;
    }
}
