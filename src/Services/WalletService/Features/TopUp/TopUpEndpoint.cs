namespace WalletService.Features.TopUp;

public static class TopUpEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/wallets/{walletId}/top-up", async (
            Guid walletId,
            TopUpCommand command,
            TopUpHandler handler,
            CancellationToken ct) =>
        {
            command = command with { WalletId = walletId };
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Accepted($"/top-up/status/{result.Value.CorrelationId}", result.Value)
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
        .WithName("TopUpWallet")
        .WithTags("Wallets")
        .WithSummary("Top up a wallet (async via outbox)")
        .WithDescription("Initiates a top-up for the specified wallet. The request is queued via the outbox pattern and processed asynchronously. Returns 202 Accepted with a correlation ID for status polling.")
        .Produces<TopUpResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
