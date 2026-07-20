using ApiGateway.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using PayFlow.Shared.Observability;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPayFlowTelemetry(
    "ApiGateway",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("default", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseRateLimiter();

app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseMiddleware<IdempotencyKeyMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway" }));

app.MapReverseProxy();

app.Run();
