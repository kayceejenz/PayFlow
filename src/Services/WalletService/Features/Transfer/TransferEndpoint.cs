namespace WalletService.Features.Transfer;

public static class TransferEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/wallets/{walletId}/transfer", async (
            Guid walletId,
            TransferCommand command,
            TransferHandler handler,
            CancellationToken ct) =>
        {
            command = command with { WalletId = walletId };
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Accepted($"/transfer/status/{result.Value.CorrelationId}", result.Value)
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
        .WithName("TransferFunds")
        .WithTags("Wallets")
        .WithSummary("Transfer funds between wallets (async via outbox)")
        .WithDescription("Initiates a transfer from the source wallet to the destination wallet. Returns 202 Accepted with a correlation ID for status polling.")
        .Produces<TransferResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
