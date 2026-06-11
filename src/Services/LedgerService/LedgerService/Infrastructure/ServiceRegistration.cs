using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddLedgerService(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<CreateEntryHandler>();
        services.AddScoped<GetBalanceHandler>();
        services.AddScoped<GetTransactionHistoryHandler>();

        return services;
    }
}
