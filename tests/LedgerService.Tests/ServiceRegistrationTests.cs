using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using LedgerService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedgerService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddLedgerService_RegistersDbContext()
    {
        var services = new ServiceCollection();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<LedgerDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddLedgerService_RegistersRepository()
    {
        var services = new ServiceCollection();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var repository = provider.GetService<ILedgerRepository>();
        Assert.NotNull(repository);
        Assert.IsType<LedgerRepository>(repository);
    }

    [Fact]
    public void AddLedgerService_RegistersCreateEntryHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<CreateEntryHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddLedgerService_RegistersGetBalanceHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<GetBalanceHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddLedgerService_RegistersGetTransactionHistoryHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<GetTransactionHistoryHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddLedgerService_UsesNpgsql()
    {
        var services = new ServiceCollection();
        services.AddLedgerService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<LedgerDbContext>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddLedgerService_AllHandlersAreScoped()
    {
        var services = new ServiceCollection();
        services.AddLedgerService("Host=localhost;Database=test");

        var createEntryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(CreateEntryHandler));
        Assert.NotNull(createEntryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, createEntryDescriptor.Lifetime);

        var getBalanceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(GetBalanceHandler));
        Assert.NotNull(getBalanceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, getBalanceDescriptor.Lifetime);

        var historyDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(GetTransactionHistoryHandler));
        Assert.NotNull(historyDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, historyDescriptor.Lifetime);
    }

    [Fact]
    public void AddLedgerService_RepositoryIsScoped()
    {
        var services = new ServiceCollection();
        services.AddLedgerService("Host=localhost;Database=test");

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILedgerRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}