using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PayFlow.Shared.Observability;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("PayFlow", "1.0.0");

    public static IServiceCollection AddPayFlowTelemetry(
        this IServiceCollection services,
        string serviceName,
        string otlpEndpoint = "http://localhost:4317")
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: "1.0.0"))
            .WithTracing(tracing => tracing
                .AddSource(ActivitySource.Name)
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }
}
