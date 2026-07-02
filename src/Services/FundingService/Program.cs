using FundingService.Infrastructure;
using FundingService.Features.Charge;
using PayFlow.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFundingService();

builder.Services.AddPayFlowTelemetry(
    "FundingService",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "PayFlow Funding Service",
        Description = "Simulated external funding source (card processor/bank) with configurable failure rate for demonstrating resilience patterns.",
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

ChargeEndpoint.Map(app);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "FundingService" }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check endpoint")
   .WithDescription("Returns the current health status of the FundingService.")
   .Produces(StatusCodes.Status200OK);

app.Run();
