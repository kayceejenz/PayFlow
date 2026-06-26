namespace PaymentService.Features.AuthorizePayment;

public static class AuthorizePaymentEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/payments/authorize", async (
            AuthorizePaymentCommand command,
            AuthorizePaymentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Accepted($"/payments/{result.Value.PaymentId}", result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "NOT_FOUND" => StatusCodes.Status404NotFound,
                        "CONFLICT" => StatusCodes.Status409Conflict,
                        "VALIDATION" => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("AuthorizePayment")
        .WithTags("Payments")
        .WithSummary("Authorize a payment (hold funds)")
        .WithDescription("Initiates a payment authorization by placing a hold on the payer's funds. Returns 202 Accepted with a payment ID for status polling.")
        .Produces<AuthorizePaymentResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
