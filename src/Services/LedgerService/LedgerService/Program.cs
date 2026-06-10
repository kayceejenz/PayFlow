using LedgerService.Infrastructure;
using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLedgerInfrastructure(
    builder.Configuration.GetConnectionString("LedgerDb")!);

builder.Services.AddPayFlowTelemetry(
    "LedgerService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

CreateEntryEndpoint.Map(app);
GetBalanceEndpoint.Map(app);
GetTransactionHistoryEndpoint.Map(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "LedgerService" }))
   .WithName("HealthCheck");

app.Run();
