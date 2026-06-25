using PaymentService.Infrastructure;
using PaymentService.Features.AuthorizePayment;
using PaymentService.Features.CapturePayment;
using PaymentService.Features.ReleasePayment;
using PaymentService.Features.GetPayment;
using PaymentService.Consumers;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPaymentService(
    builder.Configuration.GetConnectionString("PaymentDb")!);

builder.Services.AddPayFlowTelemetry(
    "PaymentService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddPayFlowMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost",
    configureConsumers: cfg =>
    {
        cfg.AddConsumer<LedgerEntryCreatedConsumer>();
        cfg.AddConsumer<LedgerEntryFailedConsumer>();
    });

builder.Services.AddHostedService<OutboxRelayService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Payment Service",
        Description = "Orchestrates the merchant payment saga: authorize (hold) → capture/release with compensating transactions via outbox-based async communication.",
        Version = "v1",
        Contact = new()
        {
            Name = "PayFlow Team",
            Email = "team@payflow.dev"
        }
    });
});
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

AuthorizePaymentEndpoint.Map(app);
CapturePaymentEndpoint.Map(app);
ReleasePaymentEndpoint.Map(app);
GetPaymentEndpoint.Map(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "PaymentService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the PaymentService. Used by orchestration and monitoring tools.")
   .Produces(StatusCodes.Status200OK);

app.Run();
