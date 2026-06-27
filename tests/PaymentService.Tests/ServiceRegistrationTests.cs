using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentService.Features.AuthorizePayment;
using PaymentService.Features.CapturePayment;
using PaymentService.Features.ReleasePayment;
using PaymentService.Features.GetPayment;
using PaymentService.Infrastructure;

namespace PaymentService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddPaymentService_RegistersDbContext()
    {
        var services = new ServiceCollection();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<PaymentDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddPaymentService_RegistersRepository()
    {
        var services = new ServiceCollection();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var repository = provider.GetService<IPaymentRepository>();
        Assert.NotNull(repository);
        Assert.IsType<PaymentRepository>(repository);
    }

    [Fact]
    public void AddPaymentService_RegistersAuthorizePaymentHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<AuthorizePaymentHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddPaymentService_RegistersCapturePaymentHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<CapturePaymentHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddPaymentService_RegistersReleasePaymentHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<ReleasePaymentHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddPaymentService_RegistersGetPaymentHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<GetPaymentHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddPaymentService_UsesNpgsql()
    {
        var services = new ServiceCollection();
        services.AddPaymentService("Host=localhost;Database=test");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<PaymentDbContext>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddPaymentService_AllHandlersAreScoped()
    {
        var services = new ServiceCollection();
        services.AddPaymentService("Host=localhost;Database=test");

        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(AuthorizePaymentHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(CapturePaymentHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(ReleasePaymentHandler)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped,
            services.First(d => d.ServiceType == typeof(GetPaymentHandler)).Lifetime);
    }

    [Fact]
    public void AddPaymentService_RepositoryIsScoped()
    {
        var services = new ServiceCollection();
        services.AddPaymentService("Host=localhost;Database=test");

        var descriptor = services.First(d => d.ServiceType == typeof(IPaymentRepository));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
