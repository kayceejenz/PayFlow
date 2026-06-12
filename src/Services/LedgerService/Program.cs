using LedgerService.Infrastructure;
using LedgerService.Features.CreateEntry;
using LedgerService.Features.GetBalance;
using LedgerService.Features.GetTransactionHistory;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLedgerService(
    builder.Configuration.GetConnectionString("LedgerDb")!);

builder.Services.AddPayFlowTelemetry(
    "LedgerService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Ledger Service",
        Description = "Event-sourced double-entry ledger with CQRS. Manages account balances and transaction history.",
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
    var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

CreateEntryEndpoint.Map(app);
GetBalanceEndpoint.Map(app);
GetTransactionHistoryEndpoint.Map(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "LedgerService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the LedgerService. Used by orchestration and monitoring tools.")
   .Produces(StatusCodes.Status200OK);

app.Run();
