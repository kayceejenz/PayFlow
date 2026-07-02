namespace FundingService.Features.Charge;

public static class ChargeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/funding/charge", async (
            HttpContext httpContext,
            ChargeCommand command,
            ChargeHandler handler,
            CancellationToken ct) =>
        {
            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return Results.Problem(
                    detail: "Idempotency-Key header is required.",
                    statusCode: StatusCodes.Status400BadRequest);

            var result = await handler.HandleAsync(idempotencyKey, command, ct);
            if (result.IsSuccess)
            {
                return result.Value.Status == "succeeded"
                    ? Results.Ok(result.Value)
                    : Results.Ok(result.Value);
            }

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.Code switch
                {
                    "VALIDATION" => StatusCodes.Status400BadRequest,
                    "CONFLICT" => StatusCodes.Status402PaymentRequired,
                    _ => StatusCodes.Status500InternalServerError
                });
        })
        .WithName("ChargeFunding")
        .WithTags("Funding")
        .WithSummary("Simulate an external card charge")
        .WithDescription("Simulates charging an external funding source (card/bank). Has a configurable failure rate. Requires an Idempotency-Key header.")
        .Produces<ChargeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status402PaymentRequired);
    }
}
