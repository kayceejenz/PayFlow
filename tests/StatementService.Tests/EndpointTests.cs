using Microsoft.AspNetCore.Builder;
using StatementService.Features.GetStatements;

namespace StatementService.Tests;

public class GetStatementsEndpointTests
{
    [Fact]
    public void Map_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var exception = Record.Exception(() => GetStatementsEndpoint.Map(app));

        Assert.Null(exception);
    }
}
