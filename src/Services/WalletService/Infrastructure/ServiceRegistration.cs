using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;
using Polly;

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

    public static IServiceCollection AddFundingClient(
        this IServiceCollection services,
        string baseUrl)
    {
        services.AddHttpClient<IFundingServiceClient, FundingServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1))))
        .AddTransientHttpErrorPolicy(policy =>
            policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        return services;
    }
}
