using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;

namespace WalletService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddWalletService(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<WalletDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IWalletRepository, WalletRepository>();

        services.AddScoped<CreateWalletHandler>();
        services.AddScoped<GetWalletHandler>();
        services.AddScoped<UpdateWalletStatusHandler>();
        services.AddScoped<TopUpHandler>();
        services.AddScoped<TransferHandler>();

        return services;
    }
}
