using NotificationService.Consumers;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPayFlowTelemetry(
    "NotificationService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddPayFlowMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost",
    configureConsumers: cfg =>
    {
        cfg.AddConsumer<LedgerEntryCreatedConsumer>();
        cfg.AddConsumer<LedgerEntryFailedConsumer>();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Notification Service",
        Description = "Fan-out consumer that listens to ledger events and sends (simulated) email and push notifications — demonstrates eventual consistency.",
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "NotificationService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the NotificationService.")
   .Produces(StatusCodes.Status200OK);

app.Run();
