namespace PaymentService.Features.GetPayment;

public static class GetPaymentEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/payments/{paymentId:guid}", async (
            Guid paymentId,
            GetPaymentHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetPaymentQuery { PaymentId = paymentId };
            var result = await handler.HandleAsync(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "NOT_FOUND" => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("GetPayment")
        .WithTags("Payments")
        .WithSummary("Get payment status")
        .WithDescription("Returns the current status of a payment. Used for polling after an async authorize/capture/release operation.")
        .Produces<GetPaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
