using WalletService.Infrastructure;
using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;
using WalletService.Consumers;
using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWalletService(
    builder.Configuration.GetConnectionString("WalletDb")!);

builder.Services.AddPayFlowTelemetry(
    "WalletService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddPayFlowMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost",
    configureConsumers: cfg => cfg.AddConsumer<LedgerEntryCreatedConsumer>());

builder.Services.AddHostedService<OutboxRelayService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Wallet Service",
        Description = "Manages wallet lifecycle and orchestrates top-up/transfer operations via async event-driven communication with LedgerService.",
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
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

CreateWalletEndpoint.Map(app);
GetWalletEndpoint.Map(app);
UpdateWalletStatusEndpoint.Map(app);
TopUpEndpoint.Map(app);
TransferEndpoint.Map(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "WalletService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the WalletService.")
   .Produces(StatusCodes.Status200OK);

app.Run();
