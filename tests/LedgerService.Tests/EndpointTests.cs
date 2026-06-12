using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using Microsoft.AspNetCore.Builder;

namespace LedgerService.Tests;

public class CreateEntryEndpointTests
{
    [Fact]
    public void Map_RegistersEntriesRoute_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        var exception = Record.Exception(() => CreateEntryEndpoint.Map(app));
        Assert.Null(exception);
    }

    [Fact]
    public void Map_CanBeCalledMultipleTimes()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        CreateEntryEndpoint.Map(app);
        var exception = Record.Exception(() => CreateEntryEndpoint.Map(app));
        Assert.Null(exception);
    }
}

public class GetBalanceEndpointTests
{
    [Fact]
    public void Map_RegistersBalanceRoute_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        var exception = Record.Exception(() => GetBalanceEndpoint.Map(app));
        Assert.Null(exception);
    }
}

public class GetTransactionHistoryEndpointTests
{
    [Fact]
    public void Map_RegistersHistoryRoute_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        var exception = Record.Exception(() => GetTransactionHistoryEndpoint.Map(app));
        Assert.Null(exception);
    }
}