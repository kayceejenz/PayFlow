using Microsoft.AspNetCore.Builder;
using PaymentService.Features.AuthorizePayment;
using PaymentService.Features.CapturePayment;
using PaymentService.Features.ReleasePayment;
using PaymentService.Features.GetPayment;

namespace PaymentService.Tests;

public class EndpointTests
{
    [Fact]
    public async Task AuthorizePaymentEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { AuthorizePaymentEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task CapturePaymentEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { CapturePaymentEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task ReleasePaymentEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { ReleasePaymentEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetPaymentEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { GetPaymentEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }
}
