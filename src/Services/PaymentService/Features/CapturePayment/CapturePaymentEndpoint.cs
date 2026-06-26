namespace PaymentService.Features.CapturePayment;

public static class CapturePaymentEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/payments/{paymentId:guid}/capture", async (
            Guid paymentId,
            CapturePaymentHandler handler,
            CancellationToken ct) =>
        {
            var command = new CapturePaymentCommand { PaymentId = paymentId };
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
        .WithName("CapturePayment")
        .WithTags("Payments")
        .WithSummary("Capture an authorized payment")
        .WithDescription("Captures the previously authorized hold and completes the payment to the merchant. Can only be called on an Authorized payment.")
        .Produces<CapturePaymentResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
