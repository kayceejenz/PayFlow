using Microsoft.Extensions.DependencyInjection;
using FundingService.Features.Charge;
using FundingService.Infrastructure;

namespace FundingService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddFundingService_RegistersIdempotencyStore()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFundingService();

        var provider = services.BuildServiceProvider();
        var store = provider.GetService<IIdempotencyStore>();
        Assert.NotNull(store);
        Assert.IsType<IdempotencyStore>(store);
    }

    [Fact]
    public void AddFundingService_IdempotencyStoreIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddFundingService();

        var descriptor = services.First(d => d.ServiceType == typeof(IIdempotencyStore));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddFundingService_RegistersChargeHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFundingService();

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<ChargeHandler>();
        Assert.NotNull(handler);
    }
}
