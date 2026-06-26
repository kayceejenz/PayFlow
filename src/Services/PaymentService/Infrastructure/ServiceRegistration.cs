using PaymentService.Features.AuthorizePayment;
using PaymentService.Features.CapturePayment;
using PaymentService.Features.ReleasePayment;
using PaymentService.Features.GetPayment;

namespace PaymentService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddPaymentService(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<AuthorizePaymentHandler>();
        services.AddScoped<CapturePaymentHandler>();
        services.AddScoped<ReleasePaymentHandler>();
        services.AddScoped<GetPaymentHandler>();

        return services;
    }
}
