using Microsoft.EntityFrameworkCore;

namespace LedgerService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddLedgerInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILedgerRepository, LedgerRepository>();

        return services;
    }
}
