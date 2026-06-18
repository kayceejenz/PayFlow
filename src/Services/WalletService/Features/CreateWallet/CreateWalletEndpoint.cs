namespace WalletService.Features.CreateWallet;

public static class CreateWalletEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/wallets", async (
            CreateWalletCommand command,
            CreateWalletHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Created($"/wallets/{result.Value.WalletId}", result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "VALIDATION" => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("CreateWallet")
        .WithTags("Wallets")
        .WithSummary("Create a new wallet")
        .WithDescription("Creates a new wallet with an associated ledger account ID. The wallet is created in Active status.")
        .Produces<CreateWalletResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
