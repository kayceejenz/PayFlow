using FundingService.Features.Charge;

namespace FundingService.Infrastructure;

public interface IIdempotencyStore
{
    Task<ChargeResponse?> GetAsync(string key, CancellationToken ct);
    Task SetAsync(string key, ChargeResponse response, CancellationToken ct);
}
