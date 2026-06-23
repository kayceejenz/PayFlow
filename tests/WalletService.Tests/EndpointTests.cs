using Microsoft.AspNetCore.Builder;
using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;

namespace WalletService.Tests;

public class EndpointTests
{
    [Fact]
    public async Task CreateWalletEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { CreateWalletEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetWalletEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { GetWalletEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task UpdateWalletStatusEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { UpdateWalletStatusEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task TopUpEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { TopUpEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }

    [Fact]
    public async Task TransferEndpoint_Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var exception = await Record.ExceptionAsync(() => { TransferEndpoint.Map(app); return Task.CompletedTask; });
        Assert.Null(exception);
    }
}
