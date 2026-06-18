namespace WalletService.Features.GetWallet;

public static class GetWalletEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/wallets/{walletId}", async (
            Guid walletId,
            GetWalletHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetWalletQuery(walletId);
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
        .WithName("GetWallet")
        .WithTags("Wallets")
        .WithSummary("Get wallet details")
        .WithDescription("Returns wallet details including the associated ledger account ID and current status.")
        .Produces<GetWalletResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
