using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Consumers;

namespace NotificationService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void ServiceProvider_ResolvesLedgerEntryCreatedConsumer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new LedgerEntryCreatedConsumer(
            services.BuildServiceProvider().GetRequiredService<ILogger<LedgerEntryCreatedConsumer>>()));

        var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<LedgerEntryCreatedConsumer>();
        Assert.NotNull(consumer);
    }

    [Fact]
    public void ServiceProvider_ResolvesLedgerEntryFailedConsumer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new LedgerEntryFailedConsumer(
            services.BuildServiceProvider().GetRequiredService<ILogger<LedgerEntryFailedConsumer>>()));

        var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<LedgerEntryFailedConsumer>();
        Assert.NotNull(consumer);
    }
}
