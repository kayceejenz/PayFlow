using Microsoft.EntityFrameworkCore;
using StatementService.Consumers;
using StatementService.Features.GetStatements;

namespace StatementService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddStatementInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StatementDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IStatementRepository, StatementRepository>();
        services.AddScoped<GetStatementsHandler>();

        return services;
    }
}
