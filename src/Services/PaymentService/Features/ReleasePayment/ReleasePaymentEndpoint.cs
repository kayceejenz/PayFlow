namespace PaymentService.Features.ReleasePayment;

public static class ReleasePaymentEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/payments/{paymentId:guid}/release", async (
            Guid paymentId,
            ReleasePaymentHandler handler,
            CancellationToken ct) =>
        {
            var command = new ReleasePaymentCommand { PaymentId = paymentId };
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
        .WithName("ReleasePayment")
        .WithTags("Payments")
        .WithSummary("Release an authorized payment hold")
        .WithDescription("Releases the previously authorized hold, returning funds to the payer. This is the compensating transaction for the authorize step in the saga.")
        .Produces<ReleasePaymentResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
