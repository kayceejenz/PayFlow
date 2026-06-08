using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace PayFlow.Shared.Messaging;

public static class BusConfigurator
{
    public static IServiceCollection AddPayFlowMessageBus(
        this IServiceCollection services,
        string host = "localhost",
        string username = "guest",
        string password = "guest",
        string virtualHost = "/")
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
