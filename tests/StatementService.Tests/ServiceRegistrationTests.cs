using Microsoft.Extensions.DependencyInjection;
using StatementService.Consumers;
using StatementService.Features.GetStatements;
using StatementService.Infrastructure;

namespace StatementService.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddStatementInfrastructure_RegistersRepository()
    {
        var services = new ServiceCollection();

        services.AddStatementInfrastructure(
            "Host=localhost;Database=statement_service;Username=payflow;Password=payflow_secret");

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<IStatementRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddStatementInfrastructure_RegistersHandler()
    {
        var services = new ServiceCollection();

        services.AddStatementInfrastructure(
            "Host=localhost;Database=statement_service;Username=payflow;Password=payflow_secret");

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<GetStatementsHandler>();
        Assert.NotNull(handler);
    }
}
