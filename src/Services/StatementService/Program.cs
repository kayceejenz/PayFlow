using PayFlow.Shared.Messaging;
using PayFlow.Shared.Observability;
using StatementService.Consumers;
using StatementService.Features.GetStatements;
using StatementService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPayFlowTelemetry(
    "StatementService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddPayFlowMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost",
    configureConsumers: cfg =>
    {
        cfg.AddConsumer<LedgerEntryCreatedConsumer>();
    });

var connectionString = builder.Configuration.GetConnectionString("StatementDb");
    
builder.Services.AddStatementInfrastructure(connectionString!);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Statement Service",
        Description = "CQRS read model — denormalized transaction history built from ledger events.",
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

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "StatementService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the StatementService.")
   .Produces(StatusCodes.Status200OK);

GetStatementsEndpoint.Map(app);

app.Run();
