using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;
using WalletService.Infrastructure;

namespace WalletService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddWalletService_RegistersDbContext()
    {
        var services = new ServiceCollection();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<WalletDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddWalletService_RegistersRepository()
    {
        var services = new ServiceCollection();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var repository = provider.GetService<IWalletRepository>();
        Assert.NotNull(repository);
        Assert.IsType<WalletRepository>(repository);
    }

    [Fact]
    public void AddWalletService_RegistersCreateWalletHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<CreateWalletHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddWalletService_RegistersGetWalletHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<GetWalletHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddWalletService_RegistersUpdateWalletStatusHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<UpdateWalletStatusHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddWalletService_RegistersTopUpHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<TopUpHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddWalletService_RegistersTransferHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<TransferHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddWalletService_UsesNpgsql()
    {
        var services = new ServiceCollection();
        services.AddWalletService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<WalletDbContext>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddWalletService_AllHandlersAreScoped()
    {
        var services = new ServiceCollection();
        services.AddWalletService("Host=localhost;Database=test");

        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(CreateWalletHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(GetWalletHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(UpdateWalletStatusHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(TopUpHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(TransferHandler)).Lifetime);
    }

    [Fact]
    public void AddWalletService_RepositoryIsScoped()
    {
        var services = new ServiceCollection();
        services.AddWalletService("Host=localhost;Database=test");

        var descriptor = services.First(d => d.ServiceType == typeof(IWalletRepository));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
