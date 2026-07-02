using FundingService.Features.Charge;

namespace FundingService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddFundingService(this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<ChargeHandler>();
        return services;
    }
}
